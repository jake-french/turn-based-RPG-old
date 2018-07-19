using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _LevelManager : MonoBehaviour {

	public _GameManager _gm;
	public List<LevelInfo> levels;
	public LevelInfo curLevel;

	//public
	
	private Vector3 loadLevelNewPos;
	private Quaternion loadLevelNewRot;

	void Start () {
		_gm = transform.parent.GetComponentInChildren<_GameManager>();

		curLevel = levels[0];
		_gm._GUI.getAudioSources();
		_gm._GUI.setLevelAudio();

		_gm.GameOver += EndGame;
	}

	void OnLevelWasLoaded(int level) {
		string myLevel = Application.loadedLevelName;
		Debug.Log ("Level loaded: " + myLevel);

		if (myLevel != "MainMenu" || myLevel != "GameOver") {
			LevelInfo loadedLevel = null;
			
			foreach(LevelInfo li in levels) {
				if (li.sceneName == myLevel) {
					loadedLevel = li;
					break;
				}
			}

			if (loadedLevel != null) {
				curLevel = loadedLevel;
				//Setup background music and sound effects
				_gm._GUI.getAudioSources();
				_gm._GUI.setLevelAudio();

				if (curLevel.type != LevelType.BATTLE) {
					Debug.Log ("Hiding additional party members");
					_gm._menu.hideParty();
					if (_gm.getFieldLeader().activeSelf == false) {
						_gm._menu.showPartyLeader();
					}
					//If field movement has been developed
					//_gm.getFieldLeader().GetComponent<FieldMovement>().setActive(true);
				}

				if ((loadLevelNewPos != null || loadLevelNewRot != null) && curLevel.type != LevelType.BATTLE) {
					_gm.getFieldLeader().transform.position = loadLevelNewPos;
					_gm.getFieldLeader().transform.rotation = loadLevelNewRot;
					Debug.Log ("Player placed at " + loadLevelNewPos + " with rotation: " + loadLevelNewRot);
				}
			}else{
				Debug.LogError("Loaded scene was not found in list of known scenes! Consider checking list!");
			}

		}else if (myLevel == "MainMenu") {
			if (_gm._GUI.fadeStatus) {
				_gm._GUI.applyFadeIn();
			}
			curLevel = levels[0];
			_gm._GUI.getAudioSources();
			_gm._GUI.setLevelAudio();
			Debug.Log ("Main Menu loaded!");
		}else if (myLevel == "GameOver") {

		}
	}

	//Change level ONLY
	public void changeLevel(string newLevel) {
		bool found = false;
		foreach(LevelInfo li in levels) {
			if (li.sceneName == newLevel) {
				found = true;
				break;
			}
		}
		if (found) {
			Debug.Log ("Loading level: " + newLevel);
			Application.LoadLevel(newLevel);
		}else{
			Debug.LogError("Could not find level called: " + newLevel);
		}
	}

	//Change level and move the player to the specific position (used by doors)
	public void changeLevel(string newLevel, Vector3 pos, Quaternion rot) {
		bool found = false;
		foreach(LevelInfo li in levels) {
			if (li.sceneName == newLevel) {
				found = true;
				break;
			}
		}
		if (found) {
			Debug.Log ("Setting spawn position as: " + pos + " and rotation as: " + rot + " for player in new level!");
			loadLevelNewPos = pos;
			loadLevelNewRot = rot;

			Debug.Log ("Loading level: " + newLevel);
			Application.LoadLevel(newLevel);
		}else{
			Debug.LogError("Could not find level called: " + newLevel);
		}
	}
	
	public LevelInfo findLevel(string name) {
		foreach(LevelInfo li in levels) {
			if (li.sceneName == name) {
				return li;
			}
		}
		return null;
	}

	public LevelInfo getLevelBattleField() {
		string lvlToLoad = "";
		if (Application.loadedLevelName != "MainMenu") {
			foreach(LevelInfo li in levels) {
				if (li.sceneName == Application.loadedLevelName) {
					if (li.type == LevelType.FIELD) {
						lvlToLoad = li.linkedScene;
					}else if (li.type == LevelType.OTHER) {
						lvlToLoad = "dev";
					}else{
						Debug.LogError ("There is no battle field for a battle field!");
						return null;
					}
				}
			}
		}else{
			lvlToLoad = "dev";
		}
		return findLevel (lvlToLoad);
	}

	public void EndGame() {
		Application.LoadLevel("GameOver");
	}

}

public enum LevelType {
	FIELD,
	BATTLE,
	OTHER
}