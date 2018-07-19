using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Battlefield : MonoBehaviour {
	public BattleTarget[] party = new BattleTarget[3];
	public BattleTarget[] enemies = new BattleTarget[5];
	public Text infoBox;

	private _BattleManager _bm;

	private char[] enemyNum = {'A', 'B', 'C', 'D', 'E'};

	private int enemyCount;
	private Queue<GameObject> turns;

	private GameObject turnOwner;
	private GameObject lastTurnOwner;
	private int partyDown;
	private int enemyDown;

    [HideInInspector]
    public BattleStage battleStage;
	[HideInInspector]
	public TurnStage turnStage;
	[HideInInspector]
	public int turnCount = 0;
	[HideInInspector]
	public bool cameFromSubMenu;
	[HideInInspector]
	public string lastSubMenu;
	[HideInInspector]
	public Skill curSkill;
	[HideInInspector]
	public List<BattleTarget> finalTargets;
	[HideInInspector]
	public TargetType curTargetType;
	[HideInInspector]
	public bool usableOnDead;

	public void setManager(_BattleManager _bm) {
		this._bm = _bm;
	}

	//Spawning

	public void SpawnParty(GameObject actor, int index) {
		if (!actor.activeSelf) {
			actor.SetActive(true);
		}
		party[index].actor = actor;

		//Set Position & Rotation
		party[index].actor.transform.position = party[index].position.transform.position;
		party[index].actor.transform.rotation = party[index].position.transform.rotation;

		party[index].actor.transform.LookAt(enemies[index].position.transform.position);
	}

	public void Spawn(GameObject monster, int index) {
		enemies[index].actor = GameObject.Instantiate(monster);
		enemies[index].actor.name = monster.GetComponent<ActorBase>().name;

		enemies[index].actor.GetComponent<Actor>().prepForBattle(this);

		//Set Position & Rotation
		enemies[index].actor.transform.position = enemies[index].position.transform.position;
		enemies[index].actor.transform.rotation = enemies[index].position.transform.rotation;

		enemies[index].actor.transform.LookAt(party[0].position.transform.position);
	}

	//Assign Characters for identifying multiple enemies of the same type
	public void prepareEnemies(int enemyCnt) {
		enemyCount = enemyCnt;

		//Setup naming convention
		string[] names = new string[enemyCount];
		System.Array.Sort<string> (names);

		int charCnt = 0;
		for(int x = 0; x < enemyCount; x++ ){
			names[x] = enemies[x].actor.name;
		}

		for (int y = 0; y < names.Length - 1; y++) {
			if (names[y] == names[y+1]) {
				names[y] = names[y] + " " + enemyNum[charCnt];
				charCnt++;
			}else{
				charCnt = 0;
			}
		}

		//Setup Statistics
		for(int x = 0; x < enemyCount; x++ ) {
			enemies[x].actor.GetComponent<Actor>()._base.Setup();
		}
	}

	//Battle

	IEnumerator beginBattle() {
		_bm._gm._GUI.getBattleGUIElements (this);
		_bm._gm._GUI.hideGenericBattleUI ();
		calculateTurns ();

		for (int i = 0; i < enemyCount - 1; i++) {
			enemies [i].actor.GetComponent<Actor> ().prepForBattle (this);
		}
		partyDown = 0;
		enemyDown = 0;
		turnCount = 0;

		if (_bm._gm._GUI.fadeStatus) {
			_bm._gm._GUI.applyFadeIn (4);
			do {
				yield return new WaitForSeconds (Time.deltaTime);
			} while (_bm._gm._GUI.fadeStatus == true);
		}
		turnOwner = turns.Peek ();
		infoBox.text = getTurnOwner().GetComponent<Actor>()._base.name + "'s turn!";
		Debug.Log ("Turn owner is " + turnOwner.name);
		battleStage = BattleStage.TURN_DECIDING;

		_bm._gm._GUI.showGenericBattleUI ();
		turnManagement();
	}

	//Manages the active battle
	public void manageBattle() {
		_bm._gm._GUI.getBattleGUIElements(this);

		calculateTurns();

		for (int i = 0; i < enemyCount - 1; i++ ) {
			enemies[i].actor.GetComponent<Actor>().prepForBattle(this);
		}

		partyDown = 0;
		enemyDown = 0;
		turnCount = 0;
		turnOwner = turns.Peek ();
		infoBox.text = getTurnOwner().GetComponent<Actor>()._base.name + "'s turn!";
		Debug.Log ("Turn owner is " + turnOwner.name);
		battleStage = BattleStage.TURN_DECIDING;

		turnManagement();

		//For skills, when particle effect is fading away get all the particles into an array,
		//lower their alpha over time and eventually when alpha is near zero, disable and delete
	}

	public void turnManagement() {
		turnCount++;
		if (partyDown != 3 && enemyDown != enemyCount) {
			if (battleStage == BattleStage.CHANGING_TURN) {
				//Turn Stage should be TURN_SET
				turnOwner = turns.Peek ();
				Debug.Log ("New turn owner is " + turnOwner.name);
				if (isEnemy(turnOwner)) {
					Debug.Log ("Enemy's Turn");
					_bm.displayBattleActionsGUI(false);
				}else{
					Debug.Log ("Player's Turn");
				}
				generatePassiveThreat();
				removeAllTargets();
				switchNextStage();
			}else if (battleStage == BattleStage.TURN_EXECUTING) {

			}else if (battleStage == BattleStage.TURN_DECIDING) {
				if (isEnemy(turnOwner)) {
					//Hide command list
					turnOwner.GetComponent<Actor>().takeTurnAI();
				}else{
					_bm.displayBattleActionsGUI();
					_bm._gm._GUI.showBaseMenu(turnOwner);

					StartCoroutine("WaitForPlayerTurn");
				}			
			}
		}else{
			Debug.Log ("Battle is over!");
			if (enemyDown == enemyCount) {
				showVictory();
			}else{
				_bm._gm.TriggerGameOver();
			}
		}
	}

	//Assumption: PARTY is always 3 people
	public void calculateTurns(BattleTarget tar = null) {
		if (tar != null) {
			Debug.Log ("One actor has been removed. Updating current list.");
			Queue<GameObject> tempQueue = new Queue<GameObject>();

			foreach(GameObject go in turns){
				if (go != tar.actor) {
					tempQueue.Enqueue(go);
				}else{
					Debug.Log ("Removed actor: " + tar.actor.GetComponent<Actor>()._base.name + " from turns list!");
				}
			}

			turns = new Queue<GameObject>();
			foreach(GameObject go in tempQueue) {
				turns.Enqueue(go);
			}
			for (int i = 0; i < turns.Count; i++) {
				GameObject reQ = turns.Dequeue();
				turns.Enqueue(reQ);
			}
		}else{
			Debug.Log ("Calculating starting turns list.");
			turns = new Queue<GameObject>();
			List<BattleTarget> actors = new List<BattleTarget>();
			if (party[0].actor.GetComponent<Actor>()._base.dead != true) {
				actors.Add (party[0]);
			}
			if (party[1].actor.GetComponent<Actor>()._base.dead != true) {
				actors.Add (party[1]);
			}
			if (party[2].actor.GetComponent<Actor>()._base.dead != true) {
				actors.Add (party[2]);
			}

			foreach(BattleTarget bt in enemies) {
				if (bt.actor != null) {
					if (bt.actor.GetComponent<Actor>()._base.dead != true) {
						actors.Add(bt);
					}
				}
			}

			//adds actors with highest speed first into queue
			int maxActors = actors.Count;

			//loop until each actor is given an 
			while (turns.Count != maxActors) {
				BattleTarget highestSpeedActor = null;

				//Get current highest speed actor
				foreach(BattleTarget bt in actors) {
					if (highestSpeedActor == null) {
						highestSpeedActor = bt;
					}else if (bt.actor.GetComponent<ActorBase>().speed.value > highestSpeedActor.actor.GetComponent<ActorBase>().speed.value) {
						highestSpeedActor = bt;
					}
				}

				//add current highest speed actor to queue
				//Debug.Log (turns.Count + " : " + highestSpeedActor.actor.name);
				turns.Enqueue(highestSpeedActor.actor);
				actors.Remove(highestSpeedActor);
			}
		}
	}

	public void playerUsedCommand(GameObject myButton) {
		//Debug.Log ("Player clicked " + myButton.GetComponent<Button>().name);
		BattleComandButton bcb = myButton.GetComponent<BattleComandButton>();
	
		if (bcb.myLinkedMenu == -1) {
			Debug.Log ("Player selected to use " + bcb.myLinkedSkill.name + " skill!");

			cameFromSubMenu = false;
			
			//Target Select
			turnStage = TurnStage.SELECTING_TARGET;
			curSkill = bcb.myLinkedSkill;
			curTargetType = curSkill.targetType;
			usableOnDead = curSkill.useOnDead;

			if (curSkill.cost < getTurnOwnerTarget().actor.GetComponent<Actor>()._base.curMana) {
				infoBox.text = curSkill.description;
				_bm._gm._GUI.hideActionMenu();
				if (curSkill.targetType == TargetType.TARGET) {
					//Debug.Log ("Skill requires enemy target. Target must be selected to confirm!");
					targetSingle (getLivingActor(false));
				}else if (curSkill.targetType == TargetType.PARTY_MEMBER) {
					//Debug.Log ("Skill requires party target. Target must be selected to confirm!");
					targetSingle (getLivingActor());
				}else if (curSkill.targetType == TargetType.SELF) {
					//Debug.Log ("Skill requires self. Confirmation required!");
					targetSelf();
				}else if (curSkill.targetType == TargetType.ALL_TARGET) {
					//Debug.Log ("Skill will hit all enemies. Confirmation required!");
					targetEntireParty();
				}else if (curSkill.targetType == TargetType.ALL_PARTY) {
					//Debug.Log ("Skill will hit all party. Confirmation required!");
					targetEntireEnemies();
				}
			}else{
				//Inform the cost
			}
		}else if (bcb.myLinkedSkill == null) {
			Debug.Log ("Player selected sub-menu " + bcb.myLinkedMenu);
			cameFromSubMenu = true;
			string submenu = "";
			if (bcb.myLinkedMenu == 0) {
				submenu = "skill";
			}else if (bcb.myLinkedMenu == 1) {
				submenu = "blackmagic";
			}else if (bcb.myLinkedMenu == 2) {
				submenu = "whitemagic";
			}else if (bcb.myLinkedMenu == 3) {
				submenu = "items";
			}else{
				submenu = null;
			}
			if (submenu != null){
				_bm._gm._GUI.showSubMenu(turnOwner, submenu); 
            }else{
				Debug.LogError("Invalid submenu was called. Terminating...");
				Application.Quit();
			}
		}
	}

	public void executeSkill() {
		if (!isEnemy (turnOwner)) {
			StopCoroutine("WaitForPlayerTurn");
			_bm._gm._GUI.hideActionMenu();
		}
		getTurnOwner().GetComponent<Actor>()._base.curMana -= curSkill.cost;
		if (!isEnemy(getTurnOwner())){
			_bm._gm._GUI.updatePartyValues();
		}

		battleStage = BattleStage.TURN_EXECUTING;
		turnStage = TurnStage.AUTOMATION;
		finalTargets = getMarkedActors();
		curSkill.useSkill(this);
	}

	private IEnumerator WaitForPlayerTurn() {
		//Debug.Log ("Game now waiting for command input from player!");
		while (turnStage != TurnStage.AUTOMATION) {
			yield return new WaitForSeconds(2.5f);
		}
	}

	//Handles victory screen
	public void showVictory() {
		Debug.Log ("VICTORY");
		infoBox.transform.parent.gameObject.SetActive(false);
		_bm._gm.monstersSlainTotal += enemyDown;
		_bm._gm._GUI.forceLevelAudio(_bm._gm._GUI.victoryMusic, false);
		_bm._gm._GUI.hideActionMenu();
		_bm._gm._GUI.hideGenericBattleUI ();
		StartCoroutine ("victoryDetails");
	}

	IEnumerator victoryDetails() {
		_bm._gm._GUI.applyFadeOut (4);
		yield return new WaitForSeconds (6);
		_bm._gm._GUI.showVictory ();
		yield return new WaitForSeconds (6);
		_bm.battleWon = true;
	}

	#region Targetting

	//targets a single target (party or enemy)
	public void targetSingle(BattleTarget tar) {
		removeAllTargets();
		if (tar != null) {
			//Debug.Log ("Single target was found!");
			tar.isTargeted = true;
			_bm._gm._GUI.markTargets();
		}else{
			Debug.LogError ("Target was not set! Battle MUST hence be won!");
		}
	}

	public void targetSelf() {
		removeAllTargets();

		BattleTarget selfTar = null;
		foreach(BattleTarget pbt in party) {
			if (pbt.actor == turnOwner) {
				selfTar = pbt;
				break;
			}
		}

		selfTar.isTargeted = true;
		_bm._gm._GUI.markTargets();
	}

	//returns the single target selected by a skill
	public BattleTarget getSingleTarget() {
		foreach(BattleTarget pbt in party) {
			if (pbt.isTargeted) {
				return pbt;
			}
		}
		foreach(BattleTarget ebt in enemies) {
			if (ebt.isTargeted) {
				return ebt;
			}
		}
		Debug.LogWarning ("No battle targets have been targetted!");
		return null;
	}

	//gets the next target for a single target skill
	public BattleTarget getNextTarget(bool ascending = true, bool allowDeadTargets = false) {
		BattleTarget bt = getSingleTarget();

		int index = -1;
		bool isParty = true;
		bool requiresOtherSide = false;

		for(int i = 0; i < party.Length; i++) {
			if (party[i] == bt) {
				index = i;
				isParty = true;
			}
		}
		for(int i = 0; i < enemies.Length - 1; i++) {
			if (enemies[i] == bt) {
				index = i;
				isParty = false;
			}
		}

		if (index != -1) {
			if (isParty) {
				//PARTY
				//Debug.Log ("Starting at: " + index);
				if (ascending) {
					if (allowDeadTargets) {
						Debug.LogWarning ("Dead targets not yet implemented!");
						return getNextTarget(ascending);
					}else{
						//Debug.Log ("Finding: " + (index + 1));
						if (index == 2) {
							return getLivingActor();
						}else if (index == 1) {
							if (party[2].actor.GetComponent<Actor>()._base.curHealth > 0) {
								return party[2];
							}else{
								if (party[0].actor.GetComponent<Actor>()._base.curHealth > 0) {
									return party[0];
								}else{
									Debug.LogWarning ("All other party members are KO'd");
									return bt;
								}
							}
						}else if (index == 0) {
							if (party[1].actor.GetComponent<Actor>()._base.curHealth > 0) {
								return party[1];
							}else{
								if (party[2].actor.GetComponent<Actor>()._base.curHealth > 0) {
									return party[2];
								}else{
									Debug.LogWarning ("All other party members are KO'd");
									return bt;
								}
							}
						}
					}
				}else{
					if (allowDeadTargets) {
						Debug.LogWarning ("Dead targets not yet implemented!");
						return getNextTarget(ascending);
					}else{
						//Debug.Log ("Finding: " + (index - 1));
						if (index == 0) {
							//If cur target is first party, check last then mid
							if (party[party.Length - 1].actor.GetComponent<Actor>()._base.curHealth > 0) {
								return party[party.Length - 1];
							}else{
								if (party[party.Length - 2].actor.GetComponent<Actor>()._base.curHealth > 0 ){
									return party[party.Length - 2];
								}else{
									Debug.LogWarning ("All other party members are KO'd");
									return bt;
								}
							}
						}else if (index > 0) {
							//If cur target is mid or last / check first or check mid then first
							if (index + 1 == party.Length - 1) {
								//Mid
								if (party[0].actor.GetComponent<Actor>()._base.curHealth > 0) {
									return party[0];
								}else{
									if (party[2].actor.GetComponent<Actor>()._base.curHealth > 0) {
										return party[2];
									}else{
										Debug.LogWarning ("All other party members are KO'd");
										return bt;
									}
								}
							}else if (index == party.Length - 1) {
								//Last
								if (party[1].actor.GetComponent<Actor>()._base.curHealth > 0) {
									return party[1];
								}else{
									if (party[0].actor.GetComponent<Actor>()._base.curHealth > 0) {
										return party[0];
									}else{
										Debug.LogWarning ("All other party members are KO'd");
										return bt;
									}
								}
							}else{
								Debug.Log ("Other two members are KO'd");
								return bt;
							}
						}
					}
				}
			}else{
				//ENEMY
				if (ascending) {
					if (index == enemyCount - 1) {
						//Last enemy
						return getLivingActor(false);
					}else if (index < enemyCount - 1) {
						int cnt = index + 1;
						//If first, check mid and last / If mid, check last / In case error get first living
						while (cnt != index) {
							if (cnt > enemyCount - 1) {
								cnt = 0;
							}else{
								if (enemies[cnt].actor.GetComponent<Actor>()._base.curHealth > 0) {
									return enemies[cnt];
								}else{
									cnt++;
									if (cnt == enemyCount) {
										cnt = 0;
									}
								}
							}
						}
						Debug.LogWarning ("No other enemies!");
						return bt;
					}
				}else{
					if (index == 0) {
						int cnt = enemyCount - 1;
						//Loop through each enemy except current target
						while (cnt > 0) {
							if (enemies[cnt].actor.GetComponent<Actor>()._base.curHealth > 0) {
								return enemies[cnt];
							}else{
								cnt--;
							}
						}
						Debug.LogWarning ("No other enemies!");
						return bt;
					}else if (index > 0) {
						int cnt = index - 1;
						//Loop through each enemy except current target
						while (cnt != index) {
							if (enemies[cnt].actor.GetComponent<Actor>()._base.curHealth > 0) {
								return enemies[cnt];
							}else{
								cnt--;
								if (cnt == -1) {
									cnt = enemyCount - 1;
								}
							}
						}
						Debug.LogWarning ("No other enemies!");
						return bt;
					}
				}
			}
		}else{
			Debug.LogError ("Could not find next target for " + bt.actor.name);
			return bt;
		}

		Debug.LogError ("Could not find marked target from list!");
		return null;
	}

	//registers all party as targets
	public void targetEntireParty() {
		removeAllTargets();
		foreach(BattleTarget bt in party) {
			bt.isTargeted = true;
		}
		_bm._gm._GUI.markTargets();
	}

	//registers all enemies as targets
	public void targetEntireEnemies() {
		removeAllTargets();
		foreach(BattleTarget bt in enemies) {
			bt.isTargeted = true;
		}
		_bm._gm._GUI.markTargets();
	}

	//resets all targets to prepare next targets
	public void removeAllTargets() {
		foreach(BattleTarget pbt in party) {
			pbt.isTargeted = false;
			Destroy(pbt.myArrow);
		}
		foreach(BattleTarget ebt in enemies) {
			if (ebt.actor != null) {
				ebt.isTargeted = false;
				Destroy(ebt.myArrow);
			}
		}
	}

	public List<BattleTarget> getMarkedActors() {
		List<BattleTarget> targets = new List<BattleTarget>();
		foreach(BattleTarget pbt in party) {
			if (pbt.isTargeted) {
				//Debug.Log (pbt.actor.name + " is marked!");
				targets.Add(pbt);
			}
		}
		foreach(BattleTarget ebt in enemies) {
			if (ebt.isTargeted) {
				//Debug.Log (ebt.actor.name + " is marked!");
				targets.Add(ebt);
			}	
		}
		return targets;
	}

	#endregion

	#region Skill Execution
	private Skill executingSkill;

	private bool meleeRangeFound = false;

	public void setupSkillExecution(Skill s) {
		meleeRangeFound = false;
		executingSkill = s;
		infoBox.text = s.name;
	}

	//This can only be used for a single target attack requiring melee
	protected IEnumerator getInMeleeRange() {
		if (executingSkill.requiresMeleePos){

			BattleTarget myUser = getTurnOwnerTarget();
			BattleTarget myTarget = getSingleTarget();

			myUser.actor.transform.LookAt(myTarget.attackPos);

			if (myUser.actor.GetComponent<Actor>().hasAnimation("Move")) {
				myUser.actor.GetComponent<Actor>().playAnim("Move");
			}
			//myUser.actor.GetComponent<Actor>() - begin move animation
			//Make user move and animate to target's attack position
			//Once there set animationPlayed to true
			float step = 0;
			do {
				step = _bm.battleMoveSpeed * Time.deltaTime;
				myUser.actor.transform.position = Vector3.MoveTowards(myUser.actor.transform.position, myTarget.attackPos.position, step);
				yield return new WaitForSeconds(Time.deltaTime);
			}while (Vector3.Distance(myUser.actor.transform.position, myTarget.attackPos.position) > 0.25);
			myUser.actor.transform.position = myTarget.attackPos.position;
			myUser.actor.transform.rotation = myTarget.attackPos.rotation;
			myUser.actor.transform.LookAt(myTarget.actor.transform);
			myUser.actor.GetComponent<Actor>().playAnim("Idle");

			executingSkill.advanceSkillStage(0);
		}
		//http://answers.unity3d.com/questions/37411/how-can-i-wait-for-an-animation-to-complete.html

		meleeRangeFound = true;
		StartCoroutine("performAttack");
	}

	protected IEnumerator performAttack() {
		if (meleeRangeFound) {
			StopCoroutine("getInMeleeRange");
		}else{
			if (getMarkedActors().Count == 1) {
				getTurnOwnerTarget().actor.transform.LookAt (getSingleTarget().actor.transform);
			}else{
				int halfPoint = Mathf.CeilToInt(getMarkedActors().Count / 2);
				getTurnOwnerTarget().actor.transform.LookAt (getMarkedActors()[halfPoint].actor.transform);
			}
		}

		bool multiTargetAttack = false;
		int halfStare;
		if (getMarkedActors().Count != 1) {
			halfStare = Mathf.FloorToInt(getMarkedActors().Count / 2);
			multiTargetAttack = true;
		}else{
			halfStare = 1;
		}
	
		BattleTarget myUser = getTurnOwnerTarget();
		if (multiTargetAttack) {
			if (isEnemy(myUser.actor)) {
				myUser.actor.transform.LookAt (party[halfStare].actor.transform);
			}else{
				myUser.actor.transform.LookAt (enemies[halfStare].actor.transform);
			}
		}else{
			myUser.actor.transform.LookAt (getSingleTarget().actor.transform);
		}

		//Wait until cast animation complete
		//myUser.actor.GetComponent<Animator>().SetBool("casting", true);

		//yield new WaitForSeconds(animationClip.length);

		ActorBase myUserBase = myUser.actor.GetComponent<ActorBase>();

		//Calculate Damage and perform skill FX on enemies
		//FX works by instiating an prefab on target and having the FX script fade it into oblivion...
		//FX also includes the scrolling damage text included in newEffects list
		List<BattleTarget> tars = getMarkedActors();
		foreach(BattleTarget tar in tars) {
			float damage = 0;
			float defendDamage = 0;
			int finalDamage = 0;

			ActorBase myBase = tar.actor.GetComponent<ActorBase>();

			Elements myEle = myBase.myElement;
			Elements myOppEle = myBase.myOppElement;

			//Algorithms are modified algorithms based off Final Fantasy X
			//Reference: http://www.gamefaqs.com/ps2/197344-final-fantasy-x/faqs/31381

			if (executingSkill.effectType == EffectType.HEAL || executingSkill.effectType == EffectType.APPLY_EFF) {
				//Play cast animation
				if (myUser.actor.GetComponent<Actor>().hasAnimation("Cast")) {
					myUser.actor.GetComponent<Actor>().playAnim("Cast");
					yield return new WaitForSeconds(1);
				}
			}else{
				//Play attack animation
				if (meleeRangeFound) {
					if (myUser.actor.GetComponent<Actor>().hasAnimation("Attack")) {
						myUser.actor.GetComponent<Actor>().playAnim("Attack");
						yield return new WaitForSeconds(0.4f);
						if (tar.actor.GetComponent<Actor>().hasAnimation("Hit")) {
							tar.actor.GetComponent<Actor>().playAnim("Hit");
						}
					}

				}else{
					if (executingSkill.effectType == EffectType.PHYSICAL_DAMAGE) {
						if (myUser.actor.GetComponent<Actor>().hasAnimation("RAttack")) {
							myUser.actor.GetComponent<Actor>().playAnim("RAttack");
							yield return new WaitForSeconds(0.4f);
							if (tar.actor.GetComponent<Actor>().hasAnimation("Hit")) {
								tar.actor.GetComponent<Actor>().playAnim("Hit");
							}
						}
					}else{
						if (myUser.actor.GetComponent<Actor>().hasAnimation("MAttack")) {
							myUser.actor.GetComponent<Actor>().playAnim("MAttack");
							yield return new WaitForSeconds(0.8f);
							if (tar.actor.GetComponent<Actor>().hasAnimation("Hit")) {
								tar.actor.GetComponent<Actor>().playAnim("Hit");
							}
						}
					}
				}
			}

			//do {
			//	yield return new WaitForSeconds(0.25f);
			//}while (myUser.actor.GetComponent<Actor>().myAnim.isPlaying && myUser.actor.GetComponent<Actor>().myAnim.clip.name.ToLower().Trim () != "idle");

			if (executingSkill.effectType == EffectType.PHYSICAL_DAMAGE) {
				//PHYSICAL DAMAGE
				damage = (((Mathf.Pow(myUserBase.strength.value , 3) / 32) + myUserBase.curAttack) * executingSkill.magnitude) / 16;
				defendDamage = (Mathf.Pow(myUserBase.curDefense, 2) / 110) + 16;
				foreach(Effect eff in myUser.actor.GetComponent<Actor>().activeEffects){
					if (eff.myEffInfo == Effects.INC_STRENGTH) {
						damage *= eff.myValue;
					}
				}
				foreach(Effect eff in tar.actor.GetComponent<Actor>().activeEffects) {
					if (eff.myEffInfo == Effects.INC_DEFENSE) {
						defendDamage *= eff.myValue;
					}
				}
				Debug.Log ("Command did: " + damage + " but target blocked: " + defendDamage);
				finalDamage = Mathf.CeilToInt(damage - defendDamage);
				//finalDamage = 200;
				if (finalDamage < 0) {
					Debug.LogWarning ("Defended damage was greater than damage! Is math correct?");
					finalDamage = finalDamage * -1;
				}else{
					if (executingSkill.element == tar.actor.GetComponent<Actor>()._base.myOppElement) {
						finalDamage *= 2;
					}else if (executingSkill.element == tar.actor.GetComponent<Actor>()._base.myElement) {
						finalDamage *= -1;
					}
				}
			}else if (executingSkill.effectType == EffectType.MAGIC_DAMAGE) {
				//MAGIC DAMAGE
				damage = (executingSkill.magnitude * (Mathf.Pow(myUserBase.wisdom.value, 2) / 6) + myUserBase.wisdom.value) / 4;
				defendDamage = (Mathf.Pow(myUserBase.curMagicDefense, 2) / 110) + 16;
				foreach(Effect eff in myUser.actor.GetComponent<Actor>().activeEffects){
					if (eff.myEffInfo == Effects.INC_WISDOM) {
						damage *= eff.myValue;
					}
				}
				foreach(Effect eff in tar.actor.GetComponent<Actor>().activeEffects) {
					if (eff.myEffInfo == Effects.INC_MDEFENSE) {
						defendDamage *= eff.myValue;
					}
				}
				finalDamage = Mathf.CeilToInt(damage - defendDamage);
				//finalDamage = 200;
				if (finalDamage < 0) {
					Debug.LogWarning ("Defended damage was greater than damage! Is math correct?");
					finalDamage = finalDamage * -1;
				}else{
					if (executingSkill.element == tar.actor.GetComponent<Actor>()._base.myOppElement) {
						finalDamage *= 2;
					}else if (executingSkill.element == tar.actor.GetComponent<Actor>()._base.myElement) {
						finalDamage *= -1;
					}
				}
			}else{
				//HEAL
				damage = executingSkill.magnitude * ((myUserBase.wisdom.value +  executingSkill.magnitude) / 2);
				foreach(Effect eff in myUser.actor.GetComponent<Actor>().activeEffects){
					if (eff.myEffInfo == Effects.INC_WISDOM) {
						damage *= eff.myValue;
					}
				}
				if (executingSkill.element == tar.actor.GetComponent<Actor>()._base.myElement) {
					damage *= 2;
				}else if (executingSkill.element == tar.actor.GetComponent<Actor>()._base.myOppElement) {
					damage /= 2;
				}
			}

			foreach(BattleFXAnim bfa in _bm.fxAnims) {
				if (executingSkill.fxType == bfa.fxType) {
					if (bfa.fxPrefab != null) {
						Vector3 spawnPos = new Vector3(tar.actor.transform.position.x, tar.actor.transform.position.y + 1, tar.actor.transform.position.z);
						GameObject go = (GameObject)Instantiate(bfa.fxPrefab, spawnPos, tar.actor.transform.rotation);
						go.GetComponent<BattleFX>().run ();
					}else{
						//Debug.Log ("Skill does not use FX!");
					}
				}
			}

			if (executingSkill.effectType == EffectType.HEAL || executingSkill.effectType == EffectType.APPLY_EFF || executingSkill.element == tar.actor.GetComponent<Actor>()._base.myElement) {
				if (executingSkill.element != tar.actor.GetComponent<Actor>()._base.myElement) {
					tar.actor.GetComponent<Actor>()._base.restoreHealth(Mathf.FloorToInt(finalDamage));
					//Debug.Log ("Damage was of target's element! Damage is now healing!");
					Debug.Log ("Healing for: " + Mathf.FloorToInt (finalDamage));
				}else{
					Debug.Log ("Healing for: " + Mathf.FloorToInt (damage));
					tar.actor.GetComponent<Actor>()._base.restoreHealth(Mathf.FloorToInt(damage));
				}
				GameObject txtObj = (GameObject)Instantiate(_bm.textObject);
				txtObj.transform.position = new Vector3(tar.actor.transform.position.x, tar.actor.transform.position.y + 2.3f, tar.actor.transform.position.z);
				txtObj.GetComponent<TextMesh>().text = finalDamage.ToString();
				txtObj.GetComponent<BattleText>().StartCoroutine("fadeUpwards");
			}else if (executingSkill.element != tar.actor.GetComponent<Actor>()._base.myElement){
				tar.actor.GetComponent<Actor>()._base.takeDamage(finalDamage);
				Debug.Log ("Damage for command: " + finalDamage);
				GameObject txtObj = (GameObject)Instantiate(_bm.textObject);
				txtObj.transform.position = new Vector3(tar.actor.transform.position.x, tar.actor.transform.position.y + 2.3f, tar.actor.transform.position.z);
				txtObj.GetComponent<TextMesh>().text = finalDamage.ToString();
				txtObj.GetComponent<BattleText>().StartCoroutine("fadeUpwards");
			}

			//Prepare for next turn in case turns are re-calculated
			if (tar.actor.GetComponent<Actor>()._base.dead == true) {
				//Set animation to dead
				Debug.Log (tar.actor.GetComponent<Actor>()._base.name + " has died!");
				tar.actor.GetComponent<Actor>().playAnim("Dead");
			}

			lastTurnOwner = turns.Dequeue();
			turns.Enqueue(lastTurnOwner);
			Debug.Log ("Finishing turn for: " + lastTurnOwner.name);
			//Debug.Log ("Compare to next turn owner: " + turns.Peek ().name);

			_bm._gm._GUI.updatePartyValues();
			yield return new WaitForSeconds(2);
		}

		executingSkill.advanceSkillStage(1);
		if (meleeRangeFound) {
			StartCoroutine("returnToStartPos");
		}else{
			finishSkill();
		}
	}

	IEnumerator returnToStartPos() {
		StopCoroutine("performAttack");
		if (executingSkill.requiresMeleePos){
			BattleTarget myUser = getTurnOwnerTarget();
			Transform myStartPos = myUser.position;

			myUser.actor.transform.LookAt (myUser.position.position);
			if (myUser.actor.GetComponent<Actor>().hasAnimation("Move")) {
				myUser.actor.GetComponent<Actor>().playAnim("Move");
			}
			float step;
			do {
				step = myUser.actor.GetComponent<Actor>().runSpeed * Time.deltaTime;
				myUser.actor.transform.position = Vector3.MoveTowards(myUser.actor.transform.position, myStartPos.position, step);
				yield return new WaitForSeconds(Time.deltaTime);
			}while (Vector3.Distance(myUser.actor.transform.position, myUser.position.position) > 0.25);
			myUser.actor.transform.position = myStartPos.position;
			//myUser.actor.transform.rotation = myStartPos.rotation;
			if (isEnemy(myUser.actor)) {
				myUser.actor.transform.LookAt(party[1].position);
			}else{
				myUser.actor.transform.LookAt(enemies[enemyCount / 2].position);
			}
			myUser.actor.GetComponent<Actor>().playAnim("Idle");

			executingSkill.advanceSkillStage(2);
			finishSkill();
		}
	}

	private void finishSkill() {
		if (meleeRangeFound) {
			StopCoroutine("returnToStartPos");
		}else{
			Debug.Log ("This was called fine!");
			StopCoroutine("performAttack");
		}
		Debug.Log (turnStage.ToString() +  " / " + battleStage.ToString());
		switchNextStage();
	}

	#endregion

	//Misc

	public GameObject getTurnOwner() {
		return turnOwner;
	}

	public BattleTarget getTurnOwnerTarget() {
		GameObject tar = getTurnOwner();
		foreach(BattleTarget pbt in party) {
			if (pbt.actor == tar) {
				return pbt;
			}
		}
		foreach(BattleTarget ebt in enemies) {
			if (ebt != null) {
				if (ebt.actor == tar) {
					return ebt;
				}
			}
		}
		Debug.LogError ("Cannot find turn owner's battle target!");
		return null;
	}

	public BattleTarget getLivingActor(bool inParty = true) {
		if (inParty) {
			for (int i = 0; i < party.Length - 1; i++ ) {
				BattleTarget bt = party[i];
				if (bt.actor.GetComponent<Actor>()._base.curHealth > 0) {
					Debug.Log ("Returning " + bt.actor.name);
					return bt;
				}
			}
		}else{
			for(int i = 0; i < enemyCount; i++) {
				BattleTarget bt = enemies[i];
				if (bt.actor.GetComponent<Actor>()._base.curHealth > 0) {
					//Debug.Log ("Returning " + bt.actor.name);
					return bt;
				}
			}
		}
		Debug.LogError ("Could not find first living target!");
		return null;
	}

	public bool isEnemy(GameObject target) {
		foreach(BattleTarget bt in enemies) {
			if (bt.actor == target) {
				//Debug.Log (target + " is an enemy!");
				return true;
			}
		}
		Debug.Log (target + " is not an enemy!");
		return false;
	}

	public void registerDeadActor(GameObject actorTar) {
		BattleTarget newBT = null;
		if (isEnemy(actorTar)) {
			enemyDown++;
			foreach(BattleTarget bt in enemies) {
				if (bt != null) {
					if (bt.actor == actorTar) {
						newBT = bt;
					}
				}
			}
		}else{
			partyDown++;
			foreach(BattleTarget bt in party) {
				if (bt != null) {
					if (bt.actor == actorTar) {
						newBT = bt;
					}
				}
			}
		}
		if (newBT != null) {
			calculateTurns(newBT);
			if (!isEnemy(newBT.actor)) {
				foreach(BattleTarget bt in enemies) {
					if (bt != null) {
						if (bt.actor.GetComponent<Actor>()._base.curHealth > 0) {
							bt.actor.GetComponent<Actor>().resetThreat(newBT);
						}
					}
				}
			}
		}else{
			Debug.LogError ("Could not set actor as dead!");
		}
	}

	//Go back to prior submenu
	public void dropTargetSelection() {
		if (cameFromSubMenu) {
			_bm._gm._GUI.showSubMenu(getTurnOwner(), lastSubMenu);
		}else{
			_bm._gm._GUI.showBaseMenu(getTurnOwner());
		}
	}

	public void generatePassiveThreat() {
		for (int i = 0; i < enemyCount-1; i++ ) {
			if (enemies[i].actor.GetComponent<Actor>()._base.curHealth > 0) {
				if (party[0].actor.GetComponent<Actor>()._base.curHealth > 0) {
					enemies[i].actor.GetComponent<Actor>().addThreat(party[0], Mathf.FloorToInt(Random.Range (20, 40)));
				}
				if (party[1].actor.GetComponent<Actor>()._base.curHealth > 0) {
					enemies[i].actor.GetComponent<Actor>().addThreat(party[1], Mathf.FloorToInt(Random.Range (20, 40)));
				}
				if (party[2].actor.GetComponent<Actor>()._base.curHealth > 0) {
					enemies[i].actor.GetComponent<Actor>().addThreat(party[2], Mathf.FloorToInt(Random.Range (20, 40)));
				}
			}
		}
	}

	//Called when command is confirmed, action has executed and turn changed
	public void switchNextStage() {
		if (battleStage == BattleStage.TURN_DECIDING) {
			battleStage = BattleStage.TURN_EXECUTING;
			Debug.Log ("Action for turn now being executed!");
			return;
		}else if (battleStage == BattleStage.TURN_EXECUTING) {
			battleStage = BattleStage.CHANGING_TURN;
			if (!isEnemy(turnOwner)) {
				StopCoroutine("WaitForPlayerTurn");
			}
			turnStage = TurnStage.TURN_SET;
			Debug.Log ("Switching to next actor!");
			turnManagement();
		}else if (battleStage == BattleStage.CHANGING_TURN) {
			battleStage = BattleStage.TURN_DECIDING;
			turnManagement();
		}
	}

}

public enum BattleStage {
	TURN_DECIDING,
	TURN_EXECUTING,
	CHANGING_TURN
}

public enum TurnStage {
	STARTING,
	SELECTING_ACTION,
	SELECTING_ACTION_SUBMENU,
	SELECTING_TARGET,
	TURN_SET,
	AUTOMATION
}