using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private bool _isFacingRight;
	private CharController2D _controller;
	private float _normalizedHorizontalSpeed;

	public float MaxSpeed = 8;
	public float SpeedAccelationOnGround = 10f;
	public float SpeedAccelationOnAir = 5f;
	public float MovementFactor = 0;
	public int MaxHealth = 100;
	public GameObject OuchEffect;
	public Animator animator;

	public int Health { get; private set;}
	public bool IsDead { get; private set;}

	public void Awake()
	{
		Health = MaxHealth;
		_controller = GetComponent<CharController2D>();
		_isFacingRight = transform.localScale.x > 0;
	}

	public void Update()
	{
		if (!IsDead) 
		{
			handleInput ();
		}

		var momentFactor = _controller.State.IsGrounded ? SpeedAccelationOnGround : SpeedAccelationOnAir;

		if (IsDead)
			_controller.SetHorizontalForce (0);
		else
			_controller.SetHorizontalForce (Mathf.Lerp (_controller.velocity.x, _normalizedHorizontalSpeed * MaxSpeed, Time.deltaTime * momentFactor));	

		if(animator != null)
			animator.SetTrigger("walking");
	}

	public void Kill()
	{
		_controller.HandleCollision = false;
		_controller.SetForce(new Vector2(0, 10));
		this.gameObject.GetComponent<Collider2D>().enabled = false;
		Debug.Log (this.gameObject.GetComponent<Collider2D> ().enabled);

		Health = 0;

		IsDead = true;	
	}

	public void ReSpawnAt(Transform spawnPoint)
	{
		if (!_isFacingRight)
			flip ();

		IsDead = false;	
		this.gameObject.GetComponent<Collider2D>().enabled = true;
		_controller.HandleCollision = true;
		transform.position = spawnPoint.position;

		Health = MaxHealth;
	}

	public void TakeDamage(int damage)
	{
		var obj = Instantiate(OuchEffect, transform.position, transform.rotation);
		Debug.Log("take that you sucker x "+transform.position.x);
		Debug.Log("take that you sucker y "+transform.position.y);
		Debug.Log("take that you sucker  "+transform.position.z);

		//Particle part = Particle(obj);

		//Debug.Log("take that you sucker part x "+obj.transform.position.x);
		//Debug.Log("take that you sucker part y "+obj.position.y);
		//Debug.Log("take that you sucker part z "+obj.position.z);

		Health -= damage;

		if (Health <= 0)
			LevelManager.Instace.KillPlayer ();
	}

	private void handleInput()
	{
		if (Input.GetKey (KeyCode.D)) 
		{
			_normalizedHorizontalSpeed = 1;
			if(!_isFacingRight)
			{
				flip();
			}
		} else if (Input.GetKey (KeyCode.A)) 
		{
			_normalizedHorizontalSpeed = -1;
			if(_isFacingRight)
			{
				flip();
			}
		} else 
		{
			_normalizedHorizontalSpeed = 0;
		}

		if (_controller.CanJump && Input.GetKey (KeyCode.Space)) 
		{
			_controller.Jump();
		}
	}

	private void flip()
	{
		transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		_isFacingRight = transform.localScale.x > 0;
	}
}
