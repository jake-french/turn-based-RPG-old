using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _MenuManager : MonoBehaviour {

	#region Global
	public _GameManager _gm;
	public _GUIManager _GUI;
	public MenuMode curMenu;

	void Start() {
		_gm = transform.parent.GetComponentInChildren<_GameManager>();
		_GUI = _gm._GUI;

	}

	#endregion

	/*
	#region Inventory
	public List<InventoryItem> items;

	public void addItem(Item item, int amount = 1) {
		Debug.Log ("Adding " + amount + " of " + item.name + " to inventory");
		foreach(InventoryItem i in items) {
			if (i.item == item) {
				i.count += amount;
				return;
			}
		}
		Debug.Log ("Item does not exist already in inventory!");
		InventoryItem newItem = new InventoryItem(item, amount);
	}
	#endregion
	*/

	#region Party
	//Have 3 actors for base party and do not include additional unless have spare time.
	//Setup of changing party involves work.
	//MemberOne will always be leader for current build.
	public GameObject memberOne, memberTwo, memberThree;
	public List<GameObject> possiblePartyMembers;

	public void addMember(GameObject actor) {
		possiblePartyMembers.Add (actor);
	}
	
	public void removeMember(GameObject actor) {
		possiblePartyMembers.Remove(actor);
	}
	
	public void addToActiveParty(GameObject actor, int pos) {
		if (pos == 1) {
			memberOne = actor;
		}else if (pos == 2) {
			memberTwo = actor;
		}else if (pos == 3) {
			memberThree = actor;
		}else{
			Debug.LogError ("Invalid position in player party!");
		}
	}
	
	public void clearMember(int pos) {
		if (pos == 1) {
			memberOne = null;
		}else if (pos == 2) {
			memberTwo = null;
		}else if (pos == 3) {
			memberThree = null;
		}else{
			Debug.LogError ("Invalid position in player party!");
		}
	}

	//Visually displays the party leader
	public void showPartyLeader() {
		_gm.getFieldLeader().SetActive(true);
	}

	//In case one wishes to hide the party leader from the game
	public void hidePartyLeader() {
		_gm.getFieldLeader().SetActive(false);
	}

	//Assumes leader is also to be displayed. Visually displays all party members
	public void showParty() {
		showPartyLeader();
		memberTwo.SetActive(true);
		memberThree.SetActive(true);
	}

	//Assumes leader is to stay displayed. Visually hides the additional party members
	public void hideParty() {
		memberTwo.SetActive(false);
		memberThree.SetActive(false);
	}

	public bool isInActiveParty(Actor actor) {
		if (actor == memberOne || actor == memberTwo || actor == memberThree) {
			return true;
		}else{
			return false;
		}
	}
	#endregion
}

public enum MenuMode {
	NOT_MENU,
	PAUSE_MENU,
	PARTY_MENU,
	INVENTORY_MENU
}