﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestJump : StateMachineBehaviour
{
	[HideInInspector]
	public Collider landCollider;
	//OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!landCollider)
		{
			Debug_Console.print("no landCollider Available");
			try
			{
				landCollider = animator.GetComponent<TestMovementPlayer>().landCollider;
			}
			catch (NullReferenceException)
			{
				Debug_Console.print("no testMovement on player");
				return;
			}
		}
		landCollider.enabled = true;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.applyRootMotion = true;
	}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK()
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that sets up animation IK (inverse kinematics)
	//}
}
