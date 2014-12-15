using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class CombatController : MonoBehaviour {

	public Weapon weapon;
	public float baseAttackTime = 1f;
	public int health { get; private set; }
	public int maxHealth { get; set; }

	private float attackStartTime;
	private bool attacking;

	public virtual void DealDamage(int amount) {
		health -= amount;
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		if (weapon) {
			attacking = attackStartTime + (baseAttackTime / weapon.attackSpeed) > Time.timeSinceLevelLoad;
			weapon.attacking = attacking;
			if (attacking) {
				weapon.AttackUpdate(this);
			}
		}
	}

	public void Attack() {
		if (!attacking) {
			attackStartTime = Time.timeSinceLevelLoad;
		}
	}
}
