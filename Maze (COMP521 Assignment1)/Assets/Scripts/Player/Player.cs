using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

//Player class which manages his health, picking up keys, and input for the gun
public class Player : MonoBehaviour {

	public delegate void PlayerEvent();
	public static event PlayerEvent OnPlayerDie;
	public CharacterController controller;
    public FirstPersonController fpc;
	public Gun gun;

	public void Update()
	{
		//Fire the gun 
		if (Input.GetButtonDown ("Fire1") && gun!=null)
		{
			gun.Fire ();
		}
		//Allow game restart
        if(Input.GetButtonDown("Restart"))
        {
            SceneManager.LoadScene(0);
        }

	}

	//When we encounter a key, tell it!
	public void OnTriggerEnter(Collider other)
	{
		Key key;
		if (key = other.gameObject.GetComponent<Key>())
		{
			key.PickupKey ();
		}
        
	}

	//When we are dead, disable our controller and notify event system the player has died
	public void Kill(){
		if (controller != null)
		{
			controller.enabled = false;
            fpc.enabled = false;

            if (OnPlayerDie != null)
			{
				OnPlayerDie ();
			}
		}
	}
}
