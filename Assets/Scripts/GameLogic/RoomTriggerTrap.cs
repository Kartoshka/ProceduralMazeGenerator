using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

/*
 * Trap set at the entry of a room which triggers when entered
 * Used to activate enemies
 */
[RequireComponent(typeof(BoxCollider))]
public class RoomTriggerTrap : CellInstance {

    public delegate void TrapTriggered(Cell origin);
    public static event TrapTriggered OnTrapTriggered;

	//When the player enters, notify the world via events that trap has been triggered
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            OnTrapTriggered(this.associatedCell);
            //Destroy(this);
        }
    }



}
