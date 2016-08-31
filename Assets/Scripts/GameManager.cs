using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager> {
	// singleton pattern
	protected GameManager () {} // guarantee this will be always a singleton only - can't use the constructor!
	private AudioSource scoreAudio, loseAudio, levelUpAudio;

	// board pieces
	public float brickSpacing = .1f;	
	public GameObject gridContainer, gridModel, ballModel, gameOverPanel, menuPanel, playerObject, livesContainer, playArea;
	
	private string[] brickColorLists = {
		"00FFFF", // cyan
		"0000FF", // blue 
		"FFA500", // orange
		"FFFF00", // yellow
		"80FF00", // lime
		"FF00FF", // purple
		"FF0000"  // red
	};
	private Color[] brickColorList = null;
	public Vector2 gridStep = Vector2.one, gridOrigin = Vector2.zero;

	// gameplay
	public Text scoreText, levelText, finalScoreText, finalLevelText;
	public static int brickLevelUp = 10, level = 1, ballsLeft = 3, maxBallsLeft = 3;
	private static int score = 0, brickCount = 0, screenBrickCount = 0;
	public static bool isPaused = false, isGameOver = false, isKill = false, isLaunch = false;
	public static float speedBoost = 1f, speedBoostStep = .1f;

	void Start()
	{
		AudioSource[] audio = GetComponents<AudioSource>();
		scoreAudio = audio[0];
		loseAudio = audio[1];
		levelUpAudio = audio[2];

		// create data structures (if any)
		bool success = InitBoard();
		if( success )
		{
			NewGame();
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}
	public void ResumeGame(bool pause = true)
	{
		isPaused = pause;
		menuPanel.SetActive(isPaused);
		gameOverPanel.SetActive(isGameOver && !isPaused);
		Time.timeScale = (isPaused || isGameOver) ? 0 : 1;
	}
	public void MenuButton()
	{
		//HideCursor(false);
		ResumeGame(!isPaused);
	}
	void Update()
	{
		if( scoreText != null )
			scoreText.text = "Score: " + score;

		if( levelText != null )
			levelText.text = "Level: " + screenNum; //level;

		if( Input.GetKeyDown(KeyCode.Escape) )
		{
			MenuButton();
		}

		if( Input.GetKeyDown("[") )
		{
			LevelUp();
		}
		if( Input.GetKeyDown("]") )
		{
			NewScreen();
		}

		if( (Input.GetButton("Jump") || Input.GetButton("Fire1")) && !isPaused && !isGameOver && !isKill )
		{
			// check if in play area
			BoxCollider2D playBox = playArea.GetComponent<BoxCollider2D>();

			// mouse position in world space
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			// bounding box for playarea
			Bounds box = new Bounds( playArea.transform.position + new Vector3(playBox.offset.x, playBox.offset.y, 0f), 
									 new Vector3( playBox.size.x, playBox.size.y, 1f)
									 );

			// check against bounding box 
			if( box.Contains( new Vector3(ray.origin.x, ray.origin.y, playArea.transform.position.z) ))
			{
				isLaunch = true;
				//HideCursor();
			}
		}

		// detect ball has been destroyed
		if( isKill )
		{
			isLaunch = false;

			isKill = false;
			loseAudio.Play();

			ballsLeft--;
			UpdateLives();

			if(ballsLeft <= 0)
			{
				// game over
				GameOver();
			}
			else
			{
				// spawn ball at platform 
				GameObject ball = (GameObject) Instantiate(ballModel, playerObject.transform.position + new Vector3(0f, .25f, 0f), Quaternion.identity);
				ball.transform.SetParent(playerObject.transform);
			}
		}
	}

	void HideCursor(bool hide = true)
	{
		//Cursor.lockState = hide ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = hide;
	}
	
	void UpdateLives() 
	{
		// get all sprite children
		Component[] hats = livesContainer.GetComponentsInChildren( typeof(SpriteRenderer), true );

		// draw as many hardhats as ballsLeft
		for( int i = 0; i < hats.Length; i++)
		{
			hats[i].gameObject.SetActive(i < ballsLeft );
		}
	}
	public void AddScore(int points)
	{
		scoreAudio.Play();

		score += points;

		// keep track of total bricks smashed
		brickCount++;
		if( 0 == brickCount % brickLevelUp )
		{
			LevelUp();
		}

		// how many are left on screen
		if(--screenBrickCount <= 0)
		{
			// need new screen
			NewScreen();
		}
	}

	void LevelUp()
	{
		level++;
		speedBoost += speedBoostStep;
		levelUpAudio.Play();
	}

	public void GameOver()
	{
		// game over
		finalScoreText.text = "Score: " + score;
		finalLevelText.text = "Level: " + level;

		isGameOver = true;
		menuPanel.SetActive(false);
		gameOverPanel.SetActive(isGameOver);
		Time.timeScale = 0;
	}
	// setup board for new game
	public void NewGame()
	{
		Debug.Log("new game");

		// reset game state
		score = 0;
		level = 1;
		speedBoost = 1f;
		brickCount = 0;

		ballsLeft = maxBallsLeft;
		UpdateLives();

		NewScreen();

		isLaunch = false;
		isPaused = false;
		isGameOver = false;
		menuPanel.SetActive(isPaused);
		gameOverPanel.SetActive(isGameOver);
	}
	public void NewScreen()
	{
		// suspend physics
		Time.timeScale = 0;

		// wipe board
		GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
		foreach( GameObject b in bricks )
			Destroy( b );
		screenBrickCount=0;

		GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
		foreach( GameObject b in balls )
			Destroy( b );

		// setup bricks
		BoxCollider2D gridBox = gridContainer.GetComponent<BoxCollider2D>();
		BoxCollider2D brickBox = gridModel.GetComponent<BoxCollider2D>();
		
		// size of brick
		float brickX = brickBox.size.x * brickBox.transform.localScale.x;
		float brickY = brickBox.size.y * brickBox.transform.localScale.y;

		// how many can stack in spawn zone
		int stackX = (int)Mathf.Floor( gridBox.size.x * gridBox.transform.localScale.x / (brickX + brickSpacing) );
		int stackY = (int)Mathf.Floor( gridBox.size.y * gridBox.transform.localScale.y / (brickY + brickSpacing) );

		//Debug.Log("stackX: " + stackX + " stackY: " + stackY);

		float offsetX = (stackX % 2 == 0) ? .5f : 0f;
		int half = 0;

		for(int y = -stackY/2; y < stackY-stackY/2; y++ )
		{
			float posY = gridBox.transform.position.y + y * (brickY + brickSpacing);

			for( int x = -stackX/2; x < stackX-stackX/2 - half; x++ )
			{

				float posX = gridBox.transform.position.x + (x + offsetX + .5f * half) * (brickX + brickSpacing);

				GameObject clone = (GameObject) Instantiate(gridModel, new Vector3(posX, posY, 0f), Quaternion.identity);
				clone.transform.SetParent( gridContainer.transform );

				SpriteRenderer sr = clone.GetComponentInChildren<SpriteRenderer>();
				sr.color = BrickColor(x, y, half, level);
				
				screenBrickCount++;
			}
			half = 1 - half;
		}

		// spawn ball at platform 
		GameObject ball = (GameObject) Instantiate(ballModel, playerObject.transform.position + new Vector3(0f, .25f, 0f), Quaternion.identity);
		ball.transform.SetParent(playerObject.transform);

		// "Begin" button?
		Time.timeScale = 1;
	}

	private static int screenNum = 0, prevLevel = -1;
	private static Color brickCol;
	Color BrickColor( int x, int y, int half, int level )
	{
		if(prevLevel != level)
		{
			screenNum++;
			prevLevel = level;
			brickCol = Color.HSVToRGB(
				Random.Range( 0f, 1f),
				Random.Range(.9f, 1f),
				1f
				);
			
		}
		Color newCol;

		switch(screenNum % 3 + 1)
		{
			case 2:			
				newCol = Random.Range(.75f, .95f) * brickColorList[(y*y + screenNum + 1) % brickColorList.Length];
				break;
			case 3:			
				newCol = Random.Range(.85f, .95f) * brickCol;
				break;
			default:
				newCol = Random.Range(.75f, .95f) * brickColorList[Random.Range(0, brickColorList.Length)];
				break;
		}
		return newCol;
	}

	// initialize board
	bool InitBoard()
	{
		// don't init structures twice
		if( brickColorList != null)
			return false;

		// decode block colors
		brickColorList = new Color[brickColorLists.Length];
		int idx = 0;
		foreach( string s in brickColorLists )
		{
			Color newCol = new Color( 	// "RRGGBB" hex color
				(float)System.Convert.ToInt32( s.Substring(0, 2), 16 ) / 255f, // red 0..1 
				(float)System.Convert.ToInt32( s.Substring(2, 2), 16 ) / 255f, // grn 0..1
				(float)System.Convert.ToInt32( s.Substring(4, 2), 16 ) / 255f  // blu 0..1
			);
			brickColorList[idx++] = newCol;
		}
		return true;
	}
}

