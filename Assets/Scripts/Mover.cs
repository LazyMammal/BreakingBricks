using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

	private AudioSource clickAudio;
	public float speed = 10f, speedBoost = 1f, rotMax = 10f;
	private bool forceSpeed = false;
	private Rigidbody2D rb;

	void Start ()
	{
		clickAudio = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody2D>();
		speed = Mathf.Abs( speed );
	}

	void Update()
	{
		if( !forceSpeed && GameManager.isLaunch )
		{
			GameManager.isLaunch = false;
			clickAudio.Play();

			forceSpeed = true;
			transform.SetParent(null);
			speedBoost = GameManager.speedBoost;
			rb.velocity = Vector2.one.normalized * speed * speedBoost;
		}
	}
	void FixedUpdate()
	{
		if( forceSpeed )
		{
			rb.velocity = rb.velocity.normalized * speed * speedBoost;
		}
	}

 	void OnCollisionEnter2D(Collision2D col2d) 
	 {
		float rotVel = 0f;
		foreach (ContactPoint2D contact in col2d.contacts) 
		{
			Debug.DrawRay(contact.point, contact.normal, Color.white, 2f);
			Debug.DrawRay(rb.position, rb.velocity, Color.yellow, 2f);
			rotVel = contact.normal.x * rb.velocity.y - contact.normal.y * rb.velocity.x;  // Z component of cross product
		}

		// check for changes to speedBoost
		speedBoost = GameManager.speedBoost;

		clickAudio.Play();

		rotVel *= rotMax * Random.Range( .5f, 1f );
		rb.angularVelocity = rotVel; // (rb.angularVelocity + rotVel) * .5f; 
    }
}
