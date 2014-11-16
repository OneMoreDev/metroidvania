using UnityEngine;
using System.Collections;

public class Helpers {
	public static float TimeScale {
		get {
			return Time.timeScale * Time.deltaTime;
		}
	}

	public static void DrawRect(Vector3 p0, Vector3 p3, Color color, float duration) {
		var p1 = new Vector3(p0.x, p3.y);
		var p2 = new Vector3(p3.x, p0.y);
		
		Debug.DrawLine(p0, p1, color, duration);
		Debug.DrawLine(p1, p3, color, duration);
		Debug.DrawLine(p3, p2, color, duration);
		Debug.DrawLine(p2, p0, color, duration);
	}
}

