using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class MotionController : MonoBehaviour {

	public float moveSpeed = 1f,
				 maxVelocity = 1f,
				 jumpSpeed = 1f,
				 extraJumpSpeedMultiplier = 0.5f,
				 jumpTimeout = 1f,
				 extendedJumpDuration = 0.7f,
				 jumpMovementMultiplier = 1f,
				 jumpCollisionCompensation = 0.1f;
	public int maximumJumpCount = 1;

	private float jumpStartTime;
	private int jumpCount;
	private bool wasJumpingBefore;

	void Start() {
		
	}
	
	void Update() {
		var hInput = Input.GetAxis("Horizontal");
		InfluenceMovement(hInput);

		var jumpingKey = Input.GetAxis("Jump") > 0.5f;
		if (jumpingKey) {
			if (!wasJumpingBefore && jumpStartTime + jumpTimeout < Time.timeSinceLevelLoad) {
				Jump(jumpCount == 0? 1.0f : extraJumpSpeedMultiplier);
			}
			if (jumpStartTime + extendedJumpDuration > Time.timeSinceLevelLoad) {
				rigidbody2D.AddForce(Vector2.up * jumpSpeed * jumpSpeed);
			}
		}
		wasJumpingBefore = jumpingKey;
	}

	void InfluenceMovement(float hInput) {
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
		Jump(1.0f);
	}

	private void Jump(float multiplier) {
		if (jumpCount < maximumJumpCount) {
			jumpCount++;
			jumpStartTime = Time.timeSinceLevelLoad;
			if (rigidbody2D.velocity.y < 0) {
				rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
			}
			rigidbody2D.velocity += new Vector2(0, jumpSpeed * multiplier);
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag.Equals("Ground")) {
			bool isFeetTouching = collision.contacts.Select(
				c => c.normal).All(p => Vector2.Dot(p, Vector2.up) > 0.5f);
			if (isFeetTouching) {
				jumpCount = 0;
			}
		}
	}
}
