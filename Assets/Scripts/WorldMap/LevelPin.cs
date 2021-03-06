﻿using System;
using System.Collections;
using System.Collections.Generic;
using Qbism.SceneTransition;
using UnityEngine;

namespace Qbism.WorldMap
{
	public class LevelPin : MonoBehaviour
	{
		//Config parameters
		public LevelIDs levelID;
		public LevelPinUI pinUI;
		[Header ("Unlocking")]
		[SerializeField] float lockedYPos;
		public float unlockedYPos;
		[SerializeField] float raiseStep;
		[SerializeField] float raiseSpeed;
		[Header ("Paths")]
		public Transform pathPoint;
		public LineRenderer[] fullLineRenderers;
		public LineRenderer dottedLineRenderer;

		//Cache
		MeshRenderer mRender;

		//Actions, events, delegates etc
		public event Action<Transform, LineTypes, List<LevelPin>> onPathDrawing;
		public event Action<Transform, List<LineDrawData>, LineRenderer[], LineRenderer> onPathCreation;

		//States
		public bool justCompleted { get; set; } = false;
		bool raising = false;

		private void Awake() 
		{
			mRender = GetComponentInChildren<MeshRenderer>();
		}

		public void CheckRaiseStatus(bool unlocked, bool unlockAnimPlayed)
		{
			if (!unlocked)
			{
				mRender.enabled = false;
				mRender.transform.position = new Vector3 (transform.position.x, lockedYPos, transform.position.z);
			} 

			else if (unlocked && unlockAnimPlayed)
			{
				mRender.enabled = true;
				mRender.transform.position = new Vector3
				(transform.position.x, unlockedYPos, transform.position.z);
			}
		}

		public void CheckPathStatus(LevelStatusData unlock1Data, 
			LevelStatusData unlock2Data, bool completed)
		{
			List<LineDrawData> lineDestList = new List<LineDrawData>();
			AddToList(completed, unlock1Data.unlocked, unlock1Data.unlockAnimPlayed,
				unlock1Data.levelID, unlock1Data.locks, lineDestList);
			AddToList(completed, unlock2Data.unlocked, unlock2Data.unlockAnimPlayed,
			unlock2Data.levelID, unlock2Data.locks, lineDestList);

			onPathCreation(pathPoint, lineDestList, fullLineRenderers, dottedLineRenderer);
		}

		private void AddToList(bool completed, bool unlockStatus, bool unlockAnim, 
			LevelIDs id, int locks, List<LineDrawData> lineDestList)
		{
			if (id == LevelIDs.empty) return;
			LevelPin pin = GetPin(id);

			int sheetLocks = pin.GetComponent<EditorSetPinValues>().locks;
			bool lessLocks = sheetLocks > locks && locks > 0;
			bool noLocks = sheetLocks > locks && locks == 0;

			if((completed && lessLocks) || (!justCompleted && completed && noLocks && !unlockAnim))
			{
				LineDrawData drawData = new LineDrawData();
				drawData.destination = pin.pathPoint;
				drawData.lineType = LineTypes.dotted;
				lineDestList.Add(drawData);
			}

			if (completed && unlockStatus && unlockAnim)
			{
				LineDrawData drawData = new LineDrawData();
				drawData.destination = pin.pathPoint;
				drawData.lineType = LineTypes.full;
				lineDestList.Add(drawData);
			}
		}

		private LevelPin GetPin(LevelIDs id)
		{
			LevelPin[] pins = FindObjectsOfType<LevelPin>();
			LevelPin foundPin = null;

			foreach (LevelPin pin in pins)
			{
				if (pin.levelID != id) continue;
				foundPin = pin;
			}
			return foundPin;
		}

		public void InitiateRaising(bool unlocked, bool unlockAnimPlayed, List<LevelPin> originPins)
		{
			mRender.transform.position = new Vector3
				(transform.position.x, lockedYPos, transform.position.z);

			raising = true;
			StartCoroutine(RaiseCliff(mRender, originPins));
		}

		private IEnumerator RaiseCliff(MeshRenderer mRender, List<LevelPin> originPins)
		{
			mRender.enabled = true;
			GetComponent<LevelPinRaiseJuicer>().PlayRaiseJuice();

			while(raising)
			{
				mRender.transform.position += new Vector3 (0, raiseStep, 0);

				yield return new WaitForSeconds(raiseSpeed);

				if (mRender.transform.position.y >= unlockedYPos)
				{
					raising = false;
					mRender.transform.position = new Vector3(
						transform.position.x, unlockedYPos, transform.position.z);

					GetComponent<LevelPinRaiseJuicer>().StopRaiseJuice();
					pinUI.ShowOrHideUI(true);
					onPathDrawing(pathPoint, LineTypes.full, originPins);
				}
			}
		}

		public void DrawPath(List<LevelPin> originPins)
		{
			onPathDrawing(pathPoint, LineTypes.dotted, originPins);
		}
	}
}
