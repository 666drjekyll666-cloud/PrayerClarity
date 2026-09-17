using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class RebalancedSoulContentment
    {
        private const float StockCoefficient = 0.1f;
        private const float RebalancedCoefficient = 0.2f;
        private static ManualLogSource _log;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type wgo = R.GameType("WorldGameObject");
            MethodInfo target = R.Method(wgo, "CheckNeededAttachedScript", false, Type.EmptyTypes);
            if (target == null) throw new MissingMethodException("WorldGameObject.CheckNeededAttachedScript()");
            R.Patch(harmonyId + ".soulcontentment", typeof(RebalancedSoulContentment), target, nameof(CheckNeededAttachedScriptPostfix));
        }

        private static void CheckNeededAttachedScriptPostfix(object __instance)
        {
            try
            {
                object definition = R.Get(__instance, "obj_def");
                string attachedScript = Convert.ToString(R.Get(definition, "attached_script"));
                if (!string.Equals(attachedScript, "soul_portal", StringComparison.Ordinal)) return;

                object graph = FindSoulPortalGraph(__instance);
                if (graph == null) throw new InvalidOperationException("Live soul_portal graph was not found after attached-script initialization.");

                ApplyCoefficient(graph);
            }
            catch (Exception ex)
            {
                if (_runtimeErrorLogged) return;
                _runtimeErrorLogged = true;
                _log?.LogError("PrayerClarity: Rebalanced Soul Contentment graph projection failed closed; stock +10% behavior remains for the affected portal. " + ex);
            }
        }

        private static object FindSoulPortalGraph(object wgo)
        {
            Component component = wgo as Component;
            if (component == null) return null;

            Type controllerType = R.AnyType("FlowCanvas.FlowScriptController");
            if (controllerType == null) throw new MissingMemberException("FlowCanvas.FlowScriptController");

            MethodInfo getComponents = typeof(Component).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "GetComponentsInChildren" || m.IsGenericMethodDefinition) return false;
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 2 && p[0].ParameterType == typeof(Type) && p[1].ParameterType == typeof(bool);
                });
            if (getComponents == null) throw new MissingMethodException("Component.GetComponentsInChildren(Type,bool)");

            Array controllers = getComponents.Invoke(component, new object[] { controllerType, true }) as Array;
            if (controllers == null) return null;

            foreach (object controller in controllers)
            {
                object graph = R.Get(controller, "graph") ?? R.Get(controller, "_graph");
                if (graph == null) continue;
                string graphName = Convert.ToString(R.Get(graph, "name"));
                if (string.Equals(graphName, "soul_portal", StringComparison.Ordinal))
                    return graph;
            }
            return null;
        }

        private static void ApplyCoefficient(object graph)
        {
            List<object> connections = CollectConnections(graph);
            List<object> candidates = new List<object>();

            foreach (object connection in connections)
            {
                if (!string.Equals(Convert.ToString(R.Get(connection, "targetPortID") ?? R.Get(connection, "_targetPortID")), "b", StringComparison.Ordinal))
                    continue;

                object source = R.Get(connection, "sourceNode") ?? R.Get(connection, "_sourceNode");
                object multiply = R.Get(connection, "targetNode") ?? R.Get(connection, "_targetNode");
                if (!IsNodeType(source, "GetVariable`1") || !IsNodeType(multiply, "FloatMultiply")) continue;

                object parameter = R.Get(source, "value");
                object rawValue = parameter == null ? null : R.Get(parameter, "_value");
                float coefficient = R.Float(rawValue);
                if (Math.Abs(coefficient - StockCoefficient) > 0.0001f && Math.Abs(coefficient - RebalancedCoefficient) > 0.0001f)
                    continue;

                object playerParamSource = FindSourceNode(connections, multiply, "a");
                if (!IsNodeType(playerParamSource, "Flow_GetPlayerParam")) continue;
                if (!string.Equals(Convert.ToString(InputValue(playerParamSource, "param")), "increase_gp_gain", StringComparison.Ordinal)) continue;

                object add = FindTargetNode(connections, multiply, "b");
                if (!IsNodeType(add, "FloatAdd")) continue;
                if (Math.Abs(R.Float(InputValue(add, "a")) - 1f) > 0.0001f) continue;

                candidates.Add(source);
            }

            candidates = candidates.Distinct(ReferenceEqualityComparer.Instance).ToList();
            if (candidates.Count != 1)
                throw new InvalidOperationException("Expected exactly one Soul Contentment coefficient node; found " + candidates.Count + ".");

            object valueParameter = R.Get(candidates[0], "value");
            if (valueParameter == null) throw new MissingMemberException("Soul Contentment GetVariable<float>.value");
            float current = R.Float(R.Get(valueParameter, "_value"));
            if (Math.Abs(current - RebalancedCoefficient) <= 0.0001f) return;
            if (Math.Abs(current - StockCoefficient) > 0.0001f)
                throw new InvalidOperationException("Soul Contentment coefficient changed unexpectedly: " + current);

            R.Set(valueParameter, "_value", RebalancedCoefficient);
        }

        private static object FindSourceNode(IEnumerable<object> connections, object targetNode, string targetPort)
        {
            foreach (object connection in connections)
            {
                object target = R.Get(connection, "targetNode") ?? R.Get(connection, "_targetNode");
                if (!ReferenceEquals(target, targetNode)) continue;
                string port = Convert.ToString(R.Get(connection, "targetPortID") ?? R.Get(connection, "_targetPortID"));
                if (!string.Equals(port, targetPort, StringComparison.Ordinal)) continue;
                return R.Get(connection, "sourceNode") ?? R.Get(connection, "_sourceNode");
            }
            return null;
        }

        private static object FindTargetNode(IEnumerable<object> connections, object sourceNode, string targetPort)
        {
            foreach (object connection in connections)
            {
                object source = R.Get(connection, "sourceNode") ?? R.Get(connection, "_sourceNode");
                if (!ReferenceEquals(source, sourceNode)) continue;
                string port = Convert.ToString(R.Get(connection, "targetPortID") ?? R.Get(connection, "_targetPortID"));
                if (!string.Equals(port, targetPort, StringComparison.Ordinal)) continue;
                return R.Get(connection, "targetNode") ?? R.Get(connection, "_targetNode");
            }
            return null;
        }

        private static object InputValue(object node, string key)
        {
            IDictionary values = R.Get(node, "_inputPortValues") as IDictionary;
            if (values == null || !values.Contains(key)) return null;
            return values[key];
        }

        private static bool IsNodeType(object node, string marker)
        {
            string fullName = node?.GetType().FullName ?? string.Empty;
            return fullName.IndexOf(marker, StringComparison.Ordinal) >= 0;
        }

        private static List<object> CollectConnections(object graph)
        {
            var result = new List<object>();
            var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            for (Type type = graph.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(flags))
                    CollectEnumerable(field.GetValue(graph), result, seen);
                foreach (PropertyInfo property in type.GetProperties(flags))
                {
                    if (!property.CanRead || property.GetIndexParameters().Length != 0) continue;
                    object value;
                    try { value = property.GetValue(graph, null); } catch { continue; }
                    CollectEnumerable(value, result, seen);
                }
            }
            return result;
        }

        private static void CollectEnumerable(object value, List<object> result, HashSet<object> seen)
        {
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable == null || value is string) return;

            IEnumerator iterator;
            try { iterator = enumerable.GetEnumerator(); } catch { return; }
            while (true)
            {
                bool moved;
                try { moved = iterator.MoveNext(); } catch { break; }
                if (!moved) break;
                object item;
                try { item = iterator.Current; } catch { continue; }
                if (item == null || !item.GetType().FullName.Contains("BinderConnection") || !seen.Add(item)) continue;
                result.Add(item);
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
