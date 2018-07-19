using UnityEngine;
using System.Collections;

public class Heal : Skill {

	public Heal() {
		name = "Heal";
		description = "Restores HP to a target";
		magnitude = 10;
		cost = 8;
		element = Elements.LIFE;
		skillType = SkillType.White_Magic;
		effectType = EffectType.HEAL;
		targetType = TargetType.PARTY_MEMBER;
		fxType = BattleFXType.HEAL_SINGLE;
		requiresMeleePos = false;
		useInField = true;
		useOnDead = false;
	}
}
