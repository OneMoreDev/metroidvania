using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MotionController : MonoBehaviour {

	public float moveSpeed = 1f,
				 maxVelocity = 1f,

				 jumpSpeed = 1f,
				 extraJumpSpeedMultiplier = 0.5f,
				 jumpTimeout = 1f,
				 extendedJumpDuration = 0.7f,
				 jumpMovementMultiplier = 1f,

				 dashTimeout = 1.5f,
				 dashDuration = 0.7f,
				 dashSpeed = 10f;

	public float horizontalDirectionIntention = 0;

	public int maximumJumpCount = 1;

	private float jumpStartTime;
	private int jumpCount;
	private bool wasJumpingBefore;

	private float dashStartTime;
	private bool wasDashingBefore;

	void Start() {
		
	}
	
	void Update() {
		var hInput = JoyInput.GetAxis("horizontal");
		InfluenceMovement(hInput);

		var jumpingKey = JoyInput.GetButton("jump");
		if (jumpingKey) {
			if (!wasJumpingBefore && jumpStartTime + jumpTimeout < Time.timeSinceLevelLoad) {
				Jump(jumpCount == 0? 1.0f : extraJumpSpeedMultiplier);
			}
			if (jumpStartTime + extendedJumpDuration > Time.timeSinceLevelLoad) {
				rigidbody2D.AddForce(Vector2.up * jumpSpeed * jumpSpeed);
			}
		}

		var dashingKey = JoyInput.GetButton("dash");
		if (dashingKey) {
			if (dashStartTime + dashTimeout < Time.timeSinceLevelLoad) {
				if (!wasDashingBefore) {
					dashStartTime = Time.timeSinceLevelLoad;
				}
			} 
			if (dashStartTime + dashDuration > Time.timeSinceLevelLoad) {
				rigidbody2D.velocity = horizontalDirectionIntention * dashSpeed * Vector2.right;
			}
		}

		wasJumpingBefore = jumpingKey;
		wasDashingBefore = dashingKey;
	}

	void InfluenceMovement(float hInput) {
		horizontalDirectionIntention = hInput;
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
		bool isFeetTouching = collision.contacts.Select(
			c => c.normal).All(p => Vector2.Dot(p, Vector2.up) > 0.5f);
		if (isFeetTouching) {
			jumpCount = 0;
		}
	}
}
