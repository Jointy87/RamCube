﻿using System;
using System.Collections;
using System.Collections.Generic;
using Qbism.PlayerCube;
using Qbism.SceneTransition;
using UnityEngine;

namespace Qbism.Cubes
{
	public class LaserCube : MonoBehaviour
	{
		//Config parameters
		public float distance = 1;
		[SerializeField] Transform laserOrigin = null;
		[SerializeField] LayerMask chosenLayers;
		
		//Cache
		PlayerCubeMover mover;
		SceneHandler loader;
		LaserJuicer juicer;
		CubeHandler cubeHandler;


		//States
		bool shouldTrigger = true;
		float currentLength = 0f;

		//Actions, events, delegates etc
		public event Action<InterfaceIDs> onRewindPulse;

		private void Awake()
		{
			mover = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCubeMover>();
			loader = FindObjectOfType<SceneHandler>();
			juicer = GetComponent<LaserJuicer>();
			cubeHandler = FindObjectOfType<CubeHandler>();
		}

		private void OnEnable() 
		{
			if(mover != null) mover.onSetLaserTriggers += SetLaserTrigger;
		}

		private void Start()
		{
			mover.lasersInLevel = true;
		}

		private void FixedUpdate()
		{
			FireSphereCast();
		}

		private void FireSphereCast()
		{
			RaycastHit[] hits = SortedSphereCasts();

			AdjustBeamLength(hits);

			if (hits.Length > 0 && (mover.input || mover.isBoosting))
			{
				if (hits[0].transform.gameObject.tag == "Player" &&
				Mathf.Approximately(Vector3.Dot(mover.transform.forward, transform.forward), -1))
				{
					// shouldTrigger is to prevent laser from triggering if boosting along laser correct-way-facing
					if(shouldTrigger)
					{
						shouldTrigger = false;
						var fartLauncher = mover.GetComponent<PlayerFartLauncher>();
						bool hasHit = false;
						RaycastHit hit = fartLauncher.FireRayCast(out hasHit);
						if (hasHit) fartLauncher.FireBulletFart();
					}
				}

				else if (hits[0].transform.gameObject.tag == "Player" &&
					!Mathf.Approximately(Vector3.Dot(mover.transform.forward, transform.forward), -1))
				{
					if (shouldTrigger)
					{
						shouldTrigger = false;
						juicer.TriggerDenyJuice(currentLength);
						onRewindPulse(InterfaceIDs.Rewind);
						mover.GetComponent<PlayerStunJuicer>().PlayStunVFX();
						mover.input = false;
						mover.isStunned = true;						
					}
				}
			}

			else
			{
				juicer.TriggerIdleJuice();
			}
		}

		public void CloseEye() //Called from fart particle collision
		{
			juicer.TriggerPassJuice();
			CheckForCubes(transform.forward, 1, (int)(Math.Floor(distance)), false);
		}

		private RaycastHit[] SortedSphereCasts()
		{
			RaycastHit[] hits = Physics.SphereCastAll(laserOrigin.position, .05f,
				transform.forward, distance, chosenLayers , QueryTriggerInteraction.Ignore);
			
			Debug.DrawRay(laserOrigin.position, transform.forward, Color.red, distance);

			float[] hitDistances = new float[hits.Length];

			for (int hit = 0; hit < hitDistances.Length; hit++)
			{
				hitDistances[hit] = hits[hit].distance;
			}

			Array.Sort(hitDistances, hits);

			return hits;
		}

		private void AdjustBeamLength(RaycastHit[] hits)
		{
			float dist;
			if (hits.Length <= 0) dist = distance;
			else dist = hits[0].distance;

			if (dist != currentLength)
			{
				CastDottedLines(dist, distance);
				juicer.AdjustBeamVisualLength(dist);
				currentLength = dist;
			}
		}

		private void CastDottedLines(float dist, float startDist)
		{
			int distRoundDown = (int)(Math.Floor(dist));
			int startDistRoundDown = (int)(Math.Floor(startDist));
			Vector3 laserDir = transform.forward;

			//enables dotted lines on cubes within distance
			CheckForCubes(laserDir, 1, distRoundDown, true);

			//disables dotted lines between actual distance and start distance
			if (startDistRoundDown - distRoundDown > 0)
				CheckForCubes(laserDir, distRoundDown + 1, startDistRoundDown, false);
		}

		private void CheckForCubes(Vector3 laserDir, int iStart, 
			int iCondition, bool enable)
		{
			//checks each int within the laser distance
			for (int i = iStart; i <= iCondition; i++)
			{
				Vector3 checkPos = transform.position + (laserDir * i);

				Vector2Int roundedCheckPos = new Vector2Int
					(Mathf.RoundToInt(checkPos.x), Mathf.RoundToInt(checkPos.z));

				//checks if point has a cube
				if (cubeHandler.CheckFloorCubeDicKey(roundedCheckPos))
				{
					var cube = cubeHandler.FetchCube(roundedCheckPos);
					cube.CastDottedLines(transform.position, enable);
				}
			}
		}

		public void SetLaserTrigger(bool value)
		{
			shouldTrigger = value;
		}

		private void OnDisable()
		{
			if (mover != null) mover.onSetLaserTriggers -= SetLaserTrigger;
		}
	}
}
