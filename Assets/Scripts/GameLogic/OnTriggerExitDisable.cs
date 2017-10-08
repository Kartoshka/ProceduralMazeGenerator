using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OnTriggerExitDisable : MonoBehaviour {

	public GameObject[] disableOnExit;

	public void OnTriggerExit(Collider other){
		if (other.tag == "Player")
		{
			foreach (GameObject g in disableOnExit)
			{
				g.SetActive (false);
			}
		}
	}
}
