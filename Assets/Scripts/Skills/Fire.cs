using UnityEngine;
using System.Collections;

public class Fire : Skill {
	
	public Fire() {
		name = "Fire";
		description = "Deals fire damage to a target";
		magnitude = 10;
		cost = 12;
		element = Elements.FIRE;
		skillType = SkillType.Black_Magic;
		effectType = EffectType.MAGIC_DAMAGE;
		targetType = TargetType.TARGET;
		fxType = BattleFXType.FIRE_SINGLE;
		requiresMeleePos = false;
		useInField = false;
		useOnDead = false;
	}
}
