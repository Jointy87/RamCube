﻿using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Qbism.MoveableCubes;
using Qbism.SceneTransition;
using UnityEngine;
using UnityEngine.Events;

namespace Qbism.PlayerCube
{
	public class PlayerCubeMover : MonoBehaviour, IActiveCube
	{
		//Config parameters
		[SerializeField] Transform center = null;
		public Transform up = null;
		public Transform down = null;
		public Transform left = null;
		public Transform right = null;
		[SerializeField] int turnStep = 18;
		[SerializeField] float timeStep = 0.01f;
		[SerializeField] float lowerStep = 2.5f;
		

		//Cache
		MoveableCubeHandler moveHandler;
		MoveableCube[] moveableCubes;
		PlayerCubeJuicer playerJuicer;

		//States
		public bool isInBoostPos { get; set; } = true;
		public bool input { get; set; } = true;
		public bool isBoosting { get; set; } = false;
		public bool isTurning { get; set; } = false;
		bool initiatedByPlayer = true;
		public bool isMoving { get; set; } = false;
		public bool lasersInLevel { get; set; } = false;

		//Actions, events, delegates etc
		public event Action<Vector2Int> onCubeShrink;
		public event Action<Vector2Int, GameObject, Transform, Vector3, Vector2Int> onFloorCheck;
		public event Action onRecordStart;
		public event Action<Vector3, Quaternion, Vector3> onInitialRecord;
		public event Action onInitialFloorCubeRecord;
		public event Action<bool> onSetLaserTriggers;

		public UnityEvent onLoweringEvent = new UnityEvent();

		private void Awake()
		{
			moveHandler = FindObjectOfType<MoveableCubeHandler>();
			moveableCubes = FindObjectsOfType<MoveableCube>();
			playerJuicer = GetComponent<PlayerCubeJuicer>();
		}

		private void OnEnable() 
		{
			if (moveableCubes != null)
			{
				foreach (MoveableCube cube in moveableCubes)
				{
					cube.onPlayerPosCheck += FetchGridPos;
					cube.onActivatePlayerMove += InitiateMoveFromOther;
				}
			}
		}

		private void Start()
		{
			UpdateCenterPosition();
		}

		public void HandleSwipeInput(Transform side, Vector3 turnAxis, Vector2Int posAhead)
		{
			if (!input) return;
			initiatedByPlayer = true;
			StartCoroutine(Move(side, turnAxis, posAhead));
		}

		public void HandleKeyInput(Transform side, Vector3 turnAxis, Vector2Int posAhead)
		{
			if (!input) return;
			initiatedByPlayer = true;
			StartCoroutine(Move(side, turnAxis, posAhead));
		}

		private void InitiateMoveFromOther(MoveableCube cube, Transform side, Vector3 turnAxis, Vector2Int posAhead)
		{
			SetSide(cube, ref side, ref posAhead);

			initiatedByPlayer = false;
			StartCoroutine(Move(side, turnAxis, posAhead));
		}

		public IEnumerator Move(Transform side, Vector3 turnAxis, Vector2Int posAhead)
		{
			yield return new WaitForSeconds(.1f); //Added to ensure laser has time to register presence

			var cubeToShrink = FetchGridPos();

			isMoving = true;

			if (initiatedByPlayer)
			{
				onInitialRecord(transform.position, transform.rotation, transform.localScale);
				onInitialFloorCubeRecord();
				moveHandler.InitialRecordMoveables();
			} 

			onRecordStart();
			if(lasersInLevel) onSetLaserTriggers(true);

			CheckPosAhead(posAhead, turnAxis);

			input = false;

			playerJuicer.PlayFlipFeedbacks();
			yield return new WaitForSeconds(playerJuicer.preFlipFeedbackDuration);

			for (int i = 0; i < (90 / turnStep); i++)
			{
				transform.RotateAround(side.position, turnAxis, turnStep);
				yield return new WaitForSeconds(timeStep);
			}

			playerJuicer.PlayPostFlipFeedbacks();

			RoundPosition();
			UpdateCenterPosition();
			isMoving = false;
			onCubeShrink(cubeToShrink);

			CheckFloorInNewPos(side, turnAxis, posAhead);
		}

		public void InitiateLowering(Vector2Int cubePos)
		{
			Vector3 targetPos = new Vector3(transform.position.x,
				transform.position.y - 1, transform.position.z);
			float step = lowerStep * Time.deltaTime;

			StartCoroutine(LowerCube(targetPos, step, cubePos));
		}

		private IEnumerator LowerCube(Vector3 targetPos, float step, Vector2Int cubePos)
		{
			isMoving = true;
			input = false;
			onLoweringEvent.Invoke();

			while (transform.position.y > targetPos.y)
			{
				transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
				yield return timeStep;
			}

			transform.localScale = new Vector3(1, 1, 1);

			RoundPosition();

			AudioSource source = GetComponentInChildren<AudioSource>();
			yield return new WaitWhile(() => source.isPlaying);

			FindObjectOfType<SceneHandler>().RestartLevel();
		}

		public void RoundPosition()
		{
			transform.position = new Vector3(Mathf.RoundToInt(transform.position.x),
				Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

			Quaternion rotation = Quaternion.Euler(Mathf.RoundToInt(transform.rotation.x),
				Mathf.RoundToInt(transform.rotation.y), Mathf.RoundToInt(transform.rotation.z));
		}

		private void CheckPosAhead(Vector2Int posAhead, Vector3 turnAxis)
		{
			if(moveHandler.CheckMoveableCubeDicKey(posAhead))
			{
				moveHandler.StartRecordingMoveables();
				moveHandler.ActivateMoveableCube(posAhead, turnAxis, FetchGridPos());
			}
		}

		public void CheckFloorInNewPos(Transform side, Vector3 turnAxis, Vector2Int posAhead)
		{
			onFloorCheck(FetchGridPos(), this.gameObject, side, turnAxis, posAhead);
		}

		public void UpdateCenterPosition()
		{
			center.position = transform.position;
		}

		public Vector2Int FetchGridPos()
		{
			Vector2Int roundedPos = new Vector2Int
				(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

			return roundedPos;
		}

		private void SetSide(MoveableCube cube, ref Transform side, ref Vector2Int posAhead)
		{
			if (side == cube.up)
			{
				side = up;
				posAhead += Vector2Int.up;
			}
			if (side == cube.down)
			{
				side = down;
				posAhead += Vector2Int.down;
			}
			if (side == cube.left)
			{
				side = left;
				posAhead += Vector2Int.left;
			}
			if (side == cube.right)
			{
				side = right;
				posAhead += Vector2Int.right;
			}
		}

		public int CheckDeltaX(Vector2Int posA, Vector2Int posB)
		{
			return posA.x - posB.x;
		}

		public int CheckDeltaY(Vector2Int posA, Vector2Int posB)
		{
			return posA.y - posB.y;
		}

		private void SetPlayerInput(bool value)
		{
			input = value;
		}

		private void OnDisable()
		{
			if (moveableCubes != null)
			{
				foreach (MoveableCube cube in moveableCubes)
				{
					cube.onPlayerPosCheck -= FetchGridPos;
					cube.onActivatePlayerMove -= InitiateMoveFromOther;
				}
			}
		}
	}
}