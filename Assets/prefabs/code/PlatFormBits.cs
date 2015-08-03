using UnityEngine;
using System.Collections;

public class PlatFormBits : MonoBehaviour {

	void OnTriggerEnter (Collider info)
	{
		Debug.Log ("GameObject : "+gameObject);
	}
}
