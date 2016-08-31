using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	int inputX = 0, gameLevel = 0;
	public float accelMax = 2f, speedMax = 3f;
	public bool useKeys = false;
	private Rigidbody2D rb; 
	private float minX = -3f, maxX = 3f;
	public GameObject playArea;
	private AudioSource clickAudio;

	void Start()
	{
		clickAudio = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody2D>();
		UpdateXsize();
	}

	void UpdateXsize()
	{
		BoxCollider2D playBox = playArea.GetComponent<BoxCollider2D>();
		BoxCollider2D box = GetComponent<BoxCollider2D>();
		float boxX = (playBox.size.x - box.size.x * transform.localScale.x ) / 2f;
		minX = playBox.offset.x - boxX;
		maxX = playBox.offset.x + boxX;
	}

	void Update () 
	{
		if( !GameManager.isGameOver && !GameManager.isPaused )
		{
			if( useKeys )
			{
				float axisX = Input.GetAxis("Horizontal");

				if( axisX < 0 || Input.GetKey(KeyCode.LeftArrow) )
				{
					// accelerate left
					inputX = -1;
				}
				else if( axisX > 0 || Input.GetKey(KeyCode.RightArrow) )
				{
					// accelerate left
					inputX = 1;
				}
				else inputX = 0;
			}
			else // use mouse
			{
				// mouse screen position
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				
				Vector3 pos = transform.position;
				pos.x = Mathf.Clamp( ray.origin.x, minX, maxX); 
				transform.position = pos;
			}

			if( gameLevel != GameManager.level )
			{
				gameLevel = GameManager.level;

				if( gameLevel > 0 )
				{
					Vector3 scale = transform.localScale; 
				
					scale.x = .1f + Mathf.Pow(.9f, gameLevel);

					transform.localScale = scale; 
				}

				UpdateXsize();
			}
		}
	}

	// called several times (physics only)
	void FixedUpdate()
	{
		if( !GameManager.isGameOver && !GameManager.isPaused )
		{
			if( useKeys )
			{
				// platform acceleration
				float velX = rb.velocity.x;
				velX = Mathf.Clamp(velX + accelMax * inputX * Time.deltaTime, -speedMax, +speedMax );
				rb.velocity = new Vector2( velX, 0f);
			}
		}
	}

/*
 	void OnCollisionEnter2D(Collision2D col2d) 
	{
		Debug.Log("platform");
		ContactPoint2D contact = col2d.contacts[0]; 
		Debug.DrawRay(contact.point, contact.normal, Color.blue, 2f);
		Vector2 vel = new Vector2( contact.point.x - transform.position.x , 1f );
		Debug.DrawRay(contact.point, vel, Color.red, 2f);
		Rigidbody2D rb = contact.otherCollider.GetComponent<Rigidbody2D>();
		rb.velocity = vel.normalized;
    }
*/
 	void OnTriggerEnter2D(Collider2D other) 
	{
		//Debug.Log("platform");
		if( other.CompareTag("Ball") )
		{
			// offset from centre of platform [-1..+1]
			BoxCollider2D box = GetComponent<BoxCollider2D>();
			float deltaX = (other.transform.position.x - transform.position.x ) / ( box.size.x * transform.localScale.x);
			Vector2 vel = new Vector2( deltaX, 1f - Mathf.Abs(deltaX) );
			Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
			rb.velocity = vel.normalized;
			Debug.DrawRay( other.transform.position, vel, Color.red, 2f );
			Debug.DrawRay( other.transform.position, new Vector2(deltaX * box.size.x, 1f - Mathf.Abs(deltaX)), Color.blue, 2f );

			clickAudio.Play();
		}
    }

}
