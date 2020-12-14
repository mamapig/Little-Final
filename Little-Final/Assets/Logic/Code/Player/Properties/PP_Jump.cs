﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Properties/Player/Jump", fileName = "PP_Jump")]
public class PP_Jump : ScriptableObject
{
	#region Singleton
	private static PP_Jump instance;

	public static PP_Jump Instance
	{
		get
		{
			if (!instance)
			{
				//PP_Jump[] propertiesFound = Resources.LoadAll<PP_Jump>("");
				//if (propertiesFound.Length >= 1) instance = propertiesFound[0];
				instance = Resources.Load<PP_Jump>("PP_Jump");
			}
			if (!instance)
			{
				Debug.Log("No Jump Properties found in Resources folder");
				instance = CreateInstance<PP_Jump>();
			}
			return instance;
		}
	}

	#endregion

#pragma warning disable 0169
#pragma warning disable 0649
	[SerializeField]
	[Range(0, 100, step: .5f)]
	private float jumpForce,
					longJumpForce,
					jumpSpeed,
					longJumpSpeed,
					fallMultiplier,
					lowJumpMultiplier,
					turnSpeedInTheAir,
					turnSpeedLongJump;
	[SerializeField]
	[UnityEngine.Range(0, 3)]
	private float coyoteTime,
		distanceToGround;
#pragma warning restore 0169
#pragma warning restore 0649

	#region Getters
	public float JumpForce => jumpForce;
	public float LongJumpForce => longJumpForce;
	public float JumpSpeed => jumpSpeed;
	public float LongJumpSpeed => longJumpSpeed;
	public float FallMultiplier => fallMultiplier;
	public float LowJumpMultiplier => lowJumpMultiplier;
	public float TurnSpeedInTheAir => turnSpeedInTheAir;
	public float TurnSpeedLongJump => turnSpeedLongJump;
	public float CoyoteTime => coyoteTime;
	public float DistanceToGround => distanceToGround;
	#endregion
}
