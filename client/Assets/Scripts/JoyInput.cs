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
/// 
/// </summary>
/// <description>
/// Requires all joystick axes up to 8 configured in Input Manager
/// with names Joy[x], where x is the name of the axis configured.
/// 
/// Keys and axis names for XBox controllers:
/// Keys:
/// :a, :b, :x, :y, :right trigger, :left trigger, left bumper,
/// right bumper, up arrow, down arrow, right arrow, left arrow, back, start
/// Axes:
/// left stick x, left stick y, right stick x, right stick y,
/// bumper axis, vertical arrow axis, horizontal arrow axis
/// </description>
/// <remarks>
/// Platform controller reference cheatsheets (Win, Mac, Linux) in that order:
/// http://wiki.unity3d.com/index.php?title=Xbox360Controller
/// </remarks>
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
		try {
			return keyMapping[axisOrKeyName.ToLower()].ToLower();
		} catch {
			Debug.Log(axisOrKeyName);
			return "";
		}
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
			joyAxisDispatch.Add("left stick x", () => {
				return Input.GetAxis("JoyX");
			});
			joyAxisDispatch.Add("left stick y", () => {
				return Input.GetAxis("JoyY");
			});
			joyAxisDispatch.Add("right stick x", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy4");
				#elif UNITY_STANDALONE_OSX
				return Input.GetAxis("Joy3");
				#endif
			});
			joyAxisDispatch.Add("right stick y", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy5");
				#elif UNITY_STANDALONE_OSX
				return Input.GetAxis("Joy4");
				#endif
			});

			joyAxisDispatch.Add("bumper axis", () => {
				return (JoyInput.GetButtonRaw("right bumper")?1:0)
					 + (JoyInput.GetButtonRaw("left bumper")?-1:0);
			});

			joyAxisDispatch.Add("vertical arrow axis", () => {
				return (JoyInput.GetButtonRaw("up arrow")?1:0)
					 + (JoyInput.GetButtonRaw("down arrow")?-1:0);
			});
			joyAxisDispatch.Add("horizontal arrow axis", () => {
				return (JoyInput.GetButtonRaw("right arrow")?1:0)
					 + (JoyInput.GetButtonRaw("left arrow")?-1:0);
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
			joyButtonDispatch.Add (":b", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetKey("joystick button 1");
				#elif UNITY_STANDALONE_OSX
				return Input.GetKey("joystick button 17");
				#endif
			});
			joyButtonDispatch.Add (":x", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetKey("joystick button 2");
				#elif UNITY_STANDALONE_OSX
				return Input.GetKey("joystick button 18");
				#endif
			});
			joyButtonDispatch.Add (":y", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetKey("joystick button 3");
				#elif UNITY_STANDALONE_OSX
				return Input.GetKey("joystick button 19");
				#endif
			});

			joyButtonDispatch.Add(":right trigger", () => {
				#if UNITY_STANDALONE_WIN
				return Input.GetAxis("Joy3") < 0f;
				#elif UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
				return Input.GetAxis("Joy6") > 0f;
				#endif
			});
			joyButtonDispatch.Add(":left trigger", () => {
				#if UNITY_STANDALONE_WIN
				return Input.GetAxis("Joy3") > 0f;
				#elif UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy3") > 0f;
				#elif UNITY_STANDALONE_OSX
				return Input.GetAxis("Joy5") > 0f;
				#endif
			});

			joyButtonDispatch.Add("left bumper", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetButton("joystick button 4");
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 13");
				#endif
			});
			joyButtonDispatch.Add("right bumper", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetButton("joystick button 5");
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 14");
				#endif
			});

			joyButtonDispatch.Add("up arrow", () => {
				#if UNITY_STANDALONE_WIN 
				return Input.GetAxis("Joy7") > 0;
				#elif UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy8") > 0;
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 5");
				#endif
			});
			joyButtonDispatch.Add("down arrow", () => {
				#if UNITY_STANDALONE_WIN 
				return Input.GetAxis("Joy7") < 0;
				#elif UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy8") < 0;
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 6");
				#endif
			});
			joyButtonDispatch.Add("right arrow", () => {
				#if UNITY_STANDALONE_WIN 
				return Input.GetAxis("Joy6") > 0;
				#elif UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy7") > 0;
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 7");
				#endif
			});
			joyButtonDispatch.Add("left arrow", () => {
				#if UNITY_STANDALONE_WIN 
				return Input.GetAxis("Joy6") < 0;
				#elif UNITY_STANDALONE_LINUX
				return Input.GetAxis("Joy7") < 0;
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 8");
				#endif
			});

			joyButtonDispatch.Add("back", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetButton("joystick button 6");
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 10");
				#endif
			});
			joyButtonDispatch.Add("start", () => {
				#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				return Input.GetButton("joystick button 7");
				#elif UNITY_STANDALONE_OSX
				return Input.GetButton("joystick button 9");
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

