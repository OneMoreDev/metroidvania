using UnityEngine;
using System.Collections;

public class DebugCombatController : CombatController {
	int oldHealth = 0;
	void OnDrawGizmos() {
		if (oldHealth > health) {
			Gizmos.DrawCube(transform.position, transform.localScale);
		}
		oldHealth = health;
	}
}

