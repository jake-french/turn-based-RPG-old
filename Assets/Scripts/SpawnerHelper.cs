using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SpawnerHelper : MonoBehaviour {

	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 1.5f);
	}
}
