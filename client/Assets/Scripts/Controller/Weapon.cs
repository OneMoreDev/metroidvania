using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {
	public int damage;
	public float attackSpeed = 1f;

	public bool attacking {get; set;}

	public abstract void AttackUpdate(CombatController controller);
}