using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PrayerClarity
{
    internal static class R
    {
        internal static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        internal static Assembly GameAssembly;
        private static MethodInfo _vanillaLocalizeMethod;
        private static bool _vanillaLocalizeResolved;

        internal static bool BindGameAssembly()
        {
            if (GameAssembly != null) return true;
            GameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
            return GameAssembly != null;
        }

        internal static Type GameType(string name)
        {
            if (GameAssembly == null) return null;
            Type exact = GameAssembly.GetType(name, false);
            if (exact != null) return exact;
            try { return GameAssembly.GetTypes().FirstOrDefault(x => x != null && x.Name == name); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.FirstOrDefault(x => x != null && x.Name == name); }
        }

        internal static Type AnyType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(name, false) ?? assembly.GetTypes().FirstOrDefault(x => x != null && x.Name == name);
                    if (type != null) return type;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type type = ex.Types.FirstOrDefault(x => x != null && x.Name == name);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        internal static MethodInfo Method(Type type, string name, bool isStatic, int parameterCount)
        {
            if (type == null) return null;
            return type.GetMethods(isStatic ? Stat : Inst)
                .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == parameterCount);
        }

        internal static MethodInfo Method(Type type, string name, bool isStatic, Type[] signature)
        {
            return type == null ? null : type.GetMethod(name, isStatic ? Stat : Inst, null, signature, null);
        }

        internal static object Get(object obj, string name)
        {
            if (obj == null) return null;
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) return field.GetValue(obj);
                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanRead) return property.GetValue(obj, null);
            }
            return null;
        }

        internal static object GetStatic(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, Stat);
                if (field != null) return field.GetValue(null);
                PropertyInfo property = current.GetProperty(name, Stat);
                if (property != null && property.CanRead) return property.GetValue(null, null);
            }
            return null;
        }

        internal static void Set(object obj, string name, object value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) { field.SetValue(obj, value); return; }
                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanWrite) { property.SetValue(obj, value, null); return; }
            }
            throw new MissingMemberException(obj.GetType().FullName, name);
        }

        internal static string Id(object obj) { return Get(obj, "id") as string; }

        internal static float Float(object value)
        {
            return value == null ? 0f : Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        internal static int Int(object value)
        {
            return value == null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        internal static float SmartFloat(object expression)
        {
            if (expression == null) return 0f;
            MethodInfo method = Method(expression.GetType(), "EvaluateFloat", false, 2);
            if (method == null) throw new MissingMethodException("SmartExpression.EvaluateFloat");
            return Float(method.Invoke(expression, new object[] { null, null }));
        }

        internal static object BalanceData(string id, string expectedTypeName, bool allowMissing)
        {
            Type balanceType = GameType("GameBalance");
            object balance = GetStatic(balanceType, "me");
            if (balance == null) throw new InvalidOperationException("GameBalance.me unavailable");

            Type expectedType = GameType(expectedTypeName);
            if (expectedType == null) throw new MissingMemberException("Expected balance type unavailable: " + expectedTypeName);

            string methodName = allowMissing ? "GetDataOrNull" : "GetData";
            MethodInfo method = ResolveBalanceGetter(balance.GetType(), methodName, expectedType);
            if (method == null)
                throw new MissingMethodException("GameBalanceBase." + methodName + "<" + expectedTypeName + ">(string)");

            return method.Invoke(balance, new object[] { id });
        }

        private static MethodInfo ResolveBalanceGetter(Type balanceRuntimeType, string methodName, Type expectedType)
        {
            MethodInfo[] candidates = balanceRuntimeType.GetMethods(Inst)
                .Where(m => m.Name == methodName)
                .Where(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 1 && p[0].ParameterType == typeof(string);
                })
                .ToArray();

            MethodInfo closed = candidates.FirstOrDefault(m =>
                !m.ContainsGenericParameters && expectedType.IsAssignableFrom(m.ReturnType));
            if (closed != null) return closed;

            MethodInfo generic = candidates.FirstOrDefault(m =>
                m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);
            if (generic == null) return null;

            MethodInfo bound = generic.MakeGenericMethod(expectedType);
            return bound.ContainsGenericParameters ? null : bound;
        }

        internal static float PlayerParam(string param, float fallback = 0f)
        {
            object mainGame = GetStatic(GameType("MainGame"), "me");
            object player = mainGame == null ? null : Get(mainGame, "player");
            if (player == null) return fallback;

            MethodInfo getParam = Method(player.GetType(), "GetParam", false, new[] { typeof(string), typeof(float) });
            if (getParam == null) throw new MissingMethodException("WorldGameObject.GetParam(string,float)");
            return Float(getParam.Invoke(player, new object[] { param, fallback }));
        }

        internal static float ZoneQuality(string zoneId)
        {
            Type worldZoneType = GameType("WorldZone");
            MethodInfo getZone = Method(worldZoneType, "GetZoneByID", true, new[] { typeof(string), typeof(bool) });
            if (getZone == null) throw new MissingMethodException("WorldZone.GetZoneByID(string,bool)");

            object zone = getZone.Invoke(null, new object[] { zoneId, false });
            if (zone == null) throw new InvalidOperationException("World zone unavailable: " + zoneId);

            MethodInfo getQuality = Method(zone.GetType(), "GetTotalQuality", false, 0);
            if (getQuality == null) throw new MissingMethodException("WorldZone.GetTotalQuality()");
            return Float(getQuality.Invoke(zone, null));
        }

        internal static float GameResGet(object gameRes, string key)
        {
            if (gameRes == null) return 0f;
            MethodInfo method = Method(gameRes.GetType(), "Get", false, new[] { typeof(string), typeof(float) });
            if (method == null) throw new MissingMethodException("GameRes.Get(string,float)");
            return Float(method.Invoke(gameRes, new object[] { key, 0f }));
        }

        internal static string CurrentLanguage()
        {
            object value = GetStatic(GameType("GameSettings"), "_cur_lng");
            return value == null ? "en" : value.ToString();
        }

        internal static string VanillaLocalize(string key)
        {
            if (!_vanillaLocalizeResolved)
            {
                Type type = AnyType("GJL");
                _vanillaLocalizeMethod = Method(type, "L", true, new[] { typeof(string) });
                _vanillaLocalizeResolved = true;
            }
            if (_vanillaLocalizeMethod == null) return key;
            object value = _vanillaLocalizeMethod.Invoke(null, new object[] { key });
            return value == null ? key : value.ToString();
        }

        internal static string FormatMoney(float value)
        {
            Type type = GameType("Trading");
            MethodInfo method = Method(type, "FormatMoney", true, new[] { typeof(float), typeof(bool), typeof(bool) });
            if (method == null) return value.ToString("0.##", CultureInfo.InvariantCulture);
            object formatted = method.Invoke(null, new object[] { value, true, true });
            return formatted == null ? value.ToString("0.##", CultureInfo.InvariantCulture) : formatted.ToString();
        }

        internal static string FormatSignedMoney(float value)
        {
            if (Math.Abs(value) < 0.0001f) return FormatMoney(0f);

            string formatted = FormatMoney(Math.Abs(value));
            string sign = value > 0f ? "+" : "-";
            int iconEnd = formatted.IndexOf(')');
            return iconEnd >= 0
                ? formatted.Insert(iconEnd + 1, sign)
                : sign + formatted;
        }

        internal static void Patch(string harmonyId, Type owner, MethodInfo target, string postfixName)
        {
            PatchHooks(harmonyId, owner, target, null, postfixName, null);
        }

        internal static void PatchPrefix(string harmonyId, Type owner, MethodInfo target, string prefixName)
        {
            PatchHooks(harmonyId, owner, target, prefixName, null, null);
        }

        internal static void PatchHooks(string harmonyId, Type owner, MethodInfo target, string prefixName, string postfixName, string finalizerName)
        {
            if (target == null) throw new MissingMethodException("PrayerClarity patch target");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { harmonyId });
            object prefix = CreateHarmonyMethod(harmonyMethodType, owner, prefixName);
            object postfix = CreateHarmonyMethod(harmonyMethodType, owner, postfixName);
            object finalizer = CreateHarmonyMethod(harmonyMethodType, owner, finalizerName);

            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5 && typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = prefix;
            args[2] = postfix;
            args[3] = null;
            args[4] = finalizer;
            patch.Invoke(harmony, args);
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, Type owner, string methodName)
        {
            if (string.IsNullOrEmpty(methodName)) return null;
            MethodInfo method = owner.GetMethod(methodName, Stat);
            if (method == null) throw new MissingMethodException(owner.FullName, methodName);

            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object harmonyMethod = Activator.CreateInstance(harmonyMethodType);
            FieldInfo methodField = harmonyMethodType.GetField("method", Inst);
            if (methodField == null) throw new MissingMemberException("HarmonyMethod.method");
            methodField.SetValue(harmonyMethod, method);
            return harmonyMethod;
        }
    }
}
