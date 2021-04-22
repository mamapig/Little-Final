﻿using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterMovement;
using VarelaAloisio.UpdateManagement.Runtime;

public enum BodyEvent
{
	TRIGGER,
	JUMP,
	CLIMB,
	LAND
}

public delegate void BodyEvents(BodyEvent typeOfEvent);
[RequireComponent(typeof(Rigidbody))]
public class Player_Body : MonoBehaviour, IUpdateable, IBody
{
	#region Variables
	const string INTERACTABLE_LAYER = "Interactable";


	#region Public
	public event BodyEvents BodyEvents;
	public Collider landCollider;
	#endregion

	#region Serialized
	[Header("Audio")]
	[SerializeField]
	AudioClip[] _soundEffects = null;

	[SerializeField]
	private float maxSpeed;
	[SerializeField]
	float _xMinAngle = 5;
	[SerializeField]
	float _xMaxAngle = 92,
		_yMinAngle = 45,
		_yMaxAngle = 90,
		_inTheAirTimeToOff = .005f,
		_colTimeToOff = 0,
		climbTimeToOff = 0.05f;
	#endregion

	#region Private

	AudioManager audioManager;

	Rigidbody rb;

	private GameObject lastFloor;
	ContactPoint lastContact;
	Vector3 _collisionAngles;
	private float jumpForce;
	public float safeDot;
	#endregion

	#region Getters
	public Vector3 Position => transform.position;
	public Vector3 Velocity { get => rb.velocity; set => rb.velocity = value; }
	public GameObject GameObject => gameObject;
	public Vector3 LastFloorNormal { get; set; }
	#endregion

	#region Setters
	public bool IsInTheAir => flags[Flag.IN_THE_AIR];
	#endregion

	#region Flags
	enum Flag
	{
		IN_THE_AIR,
		JUMP_REQUEST,
		COLLIDING,
		TOUCHING_PICKABLE,
		COL_COUNTING,
		WEIRD_COL_COUNTING,
		IN_COYOTE_TIME
	}

	private readonly Dictionary<Flag, bool> flags = new Dictionary<Flag, bool>();

	#endregion


	#endregion

	#region Unity
	void Start()
	{
		UpdateManager.Subscribe(this);
		rb = GetComponent<Rigidbody>();

		SetupFlags();
	}
	public void OnUpdate()
	{
		ControlJump();
		AccelerateFall();
		if (rb.velocity.magnitude > maxSpeed)
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
	}
	#endregion

	#region Private
	/// <summary>
	/// Setups the flags
	/// </summary>
	void SetupFlags()
	{
		foreach (var flag in (Flag[])Enum.GetValues(typeof(Flag)))
		{
			flags.Add(flag, false);
		}
	}

	/// <summary>
	/// This function lets the body know if it should move or if moving would cause trouble
	/// </summary>
	/// <param name="direction">Player's input</param>
	/// <returns></returns>
	private bool CheckCollisionAngle(Vector3 direction)
	{
		//Set Variables
		Vector3 horizontalCollisionNormal = lastContact.normal;
		horizontalCollisionNormal.y = 0;

		_collisionAngles = new Vector2(Vector3.Angle(direction, horizontalCollisionNormal), Vector3.Angle(transform.up, lastContact.normal));

		//Conditions
		bool _conditionA = (_collisionAngles.y > _yMinAngle && _collisionAngles.y < _yMaxAngle);
		bool _conditionB = (_collisionAngles.x > _xMinAngle && _collisionAngles.x < _xMaxAngle);

		//Decide return
		if (!flags[Flag.COLLIDING]) return true;
		return !(_conditionA && _conditionB);
	}

	/// <summary>
	/// Makes the player jump
	/// </summary>
	private void ControlJump()
	{
		if (flags[Flag.JUMP_REQUEST])
		{
			flags[Flag.JUMP_REQUEST] = false;
			//Physics
			Vector3 newVel = rb.velocity;
			newVel.y = 0;
			rb.velocity = newVel;
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			//Event
			BodyEvents?.Invoke(BodyEvent.JUMP);

			//Sound
			PlaySound(0);
		}
	}

	/// <summary>
	/// Accelerates the velocity of the player while falling to eliminate feather falling effet
	/// </summary>
	void AccelerateFall()
	{
		if (rb.velocity.y < .5 && rb.velocity.y > -10)
		{
			rb.velocity += Vector3.up * Physics2D.gravity.y * (PP_Jump.FallMultiplier - 1) * Time.deltaTime;
		}
	}

	public void SetDrag(float value) => rb.drag = value;

	public float GetDrag() => rb.drag;

	void PlaySound(int Index)
	{
		try
		{
			//audioManager.PlayCharacterSound(_soundEffects[Index]);

		}
		catch (NullReferenceException)
		{
			print("PBODY: AudioManager not found");
		}
	}

	#endregion

	#region Public
	/// <summary>
	/// Sets the Velocity for the Player
	/// </summary>
	/// <param name="input"></param>
	public void MoveHorizontally(Vector3 direction, float speed)
	{
		//if (CheckCollisionAngle(direction))
		//{
			rb.velocity = direction * speed + rb.velocity.y * Vector3.up;
			Debug.DrawRay(transform.position, rb.velocity, Color.cyan);
		//}
	}

	public void Move(Vector3 direction, float speed) => transform.position += direction * speed * Time.deltaTime;

	public void Jump(float jumpForce)
	{
		this.jumpForce = jumpForce;
		flags[Flag.JUMP_REQUEST] = true;
	}

	/// <summary>
	/// Stops Characters jump to give the user more control
	/// </summary>
	public void StopJump()
	{
		rb.velocity += Vector3.up * Physics2D.gravity.y * (PP_Jump.LowJumpMultiplier - 1) * Time.deltaTime;
	}

	public void Push(Vector3 directionNormalized, float force)
	{
		Push(directionNormalized * force);
	}
	public void Push(Vector3 direction)
	{
		rb.AddForce(direction, ForceMode.Impulse);
	}
	public Collider GetLandCollider()
	{
		return landCollider;
	}
	#endregion

	#region Collisions
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer(INTERACTABLE_LAYER) || other.gameObject.layer == LayerMask.NameToLayer("OnlyForShadows"))
			return;
		else
		{
			FallHelper.AddFloor(other.gameObject);
			flags[Flag.IN_THE_AIR] = false;
			if (lastFloor != other.gameObject)
			{
				lastFloor = other.gameObject;
				Physics.Raycast(Position, -transform.up, out RaycastHit hit, 10);
				LastFloorNormal = hit.normal;
			}
			BodyEvents?.Invoke(BodyEvent.LAND);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer(INTERACTABLE_LAYER))
			return;
		FallHelper.RemoveFloor(other.gameObject);
		flags[Flag.IN_THE_AIR] = true;
		BodyEvents?.Invoke(BodyEvent.JUMP);
	}

	private void OnCollisionStay(Collision collision)
	{
		flags[Flag.COLLIDING] = true;
		lastContact = collision.contacts[0];
	}
	#endregion
}