using UnityEngine;
using System.Collections;

public class PathedProjectile : MonoBehaviour 
{
	private Transform _destination;
	private float _speed;


	public void Initialize(Transform destination, float speed)
	{
		_destination = destination;
		_speed = speed;
	}

	public void Update()
	{
		//Debug.Log ("Sheru : "+_destination);
		if(_destination)
		{
			transform.position = Vector3.MoveTowards (transform.position, _destination.position, Time.deltaTime * _speed);
			var distenceSquared = (_destination.transform.position - transform.position).sqrMagnitude;
			
			if (distenceSquared > 0.1f * 0.1f)
				return;
			Destroy (gameObject);
		}
	}
}
