using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelInfo : System.Object {
	public string sceneName;
	public LevelType type;
	
	public string linkedScene;		//if level is field: this is the battle, if level is battle: this is the field
	public AudioClip backgroundMusic;	//background music always emits from the camera

	public List<Troop> troops;

}