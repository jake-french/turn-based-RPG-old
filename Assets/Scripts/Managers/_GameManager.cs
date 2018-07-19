using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the game, including creating links between individual managers and controlling input.
/// </summary>
public class _GameManager : MonoBehaviour {
	public delegate void GameEvent();
	public event GameEvent GameStart, GameOver;
	
	//Manager Variables
	public GameObject _managers;
	public _BattleManager _battle;
	public _MenuManager _menu;
	public _LevelManager _level;
	public _GUIManager _GUI;

	private Battlefield bf;

	//DEMO prefab references
	public GameObject DEBUG_party01, DEBUG_party02, DEBUG_party03;
	public Troop DEBUG_testEnemies;

	[HideInInspector]
	public bool paused;		//whether the game is paused or not

	//PLAYER STATISTICS
	public int stepCount;				//UNUSED
	public int goldGainedTotal;			//UNUSED
	public int monstersSlainTotal;		//UNUSED

	//SKILLS
	public List<Skill> allSkills;		//list containing all working skills
	public Sprite[] subMenuIcons = new Sprite[3];	//UNUSED

	//Ensures managers stay for entire game, prepares skills
	void Start() {
		DontDestroyOnLoad(_managers);
		addAllSkills();

		GameStart += startGame;
	}

	//Manages input
	void Update() {
		//Input Conditions
		if (Input.anyKeyDown) {
			//Battle Input
			if (_level.curLevel.type == LevelType.BATTLE) {
				//If a battle has won and input is pressed, return to main menu
				if (_battle.battleWon) {
					_battle.battleWon = false;
					StartCoroutine("returnToMainMenu");
				}else{
					//Input during player's turn
					if (bf.battleStage == BattleStage.TURN_DECIDING && paused == false) {
						if (Input.GetKeyDown (KeyCode.Escape) && bf.turnStage != TurnStage.AUTOMATION) {
							//Go to previous stage/menu
							if (bf.turnStage == TurnStage.SELECTING_TARGET) {
								bf.removeAllTargets();
								if (bf.cameFromSubMenu) {
									_GUI.showSubMenu(bf.getTurnOwner(), bf.lastSubMenu);
								}else{
									_GUI.showBaseMenu(bf.getTurnOwner());
								}
							}else if (bf.turnStage == TurnStage.SELECTING_ACTION_SUBMENU) {
								bf.removeAllTargets();
								_GUI.showBaseMenu(bf.getTurnOwner());
							}else if (bf.turnStage == TurnStage.SELECTING_ACTION) {
								//Pause Menu
								_GUI.triggerPauseMenu();
							}
						}else{
							//Arrow keys for switching target/enter for confirming all party/enemies/all skills.
							if (bf.turnStage == TurnStage.SELECTING_TARGET && bf.getTurnOwner().GetComponent<Actor>().isEnemy == false) {
								if (bf.getMarkedActors().Count > 0) {
									if (Input.GetKeyDown(KeyCode.Return)) {
										Debug.Log ("Skill is being executed!");
										bf.executeSkill();
									}
								}
								//Switch targets
								if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow)) {
									if (bf.curTargetType == TargetType.TARGET) {
										if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) {
											//Select first living enemy
											BattleTarget newTar = null;
											if (bf.getMarkedActors().Count == 1) {
												//get adjacent target
												if (Input.GetKeyDown(KeyCode.LeftArrow)) {
													Debug.Log ("Searching for target on left.");
													newTar = bf.getNextTarget(false, bf.usableOnDead);
												}else if (Input.GetKeyDown (KeyCode.RightArrow)) {
													Debug.Log ("Searching for target on right.");
													newTar = bf.getNextTarget(true, bf.usableOnDead);
												}
											}else{
												//No currently selected target (backup)
												newTar = bf.getLivingActor(false);
											}
											Debug.Log ("New target is: " + newTar.actor.name);
											bf.targetSingle(newTar);
										}
									}else if (bf.curTargetType == TargetType.PARTY_MEMBER) {
										//Select first living party
										BattleTarget newTar = null;
										if (bf.getMarkedActors().Count == 1) {
											if (Input.GetKeyDown(KeyCode.LeftArrow)) {
												newTar = bf.getNextTarget(false, bf.usableOnDead);
											}else if (Input.GetKeyDown (KeyCode.RightArrow)) {
												newTar = bf.getNextTarget(true, bf.usableOnDead);
											}
										}else{
											newTar = bf.getLivingActor();
										}
										Debug.Log ("New target is: " + newTar.actor.name);
										bf.targetSingle(newTar);
									}else if (bf.curTargetType == TargetType.SELF || bf.curTargetType == TargetType.ALL_PARTY || bf.curTargetType == TargetType.ALL_TARGET) {
										//Cannot change / fixed targets
									}
								}
							}
						}
					}else{
						if (Input.GetKeyDown(KeyCode.Escape)) {
							_GUI.triggerPauseMenu();
						}
					}
				}
			//UNUSED
			}else if (_level.curLevel.type == LevelType.FIELD) {
				if (Input.GetKeyDown (KeyCode.Escape)) {
					Debug.Log ("Load party menu");
					
				}
			//Show the quit menu on menus
			}else if (_level.curLevel.type == LevelType.OTHER) {
				if (Input.GetKeyDown (KeyCode.Escape)) {
					if (_level.curLevel.sceneName == "MainMenu") {
						if (_GUI.quitConfirmPanel.activeSelf) {
							_GUI.hideQuitConfirmation();
						}else{
							_GUI.showQuitConfirmation();
						}
					}
				}
			}
		}
	}

	//Find level-specific crucial manager objects
	void OnLevelWasLoaded(int level) {
		if (_level.findLevel(Application.loadedLevelName).type == LevelType.BATTLE) {
			bf = GameObject.FindGameObjectWithTag("Battlefield").GetComponent<Battlefield>();
		}
	}

	#region Public Functions

	//Game Condition Functions
	public void TriggerGameStart() {
		if (GameStart != null) {
			GameStart();
		}
	}
	
	public void TriggerGameOver() {
		if (GameOver != null) {
			GameOver();
		}
	}

	//Start game as new game
	public void startGame() {
		Debug.Log ("Game Started");
		setupParty();

		stepCount = 0;
		goldGainedTotal = 0;
		monstersSlainTotal = 0;

		_GUI.playSoundFX(_GUI.buttonSelectNoise);
		_GUI.applyFadeOut(4);

		StartCoroutine ("fadeToBattle");
	}

	//Creates a music and visual fade-to-black when leaving level
	IEnumerator fadeToBattle() {
		_GUI.applyFadeOut (4);			//applies fade-to-black
		_GUI.musicFadeOut ();			//applies music fade
		yield return new WaitForSeconds (4);		//wait
		DEMO_testBattleSystem ();		//force the battle to run
	}

	//Steps taken to fade to and from victory back to main menu
	IEnumerator returnToMainMenu() {
		_GUI.hideVictory ();
		yield return new WaitForSeconds (0.25f);
		_GUI.applyFadeOut ();
		_GUI.musicFadeOut ();
		yield return new WaitForSeconds (4);
		_level.changeLevel ("MainMenu");
	}

	//Manages the quit confirmation of game
	public void quitGame(int stage) {
		if (stage == 0) {
			//Display confirmation of quitting
			_GUI.showQuitConfirmation();

		}else if (stage == 1) {
			//Quit
			Debug.Log ("Game is quitting...");
			Application.Quit ();
		}else if (stage == 2) {
			//Return
			_GUI.hideQuitConfirmation();
		}
	}

	//reutrns the party leader - UNUSED
	public GameObject getFieldLeader() {
		return _menu.memberOne;
	}

	//DEBUG: for testing purposes triggers a battle
	public void DEMO_testBattleSystem() {
		_battle.triggerBattle(_level.curLevel);
	}

	//Other

	#endregion

	#region Private Functions
	//Creates an instance of every skill (Replace with objects for simplicity?)
	private void addAllSkills() {
		allSkills.Clear();

		//Add each skill individually
		allSkills.Add (new Attack());
		allSkills.Add (new DoubleAttack());
		allSkills.Add (new Heal());
		allSkills.Add (new Fire());
		allSkills.Add (new Shock());

		Debug.Log ("All skills have been added!");
	}

	//Creates instances of the party and calculates stats and applies skills
	private void setupParty() {
		_menu.memberOne =(GameObject) Instantiate(DEBUG_party01);
		_menu.memberTwo =(GameObject) Instantiate(DEBUG_party02);
		_menu.memberThree =(GameObject) Instantiate(DEBUG_party03);

		//Reset prefabs to Original
		_menu.memberOne.GetComponent<Actor>()._base.skills = new List<Skill>();
		_menu.memberTwo.GetComponent<Actor>()._base.skills = new List<Skill>();
		_menu.memberThree.GetComponent<Actor>()._base.skills = new List<Skill>();

		//Setup Actors
		_menu.memberOne.GetComponent<Actor>()._base.Setup();
		_menu.memberTwo.GetComponent<Actor>()._base.Setup();
		_menu.memberThree.GetComponent<Actor>()._base.Setup();

		//Set party members to persist
		DontDestroyOnLoad(_menu.memberOne);
		DontDestroyOnLoad(_menu.memberTwo);
		DontDestroyOnLoad(_menu.memberThree);
	}

	#endregion
}
