﻿using System.Collections;
using System.Collections.Generic;
using Qbism.MoveableCubes;
using Qbism.PlayerCube;
using UnityEngine;
using UnityEngine.Events;

namespace Qbism.Cubes
{
	public class BoostCube : MonoBehaviour, ICubeInfluencer
	{
		//Config parameters
		[SerializeField] float boostSpeed = 30f;
		[SerializeField] GameObject boostCollider = null;
		[SerializeField] Transform colliderSpawnPos = null;

		public UnityEvent onBoostEvent = new UnityEvent();

		public void PrepareAction(GameObject cube)
		{
			CreateSpawnCollider(cube);

			if (cube.GetComponent<PlayerCubeMover>()) StartCoroutine(ExecuteActionOnPlayer(cube));
			else if (cube.GetComponent<FeedForwardCube>()) StartCoroutine(ExecuteActionOnFF(cube));
		}

		public void PrepareActionForMoveable(Transform side, Vector3 turnAxis, 
			Vector2Int posAhead, GameObject cube, FloorCube prevCube)
		{
			CreateSpawnCollider(cube);
			StartCoroutine(ExecuteActionOnMoveable(side, turnAxis, posAhead, cube, prevCube));
		}

		private void CreateSpawnCollider(GameObject cube)
		{
			GameObject spawnedCollider = Instantiate(boostCollider,
							colliderSpawnPos.position, transform.localRotation);

			spawnedCollider.transform.parent = cube.transform;
			spawnedCollider.GetComponent<Collider>().enabled = true;
		}

		public IEnumerator ExecuteActionOnPlayer(GameObject cube)
		{
			var mover = cube.GetComponent<PlayerCubeMover>();

			mover.input = false;
			cube.GetComponent<Rigidbody>().isKinematic = true;
			mover.isBoosting = true;

			onBoostEvent.Invoke();

			while (mover.isBoosting)
			{
				cube.transform.position +=
					transform.TransformDirection(Vector3.forward) * boostSpeed * Time.deltaTime;
				yield return null;
			}

			mover.RoundPosition();
			mover.UpdateCenterPosition();

			cube.GetComponent<Rigidbody>().isKinematic = false;

			mover.CheckFloorInNewPos();
		}

		public IEnumerator ExecuteActionOnFF(GameObject ffCube)
		{
			var ff = ffCube.GetComponent<FeedForwardCube>();

			ff.isBoosting = true;

			while (ff.isBoosting)
			{
				ffCube.transform.position +=
					transform.TransformDirection(Vector3.forward) * boostSpeed * Time.deltaTime;
				yield return null;
			}

			if (ff.gameObject.activeSelf)
			{
				ff.RoundPosition();
				ff.isBoosting = false;

				ff.CheckFloorInNewPos();
			}
		}

		public IEnumerator ExecuteActionOnMoveable(Transform side, Vector3 turnAxis, 
			Vector2Int posAhead, GameObject cube, FloorCube prevCube)
		{
			var moveable = cube.GetComponent<MoveableCube>();
			Vector2Int launchPos = moveable.FetchGridPos();
			
			moveable.isBoosting = true;

			while (moveable.isBoosting)
			{
				moveable.transform.position +=
					transform.TransformDirection(Vector3.forward) * boostSpeed * Time.deltaTime;
				yield return null;
			}
			moveable.RoundPosition();
			moveable.UpdateCenterPosition();

			Vector2Int cubePos = moveable.FetchGridPos();

			MoveableCubeHandler moveHandler = FindObjectOfType<MoveableCubeHandler>();
			
			if(moveHandler.CheckDeltaY(cubePos, launchPos) > 0)
			{
				side = moveable.up;
				turnAxis = Vector3.right;
				posAhead = cubePos + Vector2Int.up;
			}
			else if(moveHandler.CheckDeltaY(cubePos, launchPos) < 0)
			{
				side = moveable.down;
				turnAxis = Vector3.left;
				posAhead = cubePos + Vector2Int.down;
			}
			else if(moveHandler.CheckDeltaX(cubePos, launchPos) < 0)
			{
				side = moveable.left;
				turnAxis = Vector3.forward;
				posAhead = cubePos + Vector2Int.left;
			}
			else if(moveHandler.CheckDeltaX(cubePos, launchPos) > 0)
			{
				side = moveable.right;
				turnAxis = Vector3.back;
				posAhead = cubePos + Vector2Int.right;
			}

			moveable.CheckFloorInNewPos(side, turnAxis, posAhead, moveable, cubePos, launchPos);
		}
	}
}
