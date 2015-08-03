using UnityEngine;
using System.Collections;

public class GiveDamageToPlayer : MonoBehaviour {

	public int DamageToGive = 10;

	private Vector2
		_velocity,
		_lastPosition;

	public void LateUpdate()
	{
		_velocity = (_lastPosition - (Vector2)transform.position) / Time.deltaTime;
		_lastPosition = transform.position;
	}

	public void OnTriggerEnter2D(Collider2D other)
	{

		var player = other.GetComponent<Player> ();

		if (player == null)
			return;

		player.TakeDamage (DamageToGive);	
		var controller = player.GetComponent<CharController2D> ();
		var totalVelocity = controller.velocity + _velocity;
		controller.SetForce(new Vector2 (
			-1 * Mathf.Sign(totalVelocity.x) * Mathf.Clamp(Mathf.Abs(totalVelocity.x)*1,2,3),
		    -1 * Mathf.Sign(totalVelocity.y) * Mathf.Clamp(Mathf.Abs(totalVelocity.y)*1,2,3)));

		FloatingText.Show(string.Format ("-{0}! ", DamageToGive), "GiveDamageText", new FromWorldPointTextPositioner (Camera.main, transform.position, 1.5f, 50));
	}
}
