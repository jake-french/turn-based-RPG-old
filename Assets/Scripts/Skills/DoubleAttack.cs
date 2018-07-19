using UnityEngine;
using System.Collections;

public class DoubleAttack : Skill {

	public DoubleAttack() {
		name = "Two Blade";
		description = "Deals physical damage twice to one target.";
		magnitude = 20;
		cost = 12;
		element = Elements.UNCLASSIFIED;
		skillType = SkillType.Skill;
		effectType = EffectType.PHYSICAL_DAMAGE;
		targetType = TargetType.TARGET;
		fxType = BattleFXType.NONE;
		requiresMeleePos = true;
		useInField = false;
		useOnDead = false;
	}
}
