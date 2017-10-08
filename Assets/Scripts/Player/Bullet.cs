using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class which manages the movement of bullets and their collision
 * 
 * 
 */
public class Bullet : MonoBehaviour {

	//Chance a bullet will kill an enemy
	[Range(0.0f,1.0f)]
	public float killchance = 0.25f;

	//Direction of travel
	private Vector3 direction;
	//Speed of travel
	public float speed = 4;
	//Whether bullet is active (gameobject is always active)
	private bool active = false;

	//Visual during flight and collision
	public GameObject flyVisual;
	public GameObject impactVisual;

	/*
	 * When we are told to fire, set ourselves as active and update our direction
	 * 
	 */ 
	public void Fire(Vector3 position, Vector3 direction)
	{
		active = true;
		//Activate flight visuals and deactivate impact visuals
		this.flyVisual.SetActive (true);
		this.impactVisual.SetActive (false);

		//Move to the start position of the gun
		this.transform.position = position;
		//Rotate ourselves in the direction
		this.transform.forward = direction.normalized;
		this.direction = direction.normalized;
	}

	public void Reset()
	{
		//On reset, simply set ourselves as deactivated and hide all visuals
		active = false;
		this.flyVisual.SetActive (false);
		this.impactVisual.SetActive (false);
	}

	public void Update()
	{
		//If we are active travel forward!
        if(active)
        {
            this.gameObject.transform.position += direction * speed * Time.deltaTime;
        }
    }


	public void OnTriggerEnter(Collider other)
	{
		//When we collide with an enemy
		if (active && other.tag == "Enemy")
		{
			Enemy e = other.gameObject.GetComponentInChildren<Enemy> ();
			if (e != null)
			{
				//Compute a cance to kill the enemy
                float rng = Random.Range(0.0f, 0.99f);
				//If we killed the enemy, notify it and explode, otherwise we go through the enemy 
				if (rng < killchance)
				{
					e.BulletHit ();
					flyVisual.SetActive (false);
					impactVisual.SetActive (true);
					active = false;
				}
			}
		} else //If we didnt hit an enemy, then we impacted something else!
		{
			flyVisual.SetActive (false);
			impactVisual.SetActive (true);
			active = false;
		}
	}
}
