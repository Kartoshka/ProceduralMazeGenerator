using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Enable gameobjects and colliders when a player enters this object
 * 
 */
[RequireComponent(typeof(BoxCollider))]
public class EnableOnEnter : MonoBehaviour {


	public Collider[] colliders;
	public GameObject[] gameObjects;

	public void OnTriggerEnter(Collider other){
		if (other.tag == "Player")
		{
			foreach (Collider c in colliders)
			{
				c.enabled = true;
			}

			foreach (GameObject g in gameObjects)
			{
				g.SetActive (true);
			}
		}
		
	}
}
