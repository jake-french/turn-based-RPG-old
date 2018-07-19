using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _BattleManager : MonoBehaviour {
	public _GameManager _gm;
	public GameObject actionButtonPrefab;

	public float battleMoveSpeed;
	[HideInInspector]
	public bool battleWon;

	private LevelInfo previousLevel;
	private bool setupBattle = false;

	//Player Original Location
	private Vector3 orgPos;
	private Quaternion orgRot;

	//Enemy Variables
	private Troop enemies;
	private int enemyNum;

	//Battlefield Variables
	public GameObject textObject;
	public List<BattleFXAnim> fxAnims;

	private Battlefield battlefield;
	private GameObject myBattleGUI;

	void OnLevelWasLoaded(int level) {
		if (setupBattle) {
			setupField();
		}
	}

	//General

	public void triggerBattle(LevelInfo loadedLevel) {
		Debug.Log ("Battle triggered on level: " + loadedLevel.linkedScene);
		if (loadedLevel.linkedScene.Trim () == "") {
			if (loadedLevel.type == LevelType.BATTLE) {
				Debug.LogError("Battles cannot be triggered from a pre-exiting battle!");
			}else{
				Debug.LogError ("Battle cannot be triggered from field without a linked scene to load!");
			}
		}else{
			if (loadedLevel.troops.Count <= 0) {
				Debug.Log ("Using DEMO enemy set!");
				enemies = _gm.DEBUG_testEnemies;
				setupBattle = true;
				LevelInfo battleLevel = _gm._level.findLevel("dev");
				_gm._level.changeLevel(battleLevel.sceneName);
			}else{
				Debug.Log ("Finding a random enemy set to fight!");
				int index = Mathf.FloorToInt(Random.Range(0, loadedLevel.troops.Count - 1));
				enemies = loadedLevel.troops[index];
				
				setupBattle = true;
				LevelInfo battleLevel = _gm._level.findLevel(loadedLevel.linkedScene);
				_gm._level.changeLevel(battleLevel.sceneName);
			}
		}
	}

	public void savePlayerPosition(Vector3 playerPos, Quaternion playerRot) {
		orgPos = playerPos;
		orgRot = playerRot;
		Debug.Log ("Player's orginal transform was preserved!");
	}

	//setup for boss fights
	public void forceBattle() {

	}

	public void setupField() {
		battlefield = GameObject.FindGameObjectWithTag("Battlefield").GetComponent<Battlefield>();
		battlefield.setManager(this);
		battleWon = false;

		Debug.Log ("Field is being setup!");
		//spawn 
		enemyNum = enemies.monsters.Count;
		for(int i = 0; i < enemyNum; i++ ){
			battlefield.Spawn(enemies.monsters[i], i);
		}
		battlefield.prepareEnemies(enemyNum);

		_gm._menu.showParty();

		battlefield.SpawnParty(_gm._menu.memberOne, 0);
		battlefield.SpawnParty(_gm._menu.memberTwo, 1);
		battlefield.SpawnParty(_gm._menu.memberThree, 2);

		battlefield.StartCoroutine ("beginBattle");
	}

	//called when player wins the battle
	public void displayBattleResults() {

	}

	//Called by button on victory screen
	public void endBattle() {
		enemies = null;
		_gm._level.changeLevel(previousLevel.sceneName, orgPos, orgRot);
	}
	
	//Battle

	public List<Skill> getSkillsFor(GameObject actor) {
		ActorBase actorBase = actor.GetComponent<Actor>()._base;
		return actorBase.skills;
	}

	//Sets whether the playable character action menu is to display (NOT THE COMPLETE BATTLE UI)
	public void displayBattleActionsGUI(bool state = true) {
		GameObject actionsUI = GameObject.Find ("actionMenuUI");

		if (state == true) {
			_gm._GUI.showActionMenu();
		}else if (state == false){
			_gm._GUI.hideActionMenu();
		}
	}

}

public enum BattleFXType {
	NONE,
	HEAL_SINGLE,
	FIRE_SINGLE,
	FIRE_ALL,
	SHOCK_SINGLE
}
