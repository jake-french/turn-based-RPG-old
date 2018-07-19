using UnityEngine;
using UnityEditor;
using System.Collections;

public class CreateScriptableObjects : MonoBehaviour {

	[MenuItem("Assets/Create/Troop")]
	public static void CreateTroopAsset() {
		Troop newTroop = ScriptableObject.CreateInstance<Troop>();

		AssetDatabase.CreateAsset(newTroop, "Assets/Prefabs/ScriptObjects/newTroop.asset");
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();
		Selection.activeObject = newTroop;
	}

}
