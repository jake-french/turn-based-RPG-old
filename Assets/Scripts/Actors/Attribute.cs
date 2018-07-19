using UnityEngine;
using System.Collections;

[System.Serializable]
public class Attribute : System.Object {
	[HideInInspector]
	public string name;
	public int value;

	public Attribute(string name) {
		this.name = name;
	}

}

public enum Elements {
	FIRE,
	EARTH,
	WATER,
	WIND,
	LIGHT,
	DARK,
	LIFE,
	DEATH,
	UNCLASSIFIED
}
