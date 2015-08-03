using UnityEngine;
using System.Collections;

public class CharController2D : MonoBehaviour 
{
	private const float SkinWidth = 0.02f;
	private const int TotalHoriRay = 8;
	private const int TotalVertRay = 4;



	private static readonly float SlopeLimitTangent = Mathf.Tan (75f * Mathf.Deg2Rad);

	public LayerMask PlatformMask;
	public ControllerParameters2D Defaultparams;
	public ControllerState2D State {get; private set; }
	public Vector2 velocity { get {return _velocity;} }
	public bool HandleCollision { get; set;}
	public ControllerParameters2D Parameters { get { return _overrideParameters ?? Defaultparams; } }
	public GameObject StandingOn { get; private set;}
	public Vector3 platformvelocity { get; private set; }


	public bool CanJump 
	{
		get 
		{
			if(Parameters.JumpRestrictions == ControllerParameters2D.JumpBehavior.CanJumpAnywhere)
				return _jumpIn <= 0;	

			if(Parameters.JumpRestrictions == ControllerParameters2D.JumpBehavior.CanJumpOnGround)
				return State.IsGrounded;
			return false;
		}
	}


	private Vector2 _velocity;
	private Transform _trasform;
	private Vector3 _localScale;
	private BoxCollider2D _boxCollider;
	private ControllerParameters2D _overrideParameters;
	private float _jumpIn;

	private Vector3 _activeGlobalPlatformpoint,
				_activeLocalPlayformPoint;


	private float 
				  _verticalDistanceBetweenRays,
				  _horizontalDistanceBetweenRays;

	private Vector3 
		_rayCastTopLeft,
		_rayCastBottomRight,
		_rayCastBottomLeft;

	public void Awake()
	{
		HandleCollision = true;
		State = new ControllerState2D ();
		_trasform = transform;
		_localScale = transform.localScale;
		_boxCollider = GetComponent<BoxCollider2D>();	


		var colliderWidth = _boxCollider.size.x * Mathf.Abs (_trasform.localScale.x) * (2 * SkinWidth);
		_horizontalDistanceBetweenRays = colliderWidth / (TotalVertRay - 1);

		var colliderHeight = _boxCollider.size.y * Mathf.Abs (_trasform.localScale.y) * (2 * SkinWidth);
		_verticalDistanceBetweenRays = colliderHeight / (TotalHoriRay - 1);
	}

	public void AddForce(Vector2 force)
	{
		_velocity += force;
	}

	public void SetForce(Vector2 force)
	{
		_velocity = force;
	}
	
	public void SetHorizontalForce(float x)
	{
		_velocity.x = x;
	}

	public void SetVerticalForce(float y)
	{
		_velocity.y = y;
	}

	public void Jump()
	{
		AddForce (new Vector2 (0, Parameters.JumpMagnitude));
		_jumpIn = Parameters.JumpFrequency;
	}

	public void LateUpdate()
	{
		_jumpIn -= Time.deltaTime;
		_velocity.y += Parameters.Gravity * Time.deltaTime;
		Move (velocity * Time.deltaTime);
	}

	private void Move(Vector2 deltaMovement)
	{
		var wasGrounded = State.IsCollidingBelow;
		State.Reset ();
		if (HandleCollision) 
		{
			HandlePlatform();
			CalculatRayOrigin();
			if(deltaMovement.y < 0 && wasGrounded)
			{
				handleVericalSlope(ref deltaMovement);
			}
			if(Mathf.Abs(deltaMovement.x) > .001f)
			{	
				MoveHorizontally(ref deltaMovement);
			}
			//Debug.Log("ahere sucker "+HandleCollision);
			MoveVeritically(ref deltaMovement);
		}

		_trasform.Translate(deltaMovement, Space.World);

		if(Time.deltaTime > 0)
			_velocity = deltaMovement / Time.deltaTime;

		_velocity.x = Mathf.Min (_velocity.x, Parameters.MaxVelocity.x);
		_velocity.y = Mathf.Min (_velocity.y, Parameters.MaxVelocity.y);

		
		if(State.IsMovingUpSlope)
			_velocity.y = 0;

		if(StandingOn != null)
		{
			_activeGlobalPlatformpoint = transform.position;
			_activeLocalPlayformPoint = StandingOn.transform.InverseTransformPoint(transform.position);
		}
	}

	public void HandlePlatform()
	{
		if (StandingOn != null) {
			var newGlobalPlatformPpoint = StandingOn.transform.TransformPoint (_activeLocalPlayformPoint);
			var moveDistence = newGlobalPlatformPpoint - _activeGlobalPlatformpoint;

			if (moveDistence != Vector3.zero) {
				transform.Translate (moveDistence, Space.World);
			}
			platformvelocity = (newGlobalPlatformPpoint - _activeGlobalPlatformpoint) / Time.deltaTime;
		} else 
		{
			platformvelocity = Vector3.zero;
		}

		StandingOn = null;

	}

	public void CalculatRayOrigin()
	{
		var size = new Vector2 (_boxCollider.size.x * Mathf.Abs (_localScale.x), _boxCollider.size.y * Mathf.Abs (_localScale.y))/2;
		var center = new Vector2 (_boxCollider.offset.x * _localScale.x, _boxCollider.offset.y * _localScale.y);

		_rayCastTopLeft = _trasform.position + new Vector3 (center.x - size.x + SkinWidth, center.y + size.y - SkinWidth);
		_rayCastBottomRight = _trasform.position + new Vector3 (center.x + size.x - SkinWidth, center.y - size.y + SkinWidth);
		_rayCastBottomLeft = _trasform.position + new Vector3 (center.x - size.x + SkinWidth, center.y - size.y + SkinWidth);
	}

	private void MoveHorizontally(ref Vector2 deltaMovement)
	{
		var isGoingRight = deltaMovement.x > 0;
		var rayDistence = Mathf.Abs (deltaMovement.x) + SkinWidth;
		var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
		var rayOrigin = isGoingRight ? _rayCastBottomRight : _rayCastBottomLeft;

		for (var i=0; i < TotalHoriRay; i++)
		{
			var rayVector = new Vector2 (rayOrigin.x, rayOrigin.y + (i * _verticalDistanceBetweenRays));
			Debug.DrawRay (rayVector, rayDirection * rayDistence, Color.red);

			var rayCastHit = Physics2D.Raycast (rayVector, rayDirection, rayDistence, PlatformMask);
			if (!rayCastHit)
				continue;

			if (i == 0 && handleHorizontalSlope (ref deltaMovement, Vector2.Angle (rayCastHit.normal, Vector2.up), isGoingRight))
				return;

			deltaMovement.x = rayCastHit.point.x - rayVector.x;
			if (isGoingRight) {
				deltaMovement.x -= SkinWidth;
				State.IsCollidingRight = true;
			} else {
				deltaMovement.x += SkinWidth;
				State.IsCollidingLeft = true;
			}

			if (rayDistence < SkinWidth + 0.0001f)
				break;

		}
	}

	private void MoveVeritically(ref Vector2 deltaMovement)
	{
		var isGoingUp = deltaMovement.y > 0;
		var rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
		var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
		var rayOrigin = isGoingUp ? _rayCastTopLeft : _rayCastBottomLeft;
		
		rayOrigin.x += deltaMovement.x;
		
		var standingOnDistance = float.MaxValue;
		for (var i = 0; i < TotalVertRay; i++)
		{
			var rayVector = new Vector2(rayOrigin.x + (i * _horizontalDistanceBetweenRays), rayOrigin.y);
			Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);
			
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
			if (!raycastHit)
				continue;
			else
			{
				Debug.Log("Is something beneath me??   "+raycastHit.transform.gameObject);
			}
			
			if (!isGoingUp)
			{
				var verticalDistanceToHit = _trasform.position.y - raycastHit.point.y;
				if (verticalDistanceToHit < standingOnDistance)
				{
					standingOnDistance = verticalDistanceToHit;
					StandingOn = raycastHit.collider.gameObject;
				}
			}
			
			deltaMovement.y = raycastHit.point.y - rayVector.y;
			rayDistance = Mathf.Abs(deltaMovement.y);
			
			if (isGoingUp)
			{
				deltaMovement.y -= SkinWidth;
				State.IsCollidingAbove = true;
			}
			else
			{
				deltaMovement.y += SkinWidth;
				State.IsCollidingBelow = true;
			}
			
			if (!isGoingUp && deltaMovement.y > .0001f)
				State.IsMovingUpSlope = true;
			
			if (rayDistance < SkinWidth + .0001f)
				break;
		}
	}

	private void handleVericalSlope(ref Vector2 deltaMovement)
	{
		var center = (_rayCastTopLeft.x + _rayCastBottomRight.x) / 2;
		var direction = -Vector2.up;

		var slopeDistence = SlopeLimitTangent * (_rayCastBottomRight.x - center);
		var slopeRayVector = new Vector2 (center, _rayCastBottomRight.y);

		Debug.DrawRay (slopeRayVector, direction * slopeDistence, Color.yellow);
		var rayCastHit = Physics2D.Raycast(slopeRayVector, direction, slopeDistence, PlatformMask);

		if (!rayCastHit)
			return;

		var isMovingDonwSlope = Mathf.Sign (rayCastHit.normal.x) == Mathf.Sign (deltaMovement.x);
		if (!isMovingDonwSlope)
			return;

		var angle = Vector2.Angle (rayCastHit.normal, Vector2.up);
		if (Mathf.Abs (angle) > 0.0001f)
			return;

		State.IsMovingDownSlope = true;
		State.SlopeAngle = angle;
		deltaMovement.y = rayCastHit.point.y - slopeRayVector.y;
	}
	private bool handleHorizontalSlope(ref Vector2 deltaMovement,float angle,bool isGoingRight )
	{
		if (Mathf.RoundToInt (angle) == 90)
			return false;

		if (angle > Parameters.SlopeLimit) 
		{
			deltaMovement.x = 0 ;
			return true;
		}

		if (deltaMovement.y > 0.07f)
			return true;

		deltaMovement.x += isGoingRight ? -SkinWidth : +SkinWidth;
		deltaMovement.y = Mathf.Abs (Mathf.Tan (angle * Mathf.Deg2Rad) * deltaMovement.x);
		State.IsMovingUpSlope = true;
		State.IsCollidingBelow = true;
		return true;

	}
	private void OnTriggerEnter2D(Collider2D other)
	{
	}
	private void OnTriggerExit2D(Collider2D other)
	{
	}
}
