using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour {
	[HideInInspector]
	public string name;
	public Skill myOwnerSkill;
	public Effects myEffInfo;
	public float myValue;	//for percentage increases assign values such as 1.05 => 5%

	public Effect(string name, Skill myOwner, Effects myEff, float val) {
		this.name = name;
		myOwnerSkill = myOwner;
		myEffInfo = myEff;
		myValue = val;
	}
}

public enum Effects {
	INC_STRENGTH,
	INC_WISDOM,
	INC_ENDURANCE,
	INC_SPEED,
	INC_DEFENSE,
	INC_MDEFENSE
}