using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class RebalancedRepose
    {
        private enum PendingMode
        {
            None,
            HalfwayToBest,
            Best
        }

        private static ManualLogSource _log;
        private static PendingMode _pendingMode;
        private static MethodInfo _findBuffById;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type dropType = R.AnyType("FlowCanvas.Nodes.Flow_DropBody");
            if (dropType == null) throw new MissingMemberException("FlowCanvas.Nodes.Flow_DropBody");

            Type[] callbackTypes = dropType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(type => type.GetFields(R.Inst).Any(field => field.FieldType == dropType))
                .ToArray();
            if (callbackTypes.Length != 1)
                throw new InvalidOperationException("Expected one Flow_DropBody execution display class, found " + callbackTypes.Length + ".");

            MethodInfo[] callbackMethods = callbackTypes[0].GetMethods(R.Inst)
                .Where(method => method.Name.IndexOf("<RegisterPorts>b__", StringComparison.Ordinal) >= 0)
                .Where(method => method.GetParameters().Length == 1)
                .ToArray();
            if (callbackMethods.Length != 1)
                throw new InvalidOperationException("Expected one Flow_DropBody execution callback, found " + callbackMethods.Length + ".");

            Type gameSave = R.GameType("GameSave");
            MethodInfo generateBody = R.Method(
                gameSave,
                "GenerateBody",
                false,
                new[] { typeof(int), typeof(int), typeof(int), typeof(int) });
            if (generateBody == null)
                throw new MissingMethodException("GameSave.GenerateBody(int,int,int,int)");

            ResolveBuffApi();

            R.PatchHooks(
                harmonyId + ".repose.callback",
                typeof(RebalancedRepose),
                callbackMethods[0],
                nameof(DropBodyCallbackPrefix),
                nameof(DropBodyCallbackPostfix),
                nameof(DropBodyCallbackFinalizer));
            R.PatchPrefix(
                harmonyId + ".repose.generate",
                typeof(RebalancedRepose),
                generateBody,
                nameof(GenerateBodyPrefix));
        }

        private static void DropBodyCallbackPrefix(object __instance)
        {
            _pendingMode = PendingMode.None;

            try
            {
                object node = FindCapturedDropNode(__instance);
                if (!IsOrdinaryDonkeyDeliveryNode(node)) return;
                if (!HasLiveBuff("buff_skull")) return;

                int tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ReposeTierParam);
                if (tier == 2)
                    _pendingMode = PendingMode.HalfwayToBest;
                else if (tier == 3)
                    _pendingMode = PendingMode.Best;
            }
            catch (Exception ex)
            {
                _pendingMode = PendingMode.None;
                LogRuntimeError("Repose delivery predicate failed closed", ex);
            }
        }

        private static void GenerateBodyPrefix(ref int tier_min, ref int tier_max)
        {
            PendingMode mode = _pendingMode;
            _pendingMode = PendingMode.None;
            if (mode == PendingMode.None) return;

            try
            {
                if (!HasLiveBuff("buff_skull")) return;
                if (tier_max < tier_min) return;

                bool forceBest = mode == PendingMode.Best ||
                                 (mode == PendingMode.HalfwayToBest && UnityEngine.Random.Range(0, 2) == 0);
                if (forceBest)
                    tier_min = tier_max;
            }
            catch (Exception ex)
            {
                LogRuntimeError("Repose tier narrowing failed closed", ex);
            }
        }

        private static void DropBodyCallbackPostfix()
        {
            _pendingMode = PendingMode.None;
        }

        private static void DropBodyCallbackFinalizer()
        {
            _pendingMode = PendingMode.None;
        }

        private static bool IsOrdinaryDonkeyDeliveryNode(object node)
        {
            if (node == null) return false;

            object graph = R.Get(node, "graph");
            string graphName = Convert.ToString(R.Get(graph, "name"));
            if (!string.Equals(graphName, "npc_donkey", StringComparison.Ordinal)) return false;

            object wgo = R.Get(node, "wgo");
            string objectId = Convert.ToString(GetAny(wgo, "obj_id", "_obj_id"));
            if (!string.Equals(objectId, "donkey", StringComparison.Ordinal)) return false;

            object tierMinSource;
            object tierMaxSource;
            object soulMinSource;
            object soulMaxSource;
            if (!TryGetUniqueSource(node, "Tier min", out tierMinSource)) return false;
            if (!TryGetUniqueSource(node, "Tier max", out tierMaxSource)) return false;
            if (!TryGetUniqueSource(node, "Tier min(soul)", out soulMinSource)) return false;
            if (!TryGetUniqueSource(node, "Tier max(soul)", out soulMaxSource)) return false;

            return IsIntegerAddFromPlayerParams(tierMinSource) &&
                   IsIntegerAddFromPlayerParams(tierMaxSource) &&
                   IsPlayerParamNode(soulMinSource) &&
                   IsPlayerParamNode(soulMaxSource);
        }

        private static bool TryGetUniqueSource(object node, string targetPortId, out object sourceNode)
        {
            sourceNode = null;
            IEnumerable connections = R.Get(node, "inConnections") as IEnumerable;
            if (connections == null) return false;

            int matches = 0;
            foreach (object connection in connections)
            {
                string target = Convert.ToString(GetAny(connection, "targetPortID", "_targetPortID"));
                if (!string.Equals(target, targetPortId, StringComparison.Ordinal)) continue;

                matches++;
                sourceNode = GetAny(connection, "sourceNode", "_sourceNode");
            }

            return matches == 1 && sourceNode != null;
        }

        private static bool IsIntegerAddFromPlayerParams(object sourceNode)
        {
            if (sourceNode == null) return false;
            string typeName = sourceNode.GetType().FullName ?? string.Empty;
            string nodeName = Convert.ToString(R.Get(sourceNode, "name"));
            if (typeName.IndexOf("IntegerAdd", StringComparison.Ordinal) < 0 &&
                !string.Equals(nodeName, "+", StringComparison.Ordinal))
                return false;

            IEnumerable connections = R.Get(sourceNode, "inConnections") as IEnumerable;
            if (connections == null) return false;

            int playerParamSources = 0;
            int total = 0;
            foreach (object connection in connections)
            {
                total++;
                object upstream = GetAny(connection, "sourceNode", "_sourceNode");
                if (IsPlayerParamNode(upstream)) playerParamSources++;
            }

            return total == 2 && playerParamSources == 2;
        }

        private static bool IsPlayerParamNode(object node)
        {
            if (node == null) return false;
            string typeName = node.GetType().FullName ?? string.Empty;
            return typeName.IndexOf("Flow_GetPlayerParamInt", StringComparison.Ordinal) >= 0;
        }

        private static object FindCapturedDropNode(object callback)
        {
            if (callback == null) return null;
            Type dropType = R.AnyType("FlowCanvas.Nodes.Flow_DropBody");
            if (dropType == null) return null;

            FieldInfo[] fields = callback.GetType().GetFields(R.Inst)
                .Where(field => field.FieldType == dropType)
                .ToArray();
            return fields.Length == 1 ? fields[0].GetValue(callback) : null;
        }

        private static object GetAny(object obj, params string[] names)
        {
            if (obj == null) return null;
            foreach (string name in names)
            {
                object value = R.Get(obj, name);
                if (value != null) return value;
            }
            return null;
        }

        private static void ResolveBuffApi()
        {
            if (_findBuffById != null) return;
            Type buffsLogics = R.GameType("BuffsLogics");
            _findBuffById = R.Method(buffsLogics, "FindBuffByID", true, new[] { typeof(string) });
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");
        }

        private static bool HasLiveBuff(string buffId)
        {
            ResolveBuffApi();
            return _findBuffById.Invoke(null, new object[] { buffId }) != null;
        }

        private static void LogRuntimeError(string context, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;
            _log?.LogError("PrayerClarity: Rebalanced " + context + ". Vanilla corpse generation remains active for the affected call. " + ex);
        }
    }
}
