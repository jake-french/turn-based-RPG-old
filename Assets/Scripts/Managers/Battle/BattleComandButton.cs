using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class BattleComandButton : MonoBehaviour {
	public Button myButton;
	public Text myText;
	public GameObject actorOwner;
	[HideInInspector]
	public Skill myLinkedSkill;
	public int myLinkedMenu;		//-1: null, 0: skill, 1: blackmagic, 2: whitemagic, 3: items
	public Image myIcon;
	public Battlefield bf;	

	public void buttonClicked() {
		if (bf.turnStage == TurnStage.SELECTING_ACTION || bf.turnStage == TurnStage.SELECTING_ACTION_SUBMENU) {
			bf.playerUsedCommand(this.gameObject);
		}else{
			Debug.LogWarning ("Cannot use commands unless the turn stage is selecting action!");
		}
	}
}
