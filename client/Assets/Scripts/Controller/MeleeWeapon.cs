using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Collider2D))]
public class MeleeWeapon : Weapon {

	void OnCollisionEnter2D(Collision2D collision) {
		var other = collision.collider;
		var otherController = other.gameObject.GetComponent<CombatController>();
		if (otherController != null) {
			otherController.DealDamage(damage);
		}
	}

	public override void AttackUpdate(CombatController controller) {

	}

	void OnDrawGizmos() {
		if (attacking) Gizmos.DrawCube(collider2D.bounds.center, collider2D.bounds.size);
	}
}

