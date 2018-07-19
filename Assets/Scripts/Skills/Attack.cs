using UnityEngine;
using System.Collections;

public class Attack : Skill {

	public Attack() {
		name = "Attack";
		description = "Deals physical damage to one target.";
		magnitude = 10;
		cost = 0;
		element = Elements.UNCLASSIFIED;
		skillType = SkillType.NONE;
		effectType = EffectType.PHYSICAL_DAMAGE;
		targetType = TargetType.TARGET;
		fxType = BattleFXType.NONE;
		requiresMeleePos = true;
		useInField = false;
		useOnDead = false;
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

	}

	//HAVE coroutines call each other as last command respectively. The first coroutine after the others are finished should call the switchStage()
}
