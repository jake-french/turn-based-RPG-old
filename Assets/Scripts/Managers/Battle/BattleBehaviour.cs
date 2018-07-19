using UnityEngine;
using System.Collections;

[System.Serializable]
public class BattleBehaviour : System.Object {
	public string skillName;
	public int priority;
	public BattleCondition[] conditions;
	public int[] values;
}

public enum BattleCondition {
	ANY_TURN,
	TURN_NUMBER,
	TURN_EVEN,
	TURN_ODD,
	HEALTH,
	MANA,
	ENEMY_ELE
}