using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleText : BattleFX {
	public TextMesh myTxt;

	IEnumerator fadeUpwards() {
		int step = 0;
		GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
		Vector3 newDest = new Vector3(transform.position.x, transform.position.y + 15, transform.position.z);
		do {
			step++;
			transform.position = Vector3.Lerp(transform.position, newDest, (Time.deltaTime / 3));
			//Snippet found at: http://forum.unity3d.com/threads/facing-textmesh-to-camera-but.323355/
			transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up);
			myTxt.color = new Color(myTxt.color.r, myTxt.color.g, myTxt.color.b, myTxt.color.a - (3*Time.deltaTime));
			yield return new WaitForSeconds(Time.deltaTime*2.5f);
		}while (step != timeTillFade);
		Destroy (this.gameObject);
	}

}
