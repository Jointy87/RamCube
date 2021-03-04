﻿using System.Collections;
using System.Collections.Generic;
using Qbism.Cubes;
using Qbism.PlayerCube;
using Qbism.Rewind;
using Qbism.SceneTransition;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Qbism.Control
{
	public class InputDetector : MonoBehaviour
	{
		//Cache
		CubeHandler handler;
		PlayerCubeMover mover;
		SceneHandler loader;
		RewindHandler rewinder;
		GameControls controls;

		//States
		Vector2 stickValue;
		bool inputting = false;

		private void Awake()
		{
			handler = GetComponent<CubeHandler>();
			mover = FindObjectOfType<PlayerCubeMover>();
			loader = GetComponent<SceneHandler>();
			rewinder = GetComponent<RewindHandler>();
			controls = new GameControls();

			//from Brackeys gamepad setup tut. Don't actually understand Lambda Expressions
			controls.Gameplay.Movement.performed += ctx => stickValue = ctx.ReadValue<Vector2>();
		}
		
		private void OnEnable() 
		{
			controls.Gameplay.Enable();	
		}

		void Update()
		{	
			if(stickValue.x > -.1 && stickValue.x < .1 && 
				stickValue.y > -.1 && stickValue.y < .1) 
				inputting = false;

			if(!inputting) HandleStickValues();
		}

		private void HandleStickValues()
		{
			if((stickValue.x > .1 && stickValue.y > .1) ||
				(stickValue.x > -.05 && stickValue.x < .05 && stickValue.y > .5)) moveUp();

			if((stickValue.x < -.1 && stickValue.y < -.1) ||
				(stickValue.x > -.05 && stickValue.x < .05 && stickValue.y < -.5)) moveDown();

			if((stickValue.x < -.1 && stickValue.y > .1) ||
				(stickValue.y > -.05 && stickValue.y < .05 && stickValue.x < -.5)) moveLeft();

			if((stickValue.x > .1 && stickValue.y < -.1) ||
				(stickValue.y > -.05 && stickValue.y < .05 && stickValue.x > .5))moveRight();
		}

		private void moveUp()
		{
			inputting = true;
			var posAhead = mover.FetchGridPos() + Vector2Int.up;

			if (handler.floorCubeDic.ContainsKey(posAhead)
				&& handler.FetchShrunkStatus(posAhead) == false)
				mover.HandleKeyInput(mover.up, Vector3.right, posAhead);
		}

		private void moveDown()
		{
			inputting = true;
				var posAhead = mover.FetchGridPos() + Vector2Int.down;

				if (handler.floorCubeDic.ContainsKey(posAhead)
					&& handler.FetchShrunkStatus(posAhead) == false)
					mover.HandleKeyInput(mover.down, Vector3.left, posAhead);
		}

		private void moveLeft()
		{
			inputting = true;
				var posAhead = mover.FetchGridPos() + Vector2Int.left;

				if (handler.floorCubeDic.ContainsKey(posAhead)
					&& handler.FetchShrunkStatus(posAhead) == false)
					mover.HandleKeyInput(mover.left, Vector3.forward, posAhead);
		}

		private void moveRight()
		{
			inputting = true;
				var posAhead = mover.FetchGridPos() + Vector2Int.right;

				if (handler.floorCubeDic.ContainsKey(posAhead)
					&& handler.FetchShrunkStatus(posAhead) == false)
					mover.HandleKeyInput(mover.right, Vector3.back, posAhead);
		}

		private void OnDisable() 
		{
			controls.Gameplay.Disable();	
		}
	}
}