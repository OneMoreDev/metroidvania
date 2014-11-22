using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using JsonFx.Json;
using UnityEngine;

/// <summary>
/// Extended functionality wrapper for Unity's Input class.
/// Handles controllers in different platforms in a unified fashion.
/// It is preferred to use this class instead of UnityEngine.Input.
/// 
/// Requires all joystick axes up to 8 configured in Input Manager
/// with names Joy[x], where x is the name of the axis configured.
/// 
/// Platform controller reference cheatsheets (Win, Mac, Linux) in that order:
/// http://wiki.unity3d.com/index.php?title=Xbox360Controller
/// </summary>
public class JoyInput {

	/// <summary>
	/// The controller axis dispatch table.
	/// Keys are axis names, funcs return the value of that axis
	/// </summary>
	private static Dictionary<string, Func<float>> joyAxisDispatch;

	/// <summary>
	/// The controller button dispatch table.
	/// Keys are button names, funcs return their state.
	/// </summary>
	private static Dictionary<string, Func<bool>> joyButtonDispatch;

	/// <summary>
	/// The key mapping. Initialized from file at first access to this class,
	/// individual keymaps can be accessed with <see cref="GetMappingFor"/>,
	/// and redefined with <see cref="Remap()"/>.
	/// </summary>
	private static Dictionary<string, string> keyMapping;

	/// <summary>
	/// Returns the current key mapping for the given input name.
	/// </summary>
	/// <returns>The internal axis or key defined for this name</returns>
	/// <param name="axisOrKeyName">Axis or key name to retrieve.</param>
	public static string GetMappingFor(string axisOrKeyName) {
		TryInitialize();
		return keyMapping[axisOrKeyName.ToLower()].ToLower();
	}

	/// <summary>
	/// Gets the current value of the given axis.
	/// </summary>
	/// <returns>The axis value, usually between -1 and 1.</returns>
	/// <param name="axisName">Pretty axis name.</param>
	public static float GetAxis(string axisName) {
		return GetAxisRaw(GetMappingFor(axisName));
	}

	/// <summary>
	/// Gets the button state.
	/// </summary>
	/// <returns><c>true</c>, if button is down, <c>false</c> otherwise.</returns>
	/// <param name="name">Pretty name of the button.</param>
	public static bool GetButton(string name) {
		return GetButtonRaw(GetMappingFor(name));
	}

	/// <summary>
	/// Similar to <see cref="GetAxis"/>, but bypasses the keymap table
	/// </summary>
	/// <returns>The axis' raw value.</returns>
	/// <param name="axisName">Axis name.</param>
	public static float GetAxisRaw(string axisName) {
		TryInitialize();
		var keys = SplitKeymap(axisName);
		var sum = keys.Aggregate(0f, (accumulator, axis) => {
			if (joyAxisDispatch.ContainsKey(axis)) {
				accumulator += joyAxisDispatch[axis]();
			} else {
				accumulator += Input.GetAxis(axis);
			}
			return accumulator;
		});
		return Mathf.Clamp(sum, -1f, 1f);
	}

	/// <summary>
	/// Similar to <see cref="GetButton"/>, but bypasses the keymap table
	/// </summary>
	/// <returns><c>true</c>, if button button is down, <c>false</c> otherwise.</returns>
	/// <param name="name">Button name.</param>
	public static bool GetButtonRaw(string name) {
		TryInitialize();
		var keys = SplitKeymap(name);
		return keys.Any(key => {
			if (joyButtonDispatch.ContainsKey(key)) {
				return joyButtonDispatch[key]();
			}
			return Input.GetKey(key);
		});
	}

	/// <summary>
	/// Remap the specified "pretty name" to the target key or axis.
	/// </summary>
	/// <param name="name">Pretty/descriptive name.</param>
	/// <param name="target">Target key or axis.</param>
	public static void Remap(string name, string target) {
		TryInitialize();
		keyMapping[name.ToLower()] = target.ToLower();
	}

	/// <summary>
	/// Tries to initialize the class. All public functions should call this 
	/// function before accessing any functionality.
	/// 
	/// Defines dispatch tables for axis and buttons, and loads the keymap file
	/// </summary>
	private static void TryInitialize() {
		if (keyMapping == null) {
			LoadKeymap();
		}

		if (joyAxisDispatch == null) {
			joyAxisDispatch = new Dictionary<string, Func<float>>();
			joyAxisDispatch.Add("leftstickx", () => {
				return Input.GetAxis("JoyX");
			});
			joyAxisDispatch.Add("left bumper", () => {
				return 42f;
			});
		}
		if (joyButtonDispatch == null) {
			joyButtonDispatch = new Dictionary<string, Func<bool>>();
			joyButtonDispatch.Add (":a", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetKey("joystick button 0");
				#elif UNITY_STANDALONE_OSX
				return Input.GetKey("joystick button 16");
				#endif
			});
			joyButtonDispatch.Add (":x", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetKey("joystick button 2");
				#elif UNITY_STANDALONE_OSX
				return Input.GetKey("joystick button 18");
				#endif
			});
		}
	}

	/// <summary>
	/// Loads the keymapping file, keymap.json, which should be located on the Assets folder.
	/// </summary>
	private static void LoadKeymap() {
		using (var file = new StreamReader(Path.Combine(Application.dataPath, "keymap.json"))) {
			var reader = new JsonReader(file);
			keyMapping = reader.Deserialize<Dictionary<string, string>>();
		}
	}

	/// <summary>
	/// Splits a keymap definition into multiple keys.
	/// </summary>
	/// <returns>The keymap rule</returns>
	/// <param name="keymap">Equivalent keys.</param>
	private static IEnumerable<string> SplitKeymap(string keymap) {
		return new List<string>(keymap.Split('|')).Select(str => str.Trim());
	}
}

