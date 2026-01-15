using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.verbosity
{

	public interface iVerbose
	{

		public enum VerbLevel
		{
			none = 0,
			verbose = 1,
			deep = 2,
		}

		public bool isVerbose(VerbLevel lvl);
		public string stamp();
	}

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
		ui = 1 << 7,

		shader = 1 << 8,
		all = ~0,
	}

	/// <summary>
	/// a manager to toggle sections of verbosity
	/// </summary>
	static public class Verbosity
	{
		public const string _ppref_prefix = "ppref_";

		public const string _tab = "   ";

		public const string color_pink_light = "ec3ef2"; // input
		public const string color_green_light = "7df27f"; // flow
		public const string color_red_light = "f23e3e"; // issue
		public const string color_blue_light = "3e83f2"; // app

		/// <summary>
		/// Enum type & bitmask
		/// contains state of each enum for builds
		/// in editor everything is also saved in edppref
		/// </summary>
		static Dictionary<Type, int> toggles = new();

		/// <summary>
		/// typeof(enum) => Enum
		/// </summary>
		static public Enum getMaskEnum(Type enumType)
		{
			return (Enum)Enum.ToObject(enumType, 0);
		}

		static public Enum convertIntToEnum(Type tEnum, int value)
		{
			return (Enum)Enum.ToObject(tEnum, value);
		}

		/// <summary>
		/// Enum => int
		/// </summary>
		static int getMaskInt(Enum enType)
		{
			return Convert.ToInt32(enType);
		}

		static public int getToggleValue(Enum en) => getToggleValue(en.GetType());
		static public int getToggleValue(Type t)
		{
			checkKey(t);
			return toggles[t];
		}

		/// <summary>
		/// reset all enums to 0
		/// </summary>
		static public void clear()
		{
			foreach(var to in toggles)
			{
				Enum val = (Enum)Enum.ToObject(to.Key, 0);
				Verbosity.toggle(Verbosity.getMaskEnum(to.Key));
			}
		}

		/// <summary>
		/// clear single channel
		/// </summary>
		static public void clear(Type eType)
		{
			if(toggles.ContainsKey(eType))
			{
				toggles[eType] = 0;
			}
		}

		/// <summary>
		/// set enum to single value
		/// </summary>
		static public void toggle(Enum flag)
		{
			// todo : don't replace, overlap

			toggles[flag.GetType()] = getMaskInt(flag);
			//Debug.Log("toggle	" + flag + " & " + toggles[flag.GetType()]);
			save(flag);
		}

		/// <summary>
		/// check if value is toggled:on
		/// </summary>
		static public bool isToggled(Enum enumSpecificValue)
		{
			Type t = enumSpecificValue.GetType();

			int local = getToggleValue(t);

			// dico/pref stored value
			var dVal = (Enum)Enum.ToObject(t, local);

			// given filter
			var fVal = (Enum)Enum.ToObject(t, enumSpecificValue);

			return dVal.HasFlag(fVal);
		}

		/// <summary>
		/// make sure key is part of toggles[]
		/// </summary>
		static void checkKey(Type enumType)
		{
			if (!toggles.ContainsKey(enumType))
			{
				toggles.Add(enumType, load(enumType));
				//Debug.Log("load	#" + toggles[enumType]);
			}
		}

		static void save(Enum flag)
		{
#if UNITY_EDITOR
			Type t = flag.GetType();
			int sVal = (int)Enum.ToObject(t, flag);

			EditorPrefs.SetInt(_ppref_prefix + t.ToString(), sVal);
			//Debug.Log("save	#" + flag.GetType() + "=" + flag + " & " + sVal);
#endif
		}

		/// <summary>
		/// only extract data from ppref
		/// build : always 0
		/// </summary>
		static int load(System.Type enumType)
		{
			int val = 0;

#if UNITY_EDITOR
			val = EditorPrefs.GetInt(_ppref_prefix + enumType.ToString(), 0);
#endif

			return val;
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
			=> logCategory("app", context, msg, color_blue_light);
		
		/// <summary>
		/// unfiltered log, visible in build
		/// major game flow event
		/// </summary>
		static public void logFlowPillar(string context, string msg)
			=> logCategory("flow", context, msg, color_green_light);

		static public void logInput(string context, string msg) 
			=> logCategory("input", context, msg, color_pink_light);

		/// <summary>
		/// unfiltered log, visible in build
		/// major game flow event
		/// </summary>
		static public void logIssue(string context, string msg)
			=> logCategory("issue", context, msg, color_red_light);

		static void logCategory(string category, string context, string msg, string color)
		{
			msg = wrapHexColor(category+"." + context, color) + _tab + _tab + msg;
			VerbosityBroadcast.logEvent(category, msg);
			ulog(msg);
		}

		static public void logNone(string content, object context = null, string hex = null)
			=> logEnum(VerbositySectionUniversal.none, content, context, hex);

		/// <summary>
		/// log universal
		/// </summary>
		static public void logUniversal(VerbositySectionUniversal section, string content, object context = null, string hex = null)
			=> logEnum(section, content, context);

		static public void logFilter(Enum enumValue, string content, object context = null, string hex = null)
			=> logEnum(enumValue, content, context, hex);

		static void logEnum(Enum enumValue, string msg, object context = null, string hex = null)
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

		static void ulog(string msg, object tar = null)
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

#if UNITY_EDITOR
		[MenuItem("Window/Verbosity/test logs")]
		static void miTestLogs()
		{
			Verbosity.logApp("app", "things to say");
			Verbosity.logFlowPillar("flow", "things to say");
			Verbosity.logIssue("issue", "things to say");
		}
#endif
	}

	static public class VerbosityBroadcast
	{

		/// <summary>
		/// sub a callback to an enum to receive matching logs
		/// </summary>
		static Dictionary<string, Action<string>> eventBroadcast = new();

		static public void subLogMessage(string category, Action<string> callback)
		{
			if (!eventBroadcast.ContainsKey(category))
			{
				eventBroadcast.Add(category, null);
			}

			eventBroadcast[category] += callback;
		}

		static public void unsubLogMessage(string category, Action<string> callback)
		{
			if (!eventBroadcast.ContainsKey(category)) return;
			eventBroadcast[category] -= callback;
		}

		static public void logEvent(string category, string msg)
		{
			if(eventBroadcast.ContainsKey(category))
			{
				eventBroadcast[category]?.Invoke(msg);
			}
		}
	}

}

