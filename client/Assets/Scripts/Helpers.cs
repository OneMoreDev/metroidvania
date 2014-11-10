using UnityEngine;
using System.Collections;

public class Helpers {
	public static float TimeScale {
		get {
			return Time.timeScale * Time.deltaTime;
		}
	}
}

