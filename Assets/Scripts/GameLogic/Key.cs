using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Key : MonoBehaviour {

	public delegate void KeyEvent(Key self);
	public static event KeyEvent OnKeyPickup;

	//When a key is picked up notify the world using events, player tells the key it picked it up
	public virtual void PickupKey()
	{
		if (OnKeyPickup != null)
		{
			OnKeyPickup (this);
			this.gameObject.SetActive (false);
		}
	} 
}
