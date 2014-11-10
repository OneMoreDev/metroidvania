﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class MotionController : MonoBehaviour {

	public float moveSpeed = 1f;
	public float maxVelocity = 1f;
	public float jumpSpeed = 1f;
	public float jumpMovementMultiplier = 1f;
	public int maximumJumpCount = 1;
	public string leftKey, rightKey, jumpKey;

	private float jumpStartTime;
	private int jumpCount;

	void Start() {
		
	}
	
	void Update() {
		var hInput = (Input.GetKey(leftKey)? -1 : 0) + (Input.GetKey(rightKey)? 1 : 0);
		InfluenceMovement(hInput);

		var jumpingKey = Input.GetKeyDown(jumpKey);
		if (jumpingKey) {
			Jump();
		}
	}

	void InfluenceMovement (int hInput) {
		var adjustedMoveSpeed = hInput * moveSpeed * Helpers.TimeScale;
		if (jumpCount > 0) {
			adjustedMoveSpeed *= jumpMovementMultiplier;
		}
		var addedSpeed = new Vector2(adjustedMoveSpeed, 0);
		var newVelocity = rigidbody2D.velocity + addedSpeed;
		if (newVelocity.sqrMagnitude < (maxVelocity * maxVelocity)) {
			rigidbody2D.AddForce(addedSpeed, ForceMode2D.Impulse);
		}
	}

	private void Jump() {
		if (jumpCount < maximumJumpCount) {
			jumpCount++;
			jumpStartTime = Time.timeSinceLevelLoad;
			if (rigidbody2D.velocity.y < 0) {
				rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
			}
			rigidbody2D.velocity += new Vector2(0, jumpSpeed);
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag.Equals("Ground")) {
			jumpCount = 0;
		}
	}
}
