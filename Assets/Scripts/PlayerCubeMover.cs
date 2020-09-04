﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCubeMover : MonoBehaviour
{
	//Config parameters
	[SerializeField] Transform center;
	public Transform up;
	public Transform down;
	public Transform left;
	public Transform right;
	public int turnStep = 9;

	//Cache
	Rigidbody rb;
	CubeHandler handler;

	//States
	public bool isInBoostPos { get; set; } = true;
	public bool input { get; set;} = true;
	public bool isBoosting { get; set; } = false;

	public Vector2Int tileAbovePos { get; private set; } = new Vector2Int(0, 1);
	public Vector2Int tileBelowPos { get; private set; } = new Vector2Int(0, -1);
	public Vector2Int tileLeftPos { get; private set; } = new Vector2Int(-1, 0);
	public Vector2Int tileRightPos { get; private set; } = new Vector2Int(1, 0);

	FloorCube currentCube = null;

	public event Action onLand;

	private void Awake() 
	{
		rb = GetComponent<Rigidbody>();	
		handler = FindObjectOfType<CubeHandler>();
	}
	private void OnEnable() 
	{
		SwipeDetector.onSwipe += HandleSwipeInput;
	}

	private void Start() 
	{
		UpdatePositions();
	}

	private void HandleSwipeInput(SwipeDetector.SwipeDirection direction)
	{
		if(!input) return;

		if(direction == SwipeDetector.SwipeDirection.up && 
			handler.tileGrid.ContainsKey(FetchCubeGridPos() + tileAbovePos))
			StartCoroutine(Move(up, Vector3.right));

		if (direction == SwipeDetector.SwipeDirection.down &&
			handler.tileGrid.ContainsKey(FetchCubeGridPos() + tileBelowPos))
			StartCoroutine(Move(down, Vector3.left));

		if (direction == SwipeDetector.SwipeDirection.left &&
			handler.tileGrid.ContainsKey(FetchCubeGridPos() + tileLeftPos))
			StartCoroutine(Move(left, Vector3.forward));

		if (direction == SwipeDetector.SwipeDirection.right &&
			handler.tileGrid.ContainsKey(FetchCubeGridPos() + tileRightPos))
			StartCoroutine(Move(right, Vector3.back));
	}

	public void HandleKeyInput(Transform side, Vector3 turnAxis)
	{
		if (!input) return;
		StartCoroutine(Move(side, turnAxis));
	}

	private IEnumerator Move(Transform side, Vector3 turnAxis)
	{
		input = false;
		rb.isKinematic = true;

		var tileToDrop = FetchCubeGridPos();

		for (int i = 0; i < (90 / turnStep); i++)
		{
			transform.RotateAround(side.position, turnAxis, turnStep);
			yield return null;
		}

		RoundPosition();
		UpdatePositions();

		handler.DropTile(tileToDrop);

		rb.isKinematic = false;

		CheckFloorInNewPos();
	}

	public void RoundPosition()
	{
		transform.position = new Vector3(Mathf.RoundToInt(transform.position.x),
			0.5f, Mathf.RoundToInt(transform.position.z));
		
		Quaternion rotation = Quaternion.Euler(Mathf.RoundToInt(transform.rotation.x), 
			Mathf.RoundToInt(transform.rotation.y), Mathf.RoundToInt(transform.rotation.z));
	}

	public void CheckFloorInNewPos()
	{
		FloorCube previousCube = null;

		if(!handler.tileGrid.ContainsKey(FetchCubeGridPos())) return;

		previousCube = currentCube;
		//print("previous cube is " + previousCube);
		
		currentCube = handler.FetchTile(FetchCubeGridPos());
		//print("current cube is " + currentCube);

		if(currentCube != previousCube && onLand != null) onLand();
		
		if(currentCube.FetchType() == CubeTypes.Boosting)
			currentCube.GetComponent<BoostCube>().PrepareBoost(this.gameObject);

		else if (currentCube.FetchType() == CubeTypes.Flipping
			&& currentCube != previousCube)
			currentCube.GetComponent<FlipCube>().StartFlip(this.gameObject);

		else input = true;
	}

	public void UpdatePositions()
	{
		center.position = transform.position;
	}

	public Vector2Int FetchCubeGridPos()
	{
		return new Vector2Int
			(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
	}

	private void OnDisable() 
	{
		SwipeDetector.onSwipe -= HandleSwipeInput;
	}
}
