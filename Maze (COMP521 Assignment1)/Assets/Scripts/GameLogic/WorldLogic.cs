using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

/*
 * World logic takes care of all the overall logic of the game (start game, end game, victory, death, repositioning somethings, disabling others)
 * 
 */
public class WorldLogic : MonoBehaviour {

	//The maze
    public AbMazeManager maze;
	//The above terrain
    public GameObject terrain;
	//A point to align the terrain with the maze when the player wins
    public Transform terrainEndingAlign;

	//The entry of the maze 
    public Transform entryPoint;

	public GameObject[] enableOnMazeClear;
	public GameObject[] disableOnMazeClear;


    public GameObject winUI;
    public GameObject deathUI;

    public Material winSkybox;
    public void OnEnable()
    {
		//Register to all the global events
		UnlockDoor.OnDoorCleared += MazeCleared;
		Player.OnPlayerDie += OnPlayerDeath;
        EntryPoint.OnMazeEnter += OnPlayerEnterMaze;
        FinalKey.OnFinalKeyPickup += OnWin;

		//Initialize the maze
        maze.Init();
		//Generate the maze 
        maze.GenerateMaze();
		//Instatiate the maze (no exit corridor yet)
        maze.InstantiateMaze();
		//Reposition the maze to be in front of the entry corridor
        if(entryPoint!=null)
        {
            maze.SetEntrance(entryPoint.position);
        }
		//Spawn enemies (not active)
        maze.SpawnEnemies();
		//Spawn keys
		maze.SpawnKeys ();

    }
		
    public void MazeCleared()
    {
		//When the maze is cleared, we reposition the terrain based on the aligned object we specify in the world in order to have the player emerge somewhere nice without weird collisions (and on the terrain)
		if(terrain!=null && terrainEndingAlign!=null && maze != null)
        {
			//So we dont shift vertically
			Vector3 alignPos = terrainEndingAlign.position;
			alignPos.y = 0;
			Vector3 mazeAlignPos = maze.GetExitPositionAlign ();
			mazeAlignPos.y = 0;
			terrain.transform.position += (mazeAlignPos - alignPos );
        }

		//Disable any objects we want to deactivate
		foreach (GameObject g in disableOnMazeClear)
		{
			g.SetActive (false);
		}

		//Enable some objects we want to enable
		foreach (GameObject g in enableOnMazeClear)
		{
			g.SetActive (true);
		}
    }

	//When the player dies, enable the death UI
	public void OnPlayerDeath()
	{
        deathUI.SetActive(true);
    }

	//When the player wins enable the win UI and change the skybox to something nicer!
    public void OnWin()
    {
        winUI.SetActive(true);
        RenderSettings.skybox = winSkybox;
    }

	//When the player enters the maze, instatiate the exit corridor (otherwise it might be visible weirdly while they are on top of the terrain)
    public void OnPlayerEnterMaze()
    {
        maze.InstantiateEnd();
    }

	//Clean up events
    private void OnDestroy()
    {
        UnlockDoor.OnDoorCleared -= MazeCleared;
        Player.OnPlayerDie -= OnPlayerDeath;
        EntryPoint.OnMazeEnter -= OnPlayerEnterMaze;
        FinalKey.OnFinalKeyPickup -= OnWin;
    }





}
