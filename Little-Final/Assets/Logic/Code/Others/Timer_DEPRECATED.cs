﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ClockTicking(string ID);

public class Timer_DEPRECATED : MonoBehaviour, IUpdateable_DEPRECATED
{
	#region Variables

	#region Public
	public event ClockTicking ClockTickingEvent;
	#endregion

	#region Getters
	public bool Counting
	{
		get
		{
			return _counting;
		}
	}
	public float CurrentTime
	{
		get
		{
			return _currentTime;
		}
	}
	#endregion

	#region Private
	UpdateManager_DEPRECATED _uManager;
	float _currentTime = 0,
		_totalTime = 0;
	bool _counting;
	string _id;
	#endregion

	#endregion

	#region Unity
	private void Awake()
	{
		_uManager = GameObject.FindObjectOfType<UpdateManager_DEPRECATED>();
		_uManager.AddItem(this);
		ClockTickingEvent = delegate { };
	}
	public void OnUpdate()
	{
		UpdateTimer();
	}
	#endregion

	#region Private
	void UpdateTimer()
	{
		if (!_counting) return;
		_currentTime += Time.deltaTime;
		CheckIfFinished();
	}
	void CheckIfFinished()
	{
		if (_currentTime >= _totalTime)
		{
			_counting = false;
			_currentTime = 0;
			ClockTickingEvent(_id);
			_uManager.RemoveItem(this);
		}
	}
	#endregion

	#region Public
	public void Play()
	{
		_uManager.RemoveItem(this);
		_uManager.AddItem(this);
		_counting = true;
		_currentTime = 0;
	}
	public void Stop()
	{
		_uManager.RemoveItem(this);
		_counting = false;
		_currentTime = 0;
	}
	public void Instantiate(float newTotalTime, string newID)
	{
		_totalTime = newTotalTime;
		_id = newID;
	}

	private void OnDestroy()
	{
		_uManager.RemoveItem(this);
	}
	#endregion
}