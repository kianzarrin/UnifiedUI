using static KianCommons.Assertion;

namespace KianCommons {
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using ICities;
    using System.Diagnostics;
    using System.Reflection;
    using UnityEngine.SceneManagement;
    using ColossalFramework;

    internal static class StackHelpers {
        public static string ToStringPretty(this StackTrace st, bool fullPath=false, bool nameSpace=false, bool showArgs = false) {
            string ret = "";
            foreach (var frame in st.GetFrames()) {
                var f = frame.GetFileName();
                if (f == null) {
                    ret += "    at " + frame + "\n";
                    continue;
                }

                //MethodBase m = frame.GetMethod();
                //var t = m.DeclaringType;
                //var args = m.GetParameters().Select(a=>a.ToString()).ToArray();
                //var genericArgs = m.GetGenericArguments();
                //if (nameSpace)
                //    ret += t.FullName;
                //else
                //    ret += t.Name;
                //ret += "." + m.Name;
                //if (m.IsGenericMethod)
                //    ret += "<" + m.GetGenericArguments() + ">";
                //if (showArgs)
                //    ret += "(" + string.Join(", ", args) + ")";
                //else
                //    ret += "()";

                if (!fullPath) {
                    f = f.Split('\\').LastOrDefault();
                }
                var l = frame.GetFileLineNumber();
                ret += $"    at {frame.GetMethod()} in {f}:{l}\n";
            }

            return ret;
        }

    }

    internal static class EnumBitMaskExtensions {
        internal static int String2Enum<T>(string str) where T : Enum {
            return Array.IndexOf(Enum.GetNames(typeof(T)), str);
        }

        internal static T Max<T>()
            where T : Enum =>
            Enum.GetValues(typeof(T)).Cast<T>().Max();

        internal static void SetBit(this ref byte b, int idx) => b |= (byte)(1 << idx);
        internal static void ClearBit(this ref byte b, int idx) => b &= ((byte)~(1 << idx));
        internal static bool GetBit(this byte b, int idx) => (b & (byte)(1 << idx)) != 0;
        internal static void SetBit(this ref byte b, int idx, bool value) {
            if (value)
                b.SetBit(idx);
            else
                b.ClearBit(idx);
        }

        internal static T GetMaxEnumValue<T>() =>
            System.Enum.GetValues(typeof(T)).Cast<T>().Max();

        internal static int GetEnumCount<T>() =>
            System.Enum.GetValues(typeof(T)).Length;

        private static void CheckEnumWithFlags<T>() {
            // copy of:
            // private static void ColossalFramework.EnumExtensions.CheckEnumWithFlags<T>()
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
            }
            if (!Attribute.IsDefined(typeof(T), typeof(FlagsAttribute))) {
                throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
            }
        }
        private static void CheckEnumWithFlags(Type t) {
            // copy of:
            // private static void ColossalFramework.EnumExtensions.CheckEnumWithFlags<T>()
            if (!t.IsEnum) {
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", t.FullName));
            }
            if (!Attribute.IsDefined(t, typeof(FlagsAttribute))) {
                throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", t.FullName));
            }
        }

        internal static bool CheckFlags(this NetNode.Flags value, NetNode.Flags required, NetNode.Flags forbidden) =>
            (value & (required | forbidden)) == required;


        internal static bool CheckFlags(this NetSegment.Flags value, NetSegment.Flags required, NetSegment.Flags forbidden) =>
            (value & (required | forbidden)) == required;

        internal static bool CheckFlags(this NetLane.Flags value, NetLane.Flags required, NetLane.Flags forbidden) =>
            (value & (required | forbidden)) == required;

        static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;
        static bool IsPow2(long x) => x != 0 && (x & (x - 1)) == 0;

        public static IEnumerable<T> GetPow2ValuesU32<T>() where T : struct, IConvertible {
            CheckEnumWithFlags<T>();
            Array values = Enum.GetValues(typeof(T));
            foreach (object val in values) {
                if (IsPow2((ulong)val))
                    yield return (T)val;
            }
        }
        public static IEnumerable<T> ExtractPow2Flags<T>(this T flags) where T : struct, IConvertible {
            foreach(T value in GetPow2ValuesU32<T>()) {
                if (flags.IsFlagSet(value))
                    yield return value;
            }
        }

        public static IEnumerable<uint> GetPow2ValuesU32(Type enumType) {
            CheckEnumWithFlags(enumType);
            Array values = Enum.GetValues(enumType);
            foreach (object val in values) {
                if (IsPow2((uint)val))
                    yield return (uint)val;
            }
        }

        public static IEnumerable<int> GetPow2ValuesI32(Type enumType) {
            CheckEnumWithFlags(enumType);
            Array values = Enum.GetValues(enumType);
            foreach (object val in values) {
                if (IsPow2((int)val))
                    yield return (int)val;
            }
        }


        public static void DropElement<T>(this T[] array, int i) {
            int n1 = array.Length;
            T[] ret = new T[n1 - 1];
            int i1 = 0, i2 = 0;

            while (i1 < n1) {
                if (i1 != i) {
                    ret[i2] = array[i1];
                    i2++;
                }
                i1++;
            }
        }

        public static void AppendElement<T>(this T[] array, T element) {
            int n1 = array.Length;
            T[] ret = new T[n1 + 1];

            for (int i = 0; i < n1; ++i)
                ret[i] = array[i];

            ret.Last() = element;
        }

        public static ref T Last<T>(this T[] array) => ref array[array.Length - 1];
    }

    internal static class AssemblyTypeExtensions {
        internal static Version Version(this Assembly asm) =>
          asm.GetName().Version;

        internal static Version VersionOf(this Type t) =>
            t.Assembly.GetName().Version;

        internal static Version VersionOf(this object obj) =>
            VersionOf(obj.GetType());

        internal static void CopyProperties(object target, object origin) {
            var t1 = target.GetType();
            var t2 = origin.GetType();
            Assert(t1 == t2 || t1.IsSubclassOf(t2));
            FieldInfo[] fields = origin.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static void CopyProperties<T>(object target, object origin) {
            Assert(target is T, "target is T");
            Assert(origin is T, "origin is T");
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                //string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static string GetPrettyFunctionName(MethodInfo m) {
            string s = m.Name;
            string[] ss = s.Split(new[] { "g__", "|" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length == 3)
                return ss[1];
            return s;
        }

        internal static bool HasAttribute<T>(this MemberInfo member, bool inherit=true) where T: Attribute {
            var att = member.GetCustomAttributes(typeof(T), inherit);
            return !att.IsNullorEmpty();
        }
    }

    internal static class StringExtensions {
        /// <summary>
        /// returns false if string is null or empty. otherwise returns true.
        /// </summary>
        internal static bool ToBool(this string str) => !(str == null || str == "");

        internal static string STR(this object obj) => obj == null ? "<null>" : obj.ToString();

        internal static string STR(this InstanceID instanceID) =>
            instanceID.Type + ":" + instanceID.Index;

        internal static string BIG(string m) {
            string mul(string s, int i) {
                string ret_ = "";
                while (i-- > 0) ret_ += s;
                return ret_;
            }
            m = "  " + m + "  ";
            int n = 120;
            string stars1 = mul("*", n);
            string stars2 = mul("*", (n - m.Length) / 2);
            string ret = stars1 + "\n" + stars2 + m + stars2 + "\n" + stars1;
            return ret;
        }


        internal static string CenterString(this string stringToCenter, int totalLength) {
            int leftPadding = ((totalLength - stringToCenter.Length) / 2) + stringToCenter.Length;
            return stringToCenter.PadLeft(leftPadding).PadRight(totalLength);
        }

        internal static string ToSTR(this InstanceID instanceID)
            => $"{instanceID.Type}:{instanceID.Index}";

        internal static string ToSTR(this KeyValuePair<InstanceID, InstanceID> map)
            => $"[{map.Key.ToSTR()}:{map.Value.ToSTR()}]";

        internal static string ToSTR<T>(this IEnumerable<T> list)
        {
            if (list == null) return "null";
            string ret = "{ ";
            foreach (T item in list) {
                string s;
                if (item is KeyValuePair<InstanceID, InstanceID> map)
                    s = map.ToSTR();
                else
                    s = item.ToString();
                ret += $"{s}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

        internal static string ToSTR<T>(this IEnumerable<T> list, string format) {
            MethodInfo mToString = typeof(T).GetMethod("ToString", new[] { typeof(string)})
                ?? throw new Exception($"{typeof(T).Name}.ToString(string) was not found");
            var arg = new object[] { format };
            string ret = "{ ";
            foreach (T item in list) {
                var s = mToString.Invoke(item, arg);
                ret += $"{s}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }
    }

    internal static class Assertion {
        [Conditional("DEBUG")]
        internal static void AssertDebug(bool con, string m = "") => Assert(con, m);

        [Conditional("DEBUG")]
        internal static void AssertNotNullDebug(object obj, string m = "") => AssertNotNull(obj, m);

        [Conditional("DEBUG")]
        internal static void AssertEqualDebug<T>(T a, T b, string m = "")
            where T : IComparable
            => AssertEqual(a, b, m);

        [Conditional("DEBUG")]
        internal static void AssertNeqDebug<T>(T a, T b, string m = "")
            where T : IComparable
            => AssertNeq(a, b, m);

        internal static void AssertNotNull(object obj, string m = "") =>
            Assert(obj != null, " unexpected null " + m);

        internal static void AssertEqual<T>(T a, T b, string m = "") where T:IComparable=>
            Assert(a.CompareTo(b) == 0, $"expected {a} == {b} | " + m);

        internal static void AssertNeq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) != 0, $"expected {a} != {b} | " + m);

        internal static void Assert(bool con, string m = "") {
            if (!con) {
                m = "Assertion failed: " + m;
                Log.Error(m);
                throw new System.Exception(m);
            }
        }

        internal static void AssertStack() {
            var frames = new StackTrace().FrameCount;
            //Log.Debug("stack frames=" + frames);
            if (frames > 100) {
                Exception e = new StackOverflowException("stack frames=" + frames);
                Log.Error(e.ToString());
                throw e;
            }
        }
    }

    internal static class EnumerationExtensions {
        /// <summary>
        /// returns a new List of cloned items.
        /// </summary>
        internal static List<T> Clone1<T>(this IEnumerable<T> orig) where T : ICloneable =>
            orig.Select(item => (T)item.Clone()).ToList();

        /// <summary>
        /// fast way of determining if collection is null or empty
        /// </summary>
        internal static bool IsNullorEmpty<T>(this ICollection<T> a)
            => a == null || a.Count == 0;

        /// <summary>
        /// generic way of determining if IEnumerable is null or empty
        /// </summary>
        internal static bool IsNullorEmpty<T>(this IEnumerable<T> a) {
            if (a == null)
                return true;
            else if (a is ICollection collection)
                return collection.Count == 0;
            else
                return a.Count() == 0;
        }
    }

    internal static class HelpersExtensions
    {
        internal static bool InSimulationThread() =>
            System.Threading.Thread.CurrentThread == SimulationManager.instance.m_simulationThread;

        internal static bool VERBOSE = false;

        internal static bool[] ALL_BOOL = new bool[] { false, true};
         
        internal static AppMode currentMode => SimulationManager.instance.m_ManagersWrapper.loading.currentMode;
        internal static bool CheckGameMode(AppMode mode)
        {
            try
            {
                if (currentMode == mode)
                    return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// determines if simulation is inside game/editor. useful to detect hot-reload.
        /// </summary>
        internal static bool InGameOrEditor =>         
            SceneManager.GetActiveScene().name != "IntroScreen" &&
            SceneManager.GetActiveScene().name != "Startup";

        internal static bool InGame => CheckGameMode(AppMode.Game);
        internal static bool InAssetEditor => CheckGameMode(AppMode.AssetEditor);

        [Obsolete]
        internal static bool IsActive => InGameOrEditor;

        internal static bool InStartup =>
            SceneManager.GetActiveScene().name == "IntroScreen" ||
            SceneManager.GetActiveScene().name == "Startup";   


        internal static bool ShiftIsPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        internal static bool ControlIsPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        internal static bool AltIsPressed => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);


    }
}
