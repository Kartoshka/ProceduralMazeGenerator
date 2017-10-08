using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Victory key whcih wins the game when picked up
 * 
 */ 
public class FinalKey : Key {

    public delegate void PickupEvent();
    public static event PickupEvent OnFinalKeyPickup;
	//Notify world using event that player has picked up the final winning key
    public override void PickupKey()
    {
        if(OnFinalKeyPickup!=null)
        {
            OnFinalKeyPickup();
            this.gameObject.SetActive(false);
        }
    }
}
