using UnityEngine;
using System.Collections;

public abstract class CombatController : MonoBehaviour {

	public Weapon weapon;
	public float baseAttackTime = 1f;

	private float attackStartTime;
	private bool attacking;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		if (weapon) {
			attacking = attackStartTime + (baseAttackTime / weapon.attackSpeed) > Time.timeSinceLevelLoad;
			if (attacking) {
				weapon.AttackUpdate(this);
			}
			Debug.Log("CHAAARGE");
			weapon.attacking = attacking;
		}
	}

	public void Attack() {
		if (!attacking) {
			attackStartTime = Time.timeSinceLevelLoad;
		}
	}
}
