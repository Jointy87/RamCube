﻿using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Qbism.Cubes;
using Qbism.Rewind;
using UnityEngine;

namespace Qbism.General
{
	public class InterfacePulser : MonoBehaviour
	{
		//Config parameters
		[SerializeField] InterfaceIDs interfaceID;
		[SerializeField] MMFeedbacks pulser = null;

		//Cache
		FinishCube finishCube = null;
		RewindHandler rewinder = null;
		LaserCube[] lasers = null;
		TimeBody[] bodies = null;
		OutOfBounds[] outOfBounds = null;

		private void Awake()
		{
			finishCube = FindObjectOfType<FinishCube>();
			rewinder = FindObjectOfType<RewindHandler>();
			lasers = FindObjectsOfType<LaserCube>();
			bodies = FindObjectsOfType<TimeBody>();
			outOfBounds = FindObjectsOfType<OutOfBounds>();
		}

		private void OnEnable()
		{
			if (finishCube != null)
			{
				finishCube.onRewindPulse += InitiatePulse;
				finishCube.onStopRewindPulse += StopPulse;
			}

			if (rewinder != null) rewinder.onStopRewindPulse += StopPulse;

			foreach (LaserCube laser in lasers)
			{
				if (laser != null) laser.onRewindPulse += InitiatePulse;
			}
			
			foreach (TimeBody body in bodies)
			{
				if (body != null) body.onStopRewindPulse += StopPulse;
			}

			foreach (OutOfBounds oob in outOfBounds)
			{
				if (oob != null) oob.onRewindPulse += InitiatePulse;
			}
		}

		private void InitiatePulse(InterfaceIDs id)
		{
			if (id == interfaceID)
			{
				pulser.Initialization();
				pulser.PlayFeedbacks();
			}
		}

		private void StopPulse(InterfaceIDs id)
		{
			pulser.StopFeedbacks();
		}

		private void OnDisable()
		{
			if (finishCube != null)
			{
				finishCube.onRewindPulse -= InitiatePulse;
				finishCube.onStopRewindPulse -= StopPulse;
			}

			if (rewinder != null) rewinder.onStopRewindPulse -= StopPulse;

			foreach (LaserCube laser in lasers)
			{
				if (laser != null) laser.onRewindPulse -= InitiatePulse;
			}

			foreach (TimeBody body in bodies)
			{
				if (body != null) body.onStopRewindPulse -= StopPulse;
			}

			foreach (OutOfBounds oob in outOfBounds)
			{
				if (oob != null) oob.onRewindPulse -= InitiatePulse;
			}
		}
	}

}