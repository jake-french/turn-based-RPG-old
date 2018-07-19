using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class Skill : System.Object {
	[HideInInspector]
	public string name;			//visible name of the skill
	public string description;
	public int magnitude;		//constant value of the skill
	public int cost;			//constant cost of the skill
	public Elements element;	//Element of the skill
	public SkillType skillType;		//Menu in which skill can be found
	public EffectType effectType;		//type of effect this skill has
	public Effect effectApplying = null;	//the "buff" to apply from this skill
	public TargetType targetType;	//Type of target needed to execute skill
	public BattleFXType fxType;
	public Image icon;			//Icon to appear next to name in battle
	public bool requiresMeleePos;	//Variable to detect whether move to attack position stage required
	public bool useInField;		//If skill is usuable in field
	public bool useOnDead;		//If skill can be used to target dead party members

	//Skill Execution variables
	protected bool animationPlayed = false;
	protected bool effectsFadedAway = false;
	protected bool attackerReturnToStart = false;

	//Override this method with skill's actual effect (Battlefield gives ability to affect field - REQUIRED)
	public void useSkill() {
		if (useInField) {

		}else{
			Debug.Log (name + " cannot be used in the field!");
		}
	}

	public void useSkill(Battlefield bf) {
		bf.setupSkillExecution(this);
		//Get into correct attack position
		Debug.Log ("Using skill: " + name);
		//Debug.Break ();
		
		if (requiresMeleePos) {
			bf.StartCoroutine("getInMeleeRange");
		}else{
			bf.StartCoroutine("performAttack");
		}

		//move to melee if required
		//Play approriate animation
		//Calculate damage
		//if damage is applied create instance of the attack effect which fades out via use of ienumator or update
		//fade out effect animation
		//return user to default position
		//inform battlefield to progress to next turn
	}

	public void advanceSkillStage(int stage) {
		if (stage == 0) {
			animationPlayed = true;
		}else if (stage == 1) {
			effectsFadedAway = true;
		}else if (stage == 2) {
			attackerReturnToStart = true;
		}else{
			Debug.LogError ("Incorrect usage of advancedSkillStage! Only values: 0-2 are accepted!");
		}
	}

	//Coroutines cannot be set to an object not inherited by MonoBehaviour
	//All calls to corountines for skill exectuion are in Battlefield.cs

}

public enum SkillType {
	NONE,
	Skill,
	White_Magic,
	Black_Magic
}

public enum EffectType {
	PHYSICAL_DAMAGE,
	MAGIC_DAMAGE,
	HEAL,
	APPLY_EFF
}

public enum TargetType {
	NULL,
	TARGET,
	SELF,
	PARTY_MEMBER,
	ALL_TARGET,
	ALL_PARTY
}
