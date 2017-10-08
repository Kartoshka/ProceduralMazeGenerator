using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Class which managed the locked exit of the maze. 
 * It is unlocked by collecting all keys, which are given to the class by the maze
 * It listens to key picked up events
 * Notifies the game that the maze has been cleared in order to reposition the world and prepare for the player exiting
 */
[RequireComponent(typeof(BoxCollider))]
public class UnlockDoor : MonoBehaviour {

	//Events for world logic
	public delegate void UnlockDoorEvent();
	public static event UnlockDoorEvent OnMazeSolved;
	public static event UnlockDoorEvent OnDoorCleared;

	//The blocker is the gameobject that will block the player from exiting the maze, will be deactivated when all keys are picked up
	public GameObject blocker;

	//List of all the keys player needs to collect
	LinkedList<Key> keys = new LinkedList<Key>();

	public void Start()
	{
		//Register to events
		Key.OnKeyPickup += OnKeyPickedUp;
	}

	//Maze calls this to notify the door a key has been spawned
	public void AddKey(Key k)
	{
		keys.AddFirst (k);
	}

	public int GetRequiredKeyCount()
	{
        return keys.Count;
	}

	/*
	 * When a key is picked up, verify whether player has picked up enough keys to unlock the door
	 */
	public void OnKeyPickedUp(Key pickedup)
	{
		keys.Remove (pickedup);
		if (keys.Count == 0)
		{
			//If player has, then we can disable the gameobject blocking the way and notify the world the maze has been solved
			if (OnMazeSolved != null)
			{
				OnMazeSolved ();
			}
			if (blocker != null)
			{
				blocker.SetActive (false);
			}
		}
	}

	/*
	 *  If the player has cleared the door, notify the world that he has
	 *  Used to reposition the terrain above the maze to allow the player to properly emerge
	 */

	public void OnTriggerExit(Collider other){
		if (other.tag == "Player")
		{
			if (OnDoorCleared != null)
			{
				OnDoorCleared ();
			}

			if (blocker != null)
			{
				blocker.SetActive (true);
			}
		}
	}

	//Clean up events
	public void OnDestroy()
	{
		Key.OnKeyPickup -= OnKeyPickedUp;
	}

}
