using UnityEngine;
using System.Collections;

public class PlayerCombatController : CombatController {
	
	// Update is called once per frame
	protected override void Update () {
		if (JoyInput.GetButton("attack")) {
			Attack();
		}
		base.Update();
	}
}

