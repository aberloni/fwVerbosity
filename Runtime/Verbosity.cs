using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.verbosity
{
    /// <summary>
    /// The following color names are supported:
    /// black, blue, green, orange, purple, red, white, and yellow.
    /// </summary>
    [System.Flags]
    public enum VerbositySectionUniversal
    {
        none = 0,
        engine = 1 << 1,
        loading = 1 << 2,

        input = 1 << 3,
        addressables = 1 << 4,
        audio = 1 << 5,
        localization = 1 << 6,

        shader = 1 << 7,
        all = ~0,
    }

    /// <summary>
    /// a manager to toggle sections of verbosity
    /// </summary>
    public class Verbosity
    {
        const string _ppref_prefix = "ppref_";

        const string _tab = "   ";

        public const string color_pink_light = "ec3ef2";
        public const string color_green_light = "7df27f";
        public const string color_red_light = "f23e3e";
        public const string color_blue_light = "3e83f2";

        // Enum type & bitmask
        static Dictionary<Type, int> toggles = new Dictionary<Type, int>();

        static public Enum getMaskEnum(Enum enumType)
        {
            int mv = getMaskInt(enumType.GetType());
            return (Enum)Enum.ToObject(enumType.GetType(), mv);
        }

        static int getMaskInt(Type enumType)
        {
            //Type t = enumType.GetType();
#if UNITY_EDITOR
            if (Application.isEditor)
                return EditorPrefs.GetInt(
                    _ppref_prefix + enumType, 0);
#endif

            checkKey(enumType);

            return toggles[enumType];
        }

        static public bool isToggled(Enum enumSpecificValue)
        {
            int local = 0;

            Type t = enumSpecificValue.GetType();

#if UNITY_EDITOR
            local = EditorPrefs.GetInt(_ppref_prefix + t.ToString(), 0);
#else
            checkKey(enumSpecificValue.GetType());
            local = toggles[t];
#endif

            // dico/pref stored value
            var dVal = (Enum)Enum.ToObject(t, local);

            // given filter
            var fVal = (Enum)Enum.ToObject(t, enumSpecificValue);

            return dVal.HasFlag(fVal);
        }

        static void checkKey(Type enumType)
        {
            if (Application.isEditor)
                return;

            // in build
            if (!toggles.ContainsKey(enumType))
            {
                toggles.Add(enumType, 0);
            }
        }

        static public void toggle(Enum flag)
        {
            Type t = flag.GetType();
            int sVal = (int)Enum.ToObject(t, flag);

#if UNITY_EDITOR
            EditorPrefs.SetInt(_ppref_prefix + t.ToString(), sVal);
            Debug.Log(" <editor< " + t.ToString() + "#" + sVal);
#else
            checkKey(flag.GetType());
            toggles[t] = sVal;

            Debug.Log(" <<< " + t.ToString() + "#" + sVal);
#endif
        }

        static string wrapHexColor(string context, string hex)
        {
            return $" <b><color=#{hex}>{context}</color></b> ";
        }

        /// <summary>
        /// unfiltered log, visible in build
        /// major app event
        /// </summary>
        static public void logApp(string context, string msg) 
            => ulog(wrapHexColor("app." + context, color_blue_light) + _tab + _tab + msg);

        /// <summary>
        /// unfiltered log, visible in build
        /// major game flow event
        /// </summary>
        static public void logFlowPillar(string context, string msg) 
            => ulog(wrapHexColor("flow." + context, color_green_light) + _tab + _tab + msg);

        /// <summary>
        /// unfiltered log, visible in build
        /// major game flow event
        /// </summary>
        static public void logIssue(string context, string msg) 
            => ulog(wrapHexColor("issue." + context, color_red_light) + _tab + _tab + msg);

        static public void logNone(string content, object context = null, string hex = null)
            => logEnum(VerbositySectionUniversal.none, content, context, hex);

        /// <summary>
        /// log universal
        /// </summary>
        static public void logUniversal(VerbositySectionUniversal section, string content, object context = null, string hex = null)
            => logEnum(section, content, context);

        static public void logFilter(Enum enumValue, string content, object context = null, string hex = null)
            => logEnum(enumValue, content, context, hex);

        static protected void logEnum(Enum enumValue, string msg, object context = null, string hex = null)
        {
            bool toggled = isToggled(enumValue);

            if (!toggled)
                return;

            if (enumValue != null)
            {
                msg = wrapHexColor(enumValue.ToString(), hex) + msg;
            }

            ulog(msg, context);
        }

        static protected void ulog(string msg, object tar = null)
        {
            string stamp = $"({Time.frameCount})" + _tab; // (fframe count)  

            UnityEngine.Object uo = tar as UnityEngine.Object;

            if (tar != null)
            {
                stamp += tar.GetType();
                if (uo != null) stamp += ":" + uo.name;
            }

            stamp += _tab; // separator

            Debug.Log(stamp + msg, uo);
        }

        [MenuItem("Window/Verbosity/test logs")]
        static protected void testLogs()
        {
            Verbosity.logApp("app", "things to say");
            Verbosity.logFlowPillar("flow", "things to say");
            Verbosity.logIssue("issue", "things to say");
        }
    }

}

