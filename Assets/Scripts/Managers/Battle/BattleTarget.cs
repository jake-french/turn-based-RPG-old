using UnityEngine;
using System.Collections;

[System.Serializable]
public class BattleTarget : System.Object {
	[HideInInspector]
	public string targetName;
	public Transform position;		//position of the actor

	public GameObject myArrow;		//reference of arrow when targeted
	public Transform arrowPos;		//position of arrow for displaying target
	public bool isTargeted;			//checks if the arrow target is above target
	
	public Transform attackPos;	//position for a melee attack to hit target
	public GameObject actor;		//instance of an object's reference
	
}
