using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Manages entry to the maze 
 * 
 */ 
[RequireComponent(typeof(BoxCollider))]
public class EntryPoint : MonoBehaviour {

    public delegate void EntryEvent();
    public static event EntryEvent OnMazeEnter;

	//Changing visual effect to cycle through
    public GameObject[] objectsToCycleThrough;
	//Which visual effect we've currently activated
	private GameObject currentGameObject;

	//terrain collider to disable so player can go through the terrain into the maze
	public TerrainCollider terrainCollider;
	//Any extra gameobjects to disable when player enters the maze
	public GameObject[] toDisable;

	//Manage how many times player has to enter the object to enter the maze
	[Range(1,10)]
	public int requiredNumEntires = 1;
	private int numEntered = 0;

	public void Start()
	{
		currentGameObject = objectsToCycleThrough [0];
	}
		
	public void OnTriggerEnter(Collider enter)
	{
        //When player enters
		if (enter.tag == "Player")
		{
			//Increment number of times player has entered
			if ((++numEntered) >= requiredNumEntires)
			{
				//If it exceeds the required number, disable current visual effect
				this.gameObject.SetActive (false);
				//Disable the terrain collider 
				if (terrainCollider != null)
				{
					terrainCollider.enabled = false;
					//Disable all other gameobjects specified
					foreach (GameObject g in toDisable)
					{
						g.SetActive (false);
					}
				}
				//Notify world via events that we've entered the maze
                if(OnMazeEnter!=null)
                {
                    OnMazeEnter();
                }
			} else
			{
				//Otherwise cycle through the visuals of the object, disabling current visual enabling the next
				currentGameObject.SetActive (false);
				currentGameObject = objectsToCycleThrough [numEntered % objectsToCycleThrough.Length];
				currentGameObject.SetActive (true);
			}
		}
	}
}
