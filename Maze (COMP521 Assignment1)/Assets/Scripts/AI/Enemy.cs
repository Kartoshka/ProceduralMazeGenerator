using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

/*
 * Class which manages the activation (based on trap triggers) of enemies and their destruction
 * 
 */

public class Enemy : MonoBehaviour {

	//Keep track of what cell we are associated to in order to be triggered active
    public Cell triggerCell;
	//What are the visuals of the enemy which need to be active when enemy is active
    public GameObject visual;

	//Flags for state
    public bool active = false;
	protected bool dead = false;

    public void Start()
    {
        if(visual != null)
        {
            visual.SetActive(false);
        }

		//Register to trap triggered events
        RoomTriggerTrap.OnTrapTriggered += TrapTriggered;
    }

	//When a trap anywhere is triggered, see whether that cell is our source cell, if so and we are not active ACTIVATE!
    public void TrapTriggered(Cell sourceCell)
    {
        if(!active)
        {
            if(sourceCell == triggerCell)
            {
                if (visual != null)
                {
                    visual.SetActive(true);
                }
                active = true;
            }
        }
    }

	//When a bullet tells us it has hit, if we are active (because the gameobject is always enabled for event handling) then we are now dead :(
	public void BulletHit()
	{
        if(active)
        {
            this.gameObject.SetActive(false);
            active = false;
            dead = true;
        }
	}

	//When we collide with the player if we are active and not dead, kill him!
	public void OnCollisionEnter(Collision other)
	{
		if (active && !dead && other.collider.tag == "Player")
		{
			Player p = other.collider.GetComponent<Player> ();
			if (p != null)
			{
				p.Kill ();
			}
		}
	}

	//Clean up events
    private void OnDestroy()
    {
        RoomTriggerTrap.OnTrapTriggered -= TrapTriggered;
    }
}
