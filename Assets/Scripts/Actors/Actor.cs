using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ActorBase))]
public class Actor : MonoBehaviour {
	private _GameManager _gm;
	private Battlefield bf;

	public Animation myAnim;
	public ActorBase _base;
	public bool isEnemy;
	public List<ActorAnim> animations;

	[HideInInspector]
	public List<Effect> activeEffects;
	public List<BattleBehaviour> behaviours;
	public float runSpeed;

	private List<BattleThreat> targets;

	void Start() {
		if (!_gm) {
			_gm = GameObject.Find ("_GameManager").GetComponent<_GameManager>();
		}
		if (!_base) {
			_base = GetComponent<ActorBase>();
		}

		_base.myOppElement = _base.setOppElement();
	}

	public void prepForBattle(Battlefield bf) {
		this.bf = bf;
		targets = new List<BattleThreat>();
		foreach(BattleTarget bt in bf.party) {
			BattleThreat newThreat = new BattleThreat(bt);
			targets.Add (newThreat);
		}
	}
	
	public Skill getOwnedSkill(string name) {
		List<Skill> mySkills = _base.skills;

		foreach(Skill s in mySkills) {
			if (s.name.ToLower().Trim () == name.ToLower ().Trim ()) {
				return s;
			}
		}
		Debug.LogWarning ("Could not find requested skill on actor: " + _base.name);
		return null;
	}

	//Animation

	public void playAnim(string name) {
		List<ActorAnim> newAnim = new List<ActorAnim>();
		foreach(ActorAnim aa in animations) {
			if (aa.name.ToLower ().Trim () == name.ToLower ().Trim ()) {
				newAnim.Add (aa);
			}
		}

		ActorAnim pickedAnim = null;
		if (newAnim.Count == 1) {
			Debug.Log ("Only one animation found to use!");
			pickedAnim = newAnim[0];
		}else{
			pickedAnim = newAnim[Mathf.FloorToInt(Random.Range (0, newAnim.Count - 1))];
			Debug.Log ("Chose to use animation: " + pickedAnim.myClip.name);
		}
		if (newAnim != null) {
			myAnim.Play (pickedAnim.myClip.name, AnimationPlayMode.Stop);
			if (pickedAnim.soundClip != null) {
				_gm._GUI.playSoundFX(pickedAnim.soundClip);
			}
		}else{
			Debug.Log ("Actor does not have animation called " + name + "!");
		}
	}

	public bool hasAnimation(string name) {
		foreach(ActorAnim aa in animations) {
			if (aa.name.ToLower().Trim () == name.ToLower().Trim ()) {
				return true;
			}
		}
		return false;
	}

	//AI Turn Decisions

	public void addThreat(BattleTarget bt, int val) {
		foreach(BattleThreat bth in targets) {
			if (bth.target == bt) {
				bth.threat += val;
				return;
			}
		}
		Debug.LogError ("Could not find battle target in list of threats!");
	}

	public void addThreatToOwner(int val) {
		BattleTarget owner = bf.getTurnOwnerTarget();
		foreach(BattleThreat bt in targets) {
			if (bt.target == owner) {
				bt.threat += val;
				return;
			}
		}
		Debug.LogError ("Could not add threat!");
	}

	public void resetThreat(BattleTarget tar) {
		foreach(BattleThreat bt in targets) {
			if (bt.target == tar) {
				bt.threat = 0;
				return;
			}
		}
		Debug.LogError ("Could not reset target's threat!");
	}

	public void removeThreat(BattleTarget tar, int val) {
		foreach(BattleThreat bt in targets) {
			if (bt.target == tar) {
				bt.threat -= val;
				return;
			}
		}
		Debug.LogError ("Could not remove threat!");
	}

	public BattleThreat getHighestThreat() {
		BattleThreat highest = null;
		foreach (BattleThreat bt in targets) {
			if (highest == null) {
				highest = bt;
			}else if (highest != null) {
				if (bt.threat > highest.threat) {
					highest = bt;
				}
			}
		}
		return highest;
	}

	public void takeTurnAI() {
		//code for enemy AI to determine action
		List<BattleBehaviour> plausbile = new List<BattleBehaviour>();

		foreach(BattleBehaviour bb in behaviours) {
			bool test = testCondition(bb);
			if (test) {
				plausbile.Add (bb);
			}
			/*
			if (bb.condition == BattleCondition.TURN_NUMBER) {
				if (bf.turnCount == bb.value) {
					highestPriority = bb;
					break;
				}
			}else{
				if (bb.condition == BattleCondition.TURN_EVEN || bb.condition == BattleCondition.TURN_ODD) {
					highestPriority = bb;
				}else{
					if (bb.condition == BattleCondition.ANY_TURN) {

					}
				}
			}
			*/
		}

		List<BattleBehaviour> highestPrioritys = new List<BattleBehaviour>();
		int topPriority = 0;
		foreach(BattleBehaviour bb in plausbile) {
			if (bb.priority > topPriority) {
				highestPrioritys.Clear();
				topPriority = bb.priority;
				highestPrioritys.Add (bb);
			}else if (bb.priority == topPriority) {
				highestPrioritys.Add (bb);
			}
		}

		BattleBehaviour finalChoice = null;
		if (highestPrioritys.Count > 1) {
			finalChoice = highestPrioritys[Mathf.FloorToInt(Random.Range(0, highestPrioritys.Count-1))];
		}else if (highestPrioritys.Count == 1){
			finalChoice = highestPrioritys[0];
		}else if (highestPrioritys.Count < 1) {
			Debug.Log ("No skills were found for enemy to use!");
		}

		if (finalChoice != null) {
			Skill skillUsing = getOwnedSkill(finalChoice.skillName);
			if (skillUsing != null) {
				if (skillUsing.targetType == TargetType.ALL_TARGET) {
					bf.targetEntireParty();
				}else if (skillUsing.targetType == TargetType.TARGET) {
					BattleTarget myTar = getHighestThreat().target;
					while (myTar.actor.GetComponent<Actor>()._base.curHealth < 0) {
						myTar = targets[Mathf.FloorToInt(Random.Range (0, targets.Count - 1))].target;
					}
					bf.targetSingle(myTar);
				}else if (skillUsing.targetType == TargetType.SELF) {
					bf.targetSelf();
				}else if (skillUsing.targetType == TargetType.ALL_PARTY) {
					bf.targetEntireEnemies();
				}else if (skillUsing.targetType == TargetType.PARTY_MEMBER) {

				}
				bf.curSkill = skillUsing;
				bf.executeSkill();
			}else{
				Debug.LogError("Skill was not found in actor's owned skills list!");
			}
		}else{
			Debug.LogError(_base.name + " was unable to find a command!");
		}
	}

	public bool testCondition(BattleBehaviour bb) {
		bool testPassed = true;
		for (int i = 0; i < bb.conditions.Length-1; i++ ) {
			if (_base.curMana < getOwnedSkill(bb.skillName).cost) {
				testPassed = false;
				break;
			}else if (bb.conditions[i] == BattleCondition.TURN_NUMBER) {
				if (bf.turnCount != bb.values[i]) {
					testPassed = false;
					break;
				}
			}else if (bb.conditions[i] == BattleCondition.TURN_EVEN || bb.conditions[i] == BattleCondition.TURN_ODD) {
				if (bb.conditions[i] == BattleCondition.TURN_EVEN) {
					//EVEN
					if (bf.turnCount % 2 != 0) {
						testPassed = false;
						break;
					}
				}else{
					//ODD
					if (bf.turnCount % 2 != 1) {
						testPassed = false;
						break;
					}
				}
			}else if (bb.conditions[i] == BattleCondition.HEALTH) {
				if (_base.curHealth > bb.values[i]) {
					testPassed = false;
					break;
				}
			}else if (bb.conditions[i] == BattleCondition.MANA) {
				if (_base.curHealth > bb.values[i]) {
					testPassed = false;
					break;
				}
			}else if (bb.conditions[i] == BattleCondition.ENEMY_ELE) {
				if (getOwnedSkill(bb.skillName).element == getHighestThreat().target.actor.GetComponent<Actor>()._base.myElement) {
					testPassed = false;
					break;
				}
			}
		}
		return testPassed;
	}
}

public class BattleThreat {
	public BattleTarget target;
	public int threat;

	public BattleThreat(BattleTarget tar) {
		target = tar;
		threat = 0;
	}
}
