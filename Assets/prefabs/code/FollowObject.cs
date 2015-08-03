using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {

	public Vector2 OffSet;
	public Transform Following;

	public void Update()
	{
		transform.position = Following.transform.position + new Vector3(OffSet.x, OffSet.y);
	}
}
