using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ActorBase : MonoBehaviour {
	#region Variables
	public string name;
	public ActorType type;
	public bool dead = false;

	#region Resources
	//Health
	public int baseHealth;
	public int curHealth;
	public int maxHealth;
	
	//Mana
	public int baseMana;
	public int curMana;
	public int maxMana;
	#endregion

	#region Other Stats
	public int curAttack;
	public int curDefense;
	public int curMagicAttack;
	public int curMagicDefense;

	#endregion

	#region Attributes
	public Attribute strength = new Attribute("Strength");
	public Attribute wisdom = new Attribute("Wisdom");
	public Attribute endurance = new Attribute("Endurance");
	public Attribute speed = new Attribute("Speed");
	
	#endregion

	#region Elements
	public Elements myElement;
	public Elements myOppElement;
	#endregion

	public List<string> startSkills;	//list of skills to start with
	[HideInInspector]
	public List<Skill> skills;

	#endregion

	#region Public Functions

	void Update() {
		myOppElement = setOppElement();
	}

	public void Setup() {
		CalculateAll();
		curHealth = maxHealth;
		curMana = maxMana;

		List<Skill> skills = GameObject.Find("_GameManager").GetComponent<_GameManager>().allSkills;
		foreach(string s in startSkills) {
			for (int i = 0; i < skills.Count; i++ ){
				if (skills[i].name.ToLower().Trim () == s.ToLower().Trim ()) {
					Debug.Log (this.name + " has skill: " + skills[i].name);
					this.skills.Add (skills[i]);
				}
			}
		}
	}

	#region Stats
	public void incrementStat(string stat, int val) {
		stat = stat.ToLower();
		if (stat == "health") {
			maxHealth += val;
		}else if (stat == "mana") {
			maxMana += val;
		}else if (stat == "defense") {
			curDefense += val;
		}
	}

	public void decrementStat(string stat, int val) {
		stat = stat.ToLower();
		if (stat == "health") {
			maxHealth -= val;
		}else if (stat == "mana") {
			maxMana -= val;
		}else if (stat == "defense") {
			curDefense -= val;
		}
	}

	public void CalculateAll() {
		CalculateMaxHealth();
		CalculateMaxMana();

		curHealth = maxHealth;
		curMana = maxMana;
	}

	public void CalculateMaxHealth() {
		int statBoost = endurance.value * 4;
		maxHealth = baseHealth + statBoost;
	}

	public void CalculateMaxMana() {
		int statBoost = wisdom.value * 4;
		maxMana = baseMana + statBoost;
	}

	public void takeDamage(int dmg) {
		if (dmg > 0) {
			curHealth -= dmg;
			if (curHealth <= 0) {
				curHealth = 0;
				setDead(true);
			}
		}else{
			Debug.LogWarning ("Cannot damage health by a negative value!");
		}
	}

	public void restoreHealth(int dmg) {
		if (dmg > 0) {
			if (!dead) {
				curHealth += dmg;
				if (curHealth > maxHealth) {
					curHealth = maxHealth;
				}
			}
		}else{
			Debug.LogWarning ("Cannot restore health by a negative value!");
		}
	}
	
	#endregion

	#region Elements

	public Elements setOppElement() {
		if (myElement == Elements.FIRE) {
			return Elements.WATER;
		}else if (myElement == Elements.EARTH) {
			return Elements.WIND;
		}else if (myElement == Elements.WATER) {
			return Elements.FIRE;
		}else if (myElement == Elements.WIND) {
			return Elements.EARTH;
		}else if (myElement == Elements.LIGHT) {
			return Elements.DARK;
		}else if (myElement == Elements.DARK) {
			return Elements.LIGHT;
		}else if (myElement == Elements.LIFE) {
			return Elements.DEATH;
		}else if (myElement == Elements.DEATH){
			return Elements.LIFE;
		}
		return Elements.UNCLASSIFIED;
	}
	#endregion
	
	#endregion

	#region Private Funtions

	private void setDead(bool state = true) {
		dead = state;
		Battlefield bf = GameObject.FindGameObjectWithTag("Battlefield").GetComponent<Battlefield>();
		bf.registerDeadActor(this.gameObject);
	}

	#endregion
}

public enum ActorType {
	Humanoid,
	Beast,
	Demon,
	Spirit
}