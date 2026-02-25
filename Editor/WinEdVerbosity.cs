using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace fwp.verbosity
{
	public class WinEdVerbosity : EditorWindow
	{
		[MenuItem("Window/Verbosity/(window) verbosity")]
		static protected void initVerbosity() => EditorWindow.GetWindow<WinEdVerbosity>();

		Type[] enumTypes = null;

		private void OnEnable()
		{
			enumTypes = getInjectionCandidates().ToArray();
			//refresh(true);
		}

		private void OnFocus()
		{
			enumTypes = getInjectionCandidates().ToArray();
			//refresh(true);
		}

		/// <summary>
		/// function to override to add more types to feature
		/// </summary>
		virtual protected List<Type> getInjectionCandidates()
			=> new List<Type>() {
				typeof(VerbositySectionUniversal),
				typeof(VerbosityUnity),
			};

		/// <summary>
		/// win editor draw
		/// </summary>
		void OnGUI()
		{
			drawHeader();

			// each possible enums
			for (int i = 0; i < enumTypes.Length; i++)
			{
				// extract value
				Enum eval = Verbosity.convertIntToEnum(enumTypes[i], Verbosity.getToggleValue(enumTypes[i]));

				// draw
				Enum nv = EditorGUILayout.EnumFlagsField(enumTypes[i].Name, eval);

				if (eval != nv)
				{
					Debug.Log("enum changed	" + nv);

					Verbosity.toggle(nv);
				}
			}

			drawFooter();
		}

		virtual protected void drawHeader()
		{
			GUILayout.Label("Verbosity toggles (x" + enumTypes.Length + ")");

			if (GUILayout.Button("reset"))
			{
				for (int i = 0; i < enumTypes.Length; i++)
				{
					Enum val = (Enum)Enum.ToObject(enumTypes[i], 0);
					Verbosity.toggle(Verbosity.getMaskEnum(enumTypes[i]));
				}
			}
		}

		virtual protected void drawFooter()
		{
		}

		public static Array GetUnderlyingEnumValues(Type type)
		{
			Array values = Enum.GetValues(type);
			Type underlyingType = Enum.GetUnderlyingType(type);
			Array arr = Array.CreateInstance(underlyingType, values.Length);
			for (int i = 0; i < values.Length; i++)
			{
				arr.SetValue(values.GetValue(i), i);
			}
			return arr;
		}
	}

}
