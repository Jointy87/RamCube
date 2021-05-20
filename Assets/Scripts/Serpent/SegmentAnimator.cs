﻿using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Qbism.Cubes;
using Qbism.PlayerCube;
using UnityEngine;

namespace Qbism.Serpent
{
	public class SegmentAnimator : MonoBehaviour
	{
		//Config paramters
		[Header ("Spawn Juice")]
		[SerializeField] MMFeedbacks spawnJuice = null;
		[Header ("Animation")]
		[SerializeField] Animator animator = null;
		[SerializeField] float lookAroundAnimDelay = 0f, 
			lookUpAnimDelay = 0f, happyWiggleAnimDelay = 0f;

		//Cache
		SegmentExpressionHandler faceHandler = null;
		PlayerAnimator playerAnim = null;

		//States
		bool justSpawned = false;

		private void Awake() 
		{
			faceHandler = GetComponent<SegmentExpressionHandler>();
			playerAnim = FindObjectOfType<PlayerAnimator>();
		}

		private void OnEnable() 
		{
			if (playerAnim != null) playerAnim.onTriggerLandingReaction += TriggerSquish;	
		}

		public void Spawn()
		{
			justSpawned = true;
			spawnJuice.Initialization();
			spawnJuice.PlayFeedbacks();
			animator.SetTrigger("SpawnWiggle");
		}

		private IEnumerator TriggerLookAround() //Called from animation event
		{
			yield return new WaitForSeconds(lookAroundAnimDelay);
			animator.SetTrigger("LookAround");
		}

		private void TriggerSquish()
		{
			if (justSpawned) animator.SetTrigger("Squish");
		}

		private IEnumerator TriggerLookUp() //Called from animation event
		{
			//Take the player landing + squish reaction duration into account for look up delay
			yield return new WaitForSeconds(lookUpAnimDelay);
			animator.SetTrigger("LookUp");
		}

		private IEnumerator TriggerHappyWiggle() //Called from animation event
		{
			yield return new WaitForSeconds(happyWiggleAnimDelay);
			animator.SetTrigger("HappyWiggle");
		}

		private void TriggerPlayerLanding() //Called from animation event
		{
			playerAnim.TriggerFalling(.5f);
		}

		private void TriggerPlayerLookDown() //Called from animation event
		{
			playerAnim.TriggerLookDown();
		}

		private void TriggerPlayerWiggle() // Called from animation event
		{
			playerAnim.TriggerWiggle();
		}

		private void OnDisable()
		{
			if (playerAnim != null) playerAnim.onTriggerLandingReaction -= TriggerSquish;
		}
	}
}
