using UnityEngine;
using System.Collections;

public class Shock : Skill {
	
	public Shock() {
		name = "Shock";
		description = "Deals earth damage to a target";
		magnitude = 10;
		cost = 12;
		element = Elements.EARTH;
		skillType = SkillType.Black_Magic;
		effectType = EffectType.MAGIC_DAMAGE;
		targetType = TargetType.TARGET;
		fxType = BattleFXType.SHOCK_SINGLE;
		requiresMeleePos = false;
		useInField = false;
		useOnDead = false;
	}
}
