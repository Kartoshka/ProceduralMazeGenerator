using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

	//Keep track of a location to fire the gun out of
	public Transform shootLocation;
	//Prefab of the bullet we will fire
	public Bullet bulletPrefab;
	//Batched instance of the bullet we are firing
	private Bullet bulletInstance;

	//When told to fire
	//NOTE: If we repeatdly fire, it will continue to reset the bullet, this is intentional and there is no timer by design
	public void Fire()
	{
		//If there is no instance of the bullet, spawn one
		if (bulletInstance == null)
		{
			bulletInstance = Instantiate (bulletPrefab, shootLocation.position, this.gameObject.transform.rotation);
		}
		//Reset the bullet as there can only be one instance flying at any point
		bulletInstance.Reset ();
		//Tell the bullet to fire in a direction
		bulletInstance.Fire (shootLocation.position, shootLocation.forward);
	}
}
