﻿using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
	[SerializeField]
	[Range(0, 500)]
	protected int stamina;
	public abstract int Stamina { get; }
	public abstract bool ValidateTrigger(PlayerController controller);
	public abstract void Use(PlayerController controller);
}