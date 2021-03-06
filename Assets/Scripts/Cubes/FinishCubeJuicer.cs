﻿using System;
using System.Collections;
using System.Collections.Generic;
using Qbism.PlayerCube;
using UnityEngine;
using UnityEngine.Events;

namespace Qbism.Cubes
{
	public class FinishCubeJuicer : MonoBehaviour
	{
		//Config parameters
		[Header("SFX")]
		[SerializeField] AudioClip succesClip = null;
		[SerializeField] AudioClip failClip = null;
		[Header("VFX")]
		[SerializeField] GameObject glowEmitParticles;
		[Header("Impact VFX")]
		[SerializeField] ParticleSystem impactParticle;
		[SerializeField] MeshRenderer[] meshes = null;
		[Header("Charging VFX")]
		[SerializeField] ParticleSystem chargingBeamsParticles;
		[SerializeField] float glowIncrease = .03f, glowIncreaseInterval = .05f;

		//Cache
		public AudioSource source { get; private set; }
		Animator animator;
		PlayerFartLauncher farter;
		PlayerCubeMover player;

		//Actions, events, delegates etc
		public UnityEvent onFinishEvent = new UnityEvent();
		public event Action onSpawnFriends;

		public delegate bool GetCompleteDel();
		public GetCompleteDel onFinishCheck;

		private void Awake() 
		{
			source = GetComponentInChildren<AudioSource>();
			animator = GetComponent<Animator>();
			player = FindObjectOfType<PlayerCubeMover>();
			farter = player.GetComponent<PlayerFartLauncher>();
		}

		private void OnEnable()
		{
			if (farter != null)
			{
				farter.onDoneFarting += StartBreakingAnimation;
				farter.onStartFarting += StartGlowing;
			} 
		}

		private void Update()
		{
			if(!onFinishCheck()) CheckForGlowDeactivation();
		}

		private void CheckForGlowDeactivation()
		{
			if (Vector3.Distance(transform.position, player.transform.position) < 1.25f)
				glowEmitParticles.SetActive(false);
			else if (glowEmitParticles.activeSelf == false) glowEmitParticles.SetActive(true);
		}

		public void DeactivateGlow()
		{
			glowEmitParticles.SetActive(false);
		}

		public void PlaySuccesSound()
		{
			source.clip = succesClip;
			onFinishEvent.Invoke();
		}

		public void PlayFailSound()
		{
			source.clip = failClip;
			onFinishEvent.Invoke();
		}

		private void StartBreakingAnimation() 
		{
			animator.SetTrigger("Open");
		}

		private void StartChargeFX() //Called from animation event
		{
			chargingBeamsParticles.Play();
		}

		private void StopChargeFX() //Called from animation event
		{
			chargingBeamsParticles.Stop();
		}

		private void Impact() //Called from animation event
		{
			impactParticle.Play();

			foreach (MeshRenderer mesh in meshes)
			{
				mesh.enabled = false;
			}

			onSpawnFriends();
		}

		private void StartGlowing()
		{
			StartCoroutine(EnableGlowMesh());
		}

		private IEnumerator EnableGlowMesh() 
		{
			meshes[1].enabled = true;

			while (meshes[1].materials[3].GetFloat("Glow_Alpha") < 1)
			{
				foreach (Material mat in meshes[1].materials)
				{
					float current = mat.GetFloat("Glow_Alpha");
					mat.SetFloat("Glow_Alpha", current + glowIncrease);
				}
				yield return new WaitForSeconds(glowIncreaseInterval);
			}

			meshes[0].enabled = false;
		}

		private void OnDisable()
		{
			if (farter != null)
			{
				farter.onDoneFarting -= StartBreakingAnimation;
				farter.onStartFarting -= StartGlowing;
			}
		}
	}
}
