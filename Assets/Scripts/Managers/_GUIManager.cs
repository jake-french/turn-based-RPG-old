using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class _GUIManager : MonoBehaviour {
	//PUBLIC
	public _GameManager _gm;		//reference to master manager script
	public bool fadeStatus;			//gloal record of whether fade-in in effect

	//GUI Elements
	public Canvas pauseCanvas, universalCanvas;
	public GameObject quitConfirmPanel;
	public GameObject fadeScreenObj;
	public Text victoryTxt;

	//Battle
	public GameObject arrowTargetObj;

	//AUDIO
	public AudioClip buttonSelectNoise;
	public AudioClip victoryMusic;

	//PRIVATE
	//GUI ELEMENTS
	private int pauseCanvasUses, universalCanvasUses;

	private List<Skill> subMenuSkills = new List<Skill>();			//list of skills found for turn owner in "Skills" submenu
	private List<Skill> subMenuWhiteMagic = new List<Skill>();		//list of skills found for turn owner in "White Magic" submenu
	private List<Skill> subMenuBlackMagic =new List<Skill>();		//list of skills found for turn owner in "Black Magic" subenu

	private GameObject actionMenuCommands, actionMenuUI, genericBattleUI;	//references to battlefields' UI elements

	//AUDIO
	private AudioSource backgroundAudioSrc, soundFXAudioSrc;		//references to the main camera's two audio sources

	//Variables for storing temporary changing values for music crossfade
	private AudioClip temp_FadingMusicClip;
	private bool temp_willLoop;

	//FUNCTIONS
	void Start() {
		pauseCanvas.gameObject.SetActive(false);	//hide pause menu
	}

	//GUI
	private float fadeTime;

	//Force fade from black
	public void applyFadeIn(float time = 3) {
		fadeTime = time;
		StartCoroutine("fadeIn");
	}

	//Force fade to black
	public void applyFadeOut(float time = 3) {
		fadeTime = time;
        if (GameObject.Find("startButton")) {
            GameObject.Find("startButton").SetActive(false);
        }
        if (GameObject.Find("quitButton")) {
            GameObject.Find("quitButton").SetActive(false);
        }
        StartCoroutine("fadeOut");
	}

	//Fade from black coroutine
	IEnumerator fadeIn() {
		float curTime = 0;
		Image fadeImg = fadeScreenObj.GetComponent<Image>();
		fadeScreenObj.SetActive(true);
		do {
			curTime += Time.deltaTime;
			fadeImg.color = Color.Lerp (fadeImg.color, Color.clear, Time.deltaTime);	//gradually change color to clear over time
			yield return new WaitForSeconds(Time.deltaTime);
		}while (curTime < fadeTime);
		fadeStatus = false;

		//disable UI if not in use
		universalCanvasUses -= 1;
		if (universalCanvasUses == 0) {
			universalCanvas.gameObject.SetActive(false);
		}
	}

	//Fade to black coroutine
	IEnumerator fadeOut() {
		universalCanvasUses += 1;
		universalCanvas.gameObject.SetActive(true);

		float curTime = 0;
		Image fadeImg = fadeScreenObj.GetComponent<Image>();
		fadeScreenObj.SetActive(true);
		fadeStatus = true;
		do {
			curTime += Time.deltaTime;
			fadeImg.color = Color.Lerp (fadeImg.color, Color.black, Time.deltaTime);	//gradually change color to black over time
			yield return new WaitForSeconds(Time.deltaTime);
		}while (curTime < fadeTime);
	}

	//Audio Functions

	//Finds the SFX and Music audio sources for a level
	public void getAudioSources() {
		AudioSource[] sources = GameObject.FindGameObjectWithTag("MainCamera").GetComponentsInChildren<AudioSource>();	//Finds all audio sources
		foreach(AudioSource a in sources) {
			//If current audio source is the background music source
			if (a.gameObject.name == "_backgroundMusic"){
				backgroundAudioSrc = a;
			//If current audio source is the SFX source
			}else if (a.gameObject.name == "_soundFX") {
				soundFXAudioSrc = a;
			}
		}
	}

	//Sets the background music based on level
	public void setLevelAudio() {
		LevelInfo tempLevelInfo = _gm._level.curLevel;
		//If level has reference to background music
		if (tempLevelInfo.backgroundMusic != null) {
			backgroundAudioSrc.clip = tempLevelInfo.backgroundMusic;		//set clip to clip from level information
			backgroundAudioSrc.loop = true;			//ensure background music loops
			if (!backgroundAudioSrc.isPlaying) {
				backgroundAudioSrc.Play ();			//play background music if not currently
			}
		} else {
			Debug.Log ("Level does not contain background music!");
		}
	}

	//fades music in over time
	public void forceLevelAudio(AudioClip clip, bool loop = true) {
		if (backgroundAudioSrc.isPlaying) {
			temp_FadingMusicClip = clip;
			temp_willLoop = loop;
			StartCoroutine("FadeAudioSources");
		}
	}

	//Coroutine for crossfading music over time
	IEnumerator FadeAudioSources() {
		do {
			backgroundAudioSrc.volume = backgroundAudioSrc.volume - (2 * Time.deltaTime);	//reduce volume of current music over time
			yield return new WaitForSeconds(Time.deltaTime);
		}while(backgroundAudioSrc.volume != 0);
		backgroundAudioSrc.Stop();		//stop current music
		yield return new WaitForSeconds(0.5f);
		backgroundAudioSrc.loop = temp_willLoop;
		backgroundAudioSrc.clip = temp_FadingMusicClip;
		backgroundAudioSrc.Play();		//play new music
		do {
			backgroundAudioSrc.volume = backgroundAudioSrc.volume + (2 * Time.deltaTime);	//increase volume of new music over time
		}while (backgroundAudioSrc.volume < 0.5f);
		backgroundAudioSrc.volume = 0.5f;	//put value of background music to default 50%
	}

	//Plays a sound effect
	public void playSoundFX(AudioClip clip) {
		if (soundFXAudioSrc != null) {
			soundFXAudioSrc.clip = clip;
			soundFXAudioSrc.Play();
		}else{
			Debug.LogError ("Scene is missing sound FX audio source!");
		}
	}

	//Triggers fading out of music
	public void musicFadeOut() {
		StartCoroutine ("fadeMusicOut");
	}

	//Coroutine for music fading out
	IEnumerator fadeMusicOut() {
		do {
			backgroundAudioSrc.volume = backgroundAudioSrc.volume - Time.deltaTime;		//reduce volume over time
			yield return new WaitForSeconds (Time.deltaTime);
		} while (backgroundAudioSrc.volume != 0);
		backgroundAudioSrc.Stop ();
		yield return new WaitForSeconds (0.5f);
	}

	//GUI Functions

	#region General

	//UNUSED
	public void triggerPauseMenu() {
		if (!_gm.paused) {
			Debug.Log ("Pausing game!");
		}else{
			Debug.Log ("Un-pausing game!");
		}
	}

	//Displays the quit confirmation dialog
	public void showQuitConfirmation() {
		Debug.Log ("Quit Confirmation Required!");
		universalCanvasUses += 1;
		universalCanvas.gameObject.SetActive(true);
		quitConfirmPanel.SetActive(true);
	}

	//Hides the quit confirmation dialog
	public void hideQuitConfirmation() {
		quitConfirmPanel.SetActive(false);
		universalCanvasUses -= 1;
		//Checks if other elements are using the universal UI canvas
		if (universalCanvasUses == 0) {
			universalCanvas.gameObject.SetActive(false);
		}
		Debug.Log("Quitting was aborted!");
	}

	//Shows the victory message
	public void showVictory() {
		universalCanvasUses += 1;
		universalCanvas.gameObject.SetActive (true);
		victoryTxt.gameObject.SetActive (true);
	}

	//Hides the victory message
	public void hideVictory() {
		universalCanvasUses -= 1;
		if (universalCanvasUses == 0) {
			universalCanvas.gameObject.SetActive(false);
		}
		victoryTxt.gameObject.SetActive (false);
	}

	#endregion

	#region Battle
	[HideInInspector]
	public Battlefield bf;

	//Retrieves the GUI elements from a battle type level
	public void getBattleGUIElements(Battlefield bf) {
		this.bf = bf;

		actionMenuCommands = GameObject.FindGameObjectWithTag ("actionMenuCommands");
		actionMenuUI = GameObject.FindGameObjectWithTag("actionMenuUI");
		genericBattleUI = GameObject.FindGameObjectWithTag("genericBattleUI");

		showGenericBattleUI();
		hideActionMenu();
	}

	//Shows the party status menu and information bar
	public void showGenericBattleUI() {
		bf.infoBox.transform.parent.gameObject.SetActive(true);
		genericBattleUI.SetActive(true);
		updatePartyValues();
	}

	//Hides the party status menu and information bar
	public void hideGenericBattleUI() {
		bf.infoBox.transform.parent.gameObject.SetActive(false);
		genericBattleUI.SetActive(false);
	}

	//Updates the values for HP and MP in the party status menu
	public void updatePartyValues() {
		GameObject[] members = GameObject.FindGameObjectsWithTag("partyMemberBattleUI");
		//Foreach member in player's party
		foreach(GameObject m in members) {
			ActorBase vals = null;
			//Gets the values from a specific member of party
			if (m.transform.parent.gameObject.name == "party01") {
				vals = bf.party[0].actor.GetComponent<Actor>()._base;
			}else if (m.transform.parent.gameObject.name == "party02") {
				vals = bf.party[1].actor.GetComponent<Actor>()._base;
			}else if (m.transform.parent.gameObject.name == "party03") {
				vals = bf.party[2].actor.GetComponent<Actor>()._base;
			}

			m.transform.GetChild (0).GetComponent<Text>().text = vals.name;		//displays the party member's name
			m.transform.GetChild (2).GetComponent<Text>().text = vals.curHealth + "/" + vals.maxHealth;		//displays the party member's current HP aganist max HP
			m.transform.GetChild(4).GetComponent<Text>().text = vals.curMana + "/" + vals.maxMana;		//displays the party member's current MP aganist max MP
		}
	}

	//Shows the base level actions for that actor, also recieves skills for other menus
	public void showBaseMenu(GameObject actor) {
		hideActionMenu();

		List<Skill> mySkills = _gm._battle.getSkillsFor(actor);
		List<Skill> skillsToAdd = new List<Skill>();

		showActionMenu();

		//Reset values of script's saved skills
		subMenuSkills = new List<Skill>();
		subMenuBlackMagic = new List<Skill>();
		subMenuWhiteMagic = new List<Skill>();

		foreach(Skill s in mySkills) {
			//Add to list
			if (s.skillType == SkillType.NONE) {
				skillsToAdd.Add (s);
			}else{
				//Player has skill for a sub-menu
				if (s.skillType == SkillType.Skill) {
					Debug.Log ("Added a skill to skill submenu: " + s.name);
					subMenuSkills.Add (s);
				}else if (s.skillType == SkillType.Black_Magic) {
					Debug.Log ("Added a skill to black magic submenu: " + s.name);
					subMenuBlackMagic.Add(s);
				}else if (s.skillType == SkillType.White_Magic) {
					Debug.Log ("Added a skill to white magic submenu: " + s.name);
					subMenuWhiteMagic.Add(s);
				}
			}
		}
		Debug.Log ("SM Skills: " + subMenuWhiteMagic.Count + ", BM Skills: " + subMenuBlackMagic.Count + ", Other Skills: " + subMenuSkills.Count);

		//Create button for each base menu skill
		foreach(Skill s in skillsToAdd) {
			GameObject so = Instantiate<GameObject>(_gm._battle.actionButtonPrefab);
			so.transform.SetParent(actionMenuCommands.transform);
			BattleComandButton sbcb = so.GetComponent<BattleComandButton>();
			sbcb.myButton = so.GetComponent<Button>();
			sbcb.myButton.onClick.AddListener(delegate() {sbcb.buttonClicked();} );
			if (s.cost != 0) {
				sbcb.myText.text = s.name + "   " + s.cost;
			}else{
				sbcb.myText.text = s.name;
			}
			sbcb.actorOwner = actor;
			sbcb.myLinkedSkill = s;
			sbcb.myLinkedMenu = -1;
			sbcb.bf = bf;
			if (actor.GetComponent<ActorBase>().curMana < s.cost) {
				//Turn owner cannot afford cost of this skill currently.
				so.GetComponent<Button>().image.color = Color.gray;
			}
			Debug.Log ("Created command for skill: " + s.name);
		}

		//Add buttons for the submenus
		//SKILLS
		if (subMenuSkills.Count > 0) {
			GameObject suo = Instantiate<GameObject>(_gm._battle.actionButtonPrefab);
			suo.transform.SetParent (actionMenuCommands.transform);
			BattleComandButton subbcb = suo.GetComponent<BattleComandButton>();
			subbcb.myButton = subbcb.GetComponent<Button>();
			subbcb.myButton.onClick.AddListener(delegate() {subbcb.buttonClicked();});
			subbcb.myText.text = "Skills";
			subbcb.actorOwner = actor;
			subbcb.myLinkedSkill = null;
			subbcb.myLinkedMenu = 0;
			subbcb.bf = bf;
			Debug.Log ("Created skills submenu button!");
		}
		//BLACK MAGIC
		if (subMenuBlackMagic.Count > 0) {
			GameObject suo = Instantiate<GameObject>(_gm._battle.actionButtonPrefab);
			suo.transform.SetParent (actionMenuCommands.transform);
			BattleComandButton subbcb = suo.GetComponent<BattleComandButton>();
			subbcb.myButton = subbcb.GetComponent<Button>();
			subbcb.myButton.onClick.AddListener(delegate() {subbcb.buttonClicked();});
			subbcb.myText.text = "Black Magic";
			subbcb.actorOwner = actor;
			subbcb.myLinkedSkill = null;
			subbcb.myLinkedMenu = 1;
			subbcb.bf = bf;
			Debug.Log ("Created black magic submenu button!");
		}
		//WHITE MAGIC
		if (subMenuWhiteMagic.Count > 0) {
			GameObject suo = Instantiate<GameObject>(_gm._battle.actionButtonPrefab);
			suo.transform.SetParent (actionMenuCommands.transform);
			BattleComandButton subbcb = suo.GetComponent<BattleComandButton>();
			subbcb.myButton = subbcb.GetComponent<Button>();
			subbcb.myButton.onClick.AddListener(delegate() {subbcb.buttonClicked();});
			subbcb.myText.text = "White Magic";
			subbcb.actorOwner = actor;
			subbcb.myLinkedSkill = null;
			subbcb.myLinkedMenu = 2;
			subbcb.bf = bf;
			Debug.Log ("Created white magic submenu button!");
		}

		Debug.Log ("Adding required Items button!");
		GameObject io = Instantiate<GameObject>(_gm._battle.actionButtonPrefab);
		io.transform.SetParent (actionMenuCommands.transform);
		BattleComandButton bcb = io.GetComponent<BattleComandButton>();
		bcb.myButton = io.GetComponent<Button>();
		bcb.myButton.onClick.AddListener(delegate() {bcb.buttonClicked();});
		bcb.myText.text = "Items";
		bcb.actorOwner = actor;
		bcb.myLinkedSkill = null;
		bcb.myLinkedMenu = 3;
		bcb.bf = bf;
		Debug.Log ("Created Items submenu button!");

		bf.turnStage = TurnStage.SELECTING_ACTION;
		bf.curSkill = null;
		bf.curTargetType = TargetType.NULL;
		bf.cameFromSubMenu = false;

		Debug.Log ("Added base menu skills for actor: " + actor.GetComponent<Actor>().name);
	}

	//Shows the submenu actions for the actor
	public void showSubMenu(GameObject actor, string subMenu) {
		hideActionMenu();

		int children = actionMenuCommands.transform.childCount;
		for (int i = 0; i < children - 1; i++) {
			Destroy (actionMenuCommands.transform.GetChild (i));
		}
		Debug.Log ("Removed old menu");

		bf.turnStage = TurnStage.SELECTING_ACTION_SUBMENU;
		bf.cameFromSubMenu = true;
		bf.curSkill = null;
		bf.curTargetType = TargetType.NULL;
		bf.lastSubMenu = subMenu;

		if (subMenu.ToLower().Trim () != "items") {
			List<Skill> skillsMenu = null;
			if (subMenu.ToLower().Trim () == "skill") {
				skillsMenu = subMenuSkills;
			}else if (subMenu.ToLower().Trim () == "blackmagic"){
				skillsMenu = subMenuBlackMagic;
			}else if (subMenu.ToLower().Trim() == "whitemagic") {
				skillsMenu = subMenuWhiteMagic;
			}
			
			foreach(Skill s in skillsMenu) {
				Debug.Log ("Adding skill: " + s.name);
				GameObject so = Instantiate<GameObject>(_gm._battle.actionButtonPrefab);
				so.transform.SetParent(actionMenuCommands.transform);
				BattleComandButton bcb = so.GetComponent<BattleComandButton>();
				bcb.myButton.onClick.AddListener(() => {bcb.buttonClicked();} );
				if (s.cost != 0) {
					bcb.myText.text = s.name + "   " + s.cost;
				}else{
					bcb.myText.text = s.name;
				}
				bcb.actorOwner = actor;
				bcb.myLinkedSkill = s;
				bcb.myLinkedMenu = -1;
				bcb.myIcon = s.icon;
				bcb.bf = bf;
			}
			Debug.Log ("Added skills for loaded sub-menu: " + subMenu + " for actor: " + actor.GetComponent<ActorBase>().name);

			showActionMenu();
		}else{
			//Get list of usuable in combat items from inventory
			//Use a larger scroll rect of multiple columns with numbers of items
			//Use same setup for targetting as skills for items and use the turnstage to keep the navigation working

			Debug.Log ("Loaded usuable item inventory for actor: " + actor.GetComponent<ActorBase>().name);
		}
	}
		
	public void showActionMenu() {
		actionMenuUI.SetActive(true);
		actionMenuCommands.SetActive(true);
	}

	public void hideActionMenu() {
		actionMenuUI.SetActive(false);
		if (actionMenuCommands != null) {
			foreach(Transform child in actionMenuCommands.transform) {
				if (child.GetComponent<BattleComandButton>()) {
					Destroy (child.gameObject);
				}
			}

			//BattleComandButton[] buttons = actionMenuCommands.GetComponentsInChildren<BattleComandButton>();
			//foreach(BattleComandButton bcb in buttons) {
			//	Destroy (bcb.gameObject);
			//}
		}
	}

	//Called by battlefield after marking specific targets. Creates arrow targets above marked.
	public void markTargets() {
		if (!bf.isEnemy(bf.getTurnOwner())) {
			List<BattleTarget> targets = bf.getMarkedActors();
			
			foreach(BattleTarget bt in targets) {
				bt.myArrow = Instantiate<GameObject>(arrowTargetObj);
				bt.myArrow.transform.SetParent (bt.arrowPos.parent.transform);
				bt.myArrow.transform.position = bt.arrowPos.position;
				bt.myArrow.transform.rotation.Set(90, 0, 0, 0);
			}
		}
	}

	#endregion
}
