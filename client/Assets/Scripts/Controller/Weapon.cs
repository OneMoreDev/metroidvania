using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {
	public float damage;
	public float reach;
	public float attackSpeed;

	public bool attacking {get; set;}

	public void AttackUpdate(CombatController controller) {
		throw new System.NotImplementedException();
	}
}