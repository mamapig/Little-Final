﻿using CharacterMovement;
using Player.PlayerInput;
using Player.Properties;
using Player.Stamina;
using UnityEngine;
using VarelaAloisio.UpdateManagement.Runtime;

namespace Player.States
{
	public class Climb : State
	{
		private IBody _body;

		private Vector3 _originPosition,
			_targetPosition;

		private Quaternion _originRotation,
			_targetRotation;

		private StaminaConsumer _consumer;

		private ActionOverTime _getInPosition;
		private CountDownTimer _afterCliff;
		private Transform _currentWall;
		private readonly int _groundMask;

		public Climb() => _groundMask = LayerMask.GetMask("default", "Floor");

		public override void OnStateEnter(PlayerController controller, int sceneIndex)
		{
			base.OnStateEnter(controller, sceneIndex);
			MyTransform = controller.transform;
			_body = controller.Body;

			if (controller.Stamina.FillState < 1)
			{
				controller.ChangeState<Jump>();
				return;
			}

			_consumer
				= new StaminaConsumer(
				controller.Stamina,
				PP_Climb.StaminaPerSecond,
				sceneIndex,
				PP_Climb.StaminaConsumingDelay
			);
			_consumer.Start();
			
			controller.Stamina.StopRefilling();

			controller.OnClimb();
			controller.GetComponent<Rigidbody>().isKinematic = true;

			ClimbHelper.CanClimb(
				MyTransform.position,
				MyTransform.forward,
				PP_Climb.MaxDistanceToTriggerClimb,
				PP_Climb.MaxClimbAngle,
				out var hit);

			_getInPosition =
				new ActionOverTime(
					PP_Climb.ClimbPositioningTime,
					GetInPosition,
					sceneIndex,
					true
				);
			ResetPosition(hit);
			_afterCliff =
				new CountDownTimer(
					PP_Climb.ClimbPositioningTime,
					controller.ChangeState<Jump>,
					sceneIndex
				);
		}

		public override void OnStateUpdate()
		{
			if (_getInPosition.IsRunning)
				return;
			var myPosition = MyTransform.position;
			var input = InputManager.GetHorInput();
			var moveDirection = MyTransform.right * input.x + MyTransform.up * input.y;

			if (IsTouchingGround() && moveDirection.y < 0)
				moveDirection.y = 0;

			//Move
			if (moveDirection.magnitude != 0 && ClimbHelper.CanMove(myPosition,
				MyTransform.forward,
				moveDirection.normalized,
				PP_Climb.MaxClimbDistanceFromCorners,
				PP_Climb.MaxDistanceToTriggerClimb,
				PP_Climb.MaxClimbAngle,
				out var hit))
			{
				if (!hit.transform.Equals(_currentWall))
					ResetPosition(hit);
				else
				{
					if (!_consumer.IsConsuming)
						_consumer.Start();
					_body.Move(moveDirection, PP_Climb.ClimbSpeed);
					//Rotation
					Physics.Raycast(myPosition, MyTransform.forward, out var forwardHit,
						PP_Climb.MaxDistanceToTriggerClimb, ~LayerMask.GetMask("NonClimbable", "Interactable"));
					MyTransform.rotation = Quaternion.LookRotation(-forwardHit.normal).normalized;
					Debug.DrawLine(myPosition, hit.point, Color.yellow);
				}
			}
			//Cliff
			else if (Mathf.Approximately(Vector3.Dot(moveDirection, MyTransform.up), 1)
			         &&
			         ClimbHelper.CanClimbUp(
				         myPosition,
				         MyTransform.up,
				         MyTransform.forward,
				         PP_Climb.MaxClimbDistanceFromCorners,
				         PP_Climb.MaxDistanceToTriggerClimb,
				         out var cliffHit))
			{
				Debug.DrawRay(cliffHit.point, cliffHit.normal / 4, Color.blue, 2);
				GetOverCliff(cliffHit);
			}
			//Stop
			else
				_consumer.Stop();

			if (!InputManager.CheckClimbInput()
			    || Controller.Stamina.FillState < 1)
			{
				Controller.ChangeState<Jump>();
				Controller.OnJump();
			}

			Controller.RunAbilityList(Controller.AbilitiesOnWall);
		}

		public override void OnStateExit()
		{
			_getInPosition.StopAction();
			_consumer.Stop();
			Controller.Stamina.ResumeRefilling();
			Controller.GetComponent<Rigidbody>().isKinematic = false;
			var newRotation = Quaternion.identity;
			newRotation.y = MyTransform.rotation.y;
			MyTransform.rotation = newRotation;
		}

		private void ResetPosition(RaycastHit hit)
		{
			_originPosition = MyTransform.position;
			_originRotation = MyTransform.rotation;

			_targetRotation = Quaternion.LookRotation(-hit.normal);
			_targetPosition = hit.point - MyTransform.forward * PP_Climb.ClimbingPositionOffset;

			_currentWall = hit.transform;

			_getInPosition.StartAction();
		}

		private void GetOverCliff(RaycastHit hit)
		{
			_originPosition = MyTransform.position;
			var myRotation = MyTransform.rotation;
			_originRotation = myRotation;
			_targetPosition = hit.point + (MyTransform.up * MyTransform.localScale.y / 5);
			_targetRotation = myRotation;

			_getInPosition.StartAction();
			_afterCliff.StartTimer();
		}

		private void GetInPosition(float bezier)
		{
			MyTransform.position = Vector3.Lerp(_originPosition, _targetPosition, bezier);
			MyTransform.rotation = Quaternion.Slerp(_originRotation, _targetRotation, bezier);
		}

		private bool IsTouchingGround()
		{
			if (!Physics.Raycast(MyTransform.position,
					Vector3.down,
					PP_Climb.MaxClimbDistanceFromCorners,
					_groundMask
				)
			)
				return false;
			Debug.DrawRay(
				MyTransform.position,
				Vector3.down * PP_Climb.MaxClimbDistanceFromCorners,
				Color.green);
			return true;
		}
	}
}