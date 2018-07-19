using UnityEngine;
using System.Collections;

public class BattleFX : MonoBehaviour {
	public int timeTillFade;

	// Use this for initialization
	public void run() {
		StartCoroutine("fadeAway");
	}

	private IEnumerator fadeAway() {
		int step = 0;
		do {
			step++;
			yield return new WaitForSeconds(Time.deltaTime);
		}while (step != timeTillFade);
		Destroy (this.gameObject);
	}
}
