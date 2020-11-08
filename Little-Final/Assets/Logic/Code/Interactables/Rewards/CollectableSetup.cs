﻿using System;
using UnityEngine;
using UpdateManagement;

public class CollectableSetup
{
	private readonly ActionOverTime setupAction;
	private readonly CollectableRotator collectable;
	private readonly Vector3 originPosition,
							originalScale,
							objScale;
	private readonly Action onFinishCallBack;
	public CollectableSetup(CollectableRotator collectable, Vector3 objScale, float setupTime, Action onFinishCallBack)
	{
		this.collectable = collectable;
		this.onFinishCallBack = onFinishCallBack;
		this.objScale = objScale;
		originalScale = collectable.transform.localScale;
		setupAction = new ActionOverTime(setupTime, Setup, true);
		originPosition = collectable.transform.position;
	}

	public void StartSetup() => setupAction.StartAction();

	private void Setup(float bezier)
	{
		collectable.transform.localScale = Vector3.Lerp(originalScale, objScale, BezierHelper.GetSinBezier(bezier));
		collectable.transform.position = Vector3.Lerp(originPosition, collectable.GetPosition(0), BezierHelper.GetSinBezier(bezier));
		if (bezier >= 1)
		{
			collectable.enabled = true;
			onFinishCallBack();
		}
	}
}
