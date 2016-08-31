using UnityEngine;
using System.Collections;

public class BrickContact : MonoBehaviour 
{
	GameManager gm;
	public int brickPoints = 10;

	void Start()
	{
		gm = FindObjectOfType<GameManager>();
	}
 	void OnCollisionEnter2D(Collision2D col2d) 
	{
		//Debug.Log("brick");

		gm.AddScore(brickPoints);

		Destroy(transform.gameObject);
    }

}
