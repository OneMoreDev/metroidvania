using UnityEngine;
using System.Collections;

public class MotionController : MonoBehaviour {

	public float moveSpeed = 1f;
	public float jumpSpeed = 1f;
	public int maximumJumpCount = 1;
	public string leftKey, rightKey, jumpKey;

	private float jumpStartTime;
	private int jumpCount;

	void Start() {
	
	}
	
	void Update() {
		var hInput = (Input.GetKey(leftKey)? -1 : 0) + (Input.GetKey(rightKey)? 1 : 0);
		var jumpingKey = Input.GetKeyDown(jumpKey);
		transform.position += new Vector3(hInput * moveSpeed * Helpers.TimeScale(), 0, 0);

		if (jumpingKey && jumpCount < maximumJumpCount) {
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
