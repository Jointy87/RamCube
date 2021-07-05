using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Qbism.PlayerCube
{
	public class PlayerSpriteEyesAnimator : MonoBehaviour
	{
		//Cache
		Animator animator;

		//States
		EyesStates currentEyes = EyesStates.normal;
		const string TO_NORMAL = "ToNormal";
		const string TO_WINK = "ToWink";
		const string TO_SHUT = "ToShut";
		const string TO_LAUGH_SHUT = "ToLaughingShut";
		const string TO_ARCH = "ToArched";
		const string TO_LAUGH_ARCH = "ToLaughingArched";
		const string TO_TWITCH = "ToTwitch";
		const string TO_SHOCKED = "ToShocked";
		const string TO_CROSS = "ToCross";
		const string TO_SPARKLE = "ToSparkle";
		const string TO_SQUINT = "ToSquint";

		List<string> animStringList = new List<string>();

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		private void Start() 
		{
			animStringList.Add(TO_NORMAL);
			animStringList.Add(TO_WINK);
			animStringList.Add(TO_SHUT);
			animStringList.Add(TO_LAUGH_SHUT);
			animStringList.Add(TO_ARCH);
			animStringList.Add(TO_LAUGH_ARCH);
			animStringList.Add(TO_TWITCH);
			animStringList.Add(TO_SHOCKED);
			animStringList.Add(TO_CROSS);
			animStringList.Add(TO_SPARKLE);
			animStringList.Add(TO_SQUINT);
		}

		//state1 is always the state you're going to.
		public void SetEyes(EyesStates state)
		{
			SetCurrentEyes();

			foreach (var anim in animStringList)
			{
				animator.ResetTrigger(anim);
			}

			if (state == EyesStates.normal) ToBaseAnim();

			if (state == EyesStates.wink) 
				ToFirstTierAnim(EyesStates.wink, EyesStates.nullz, TO_WINK);

			if (state == EyesStates.shut) 
				ToFirstTierAnim(EyesStates.shut, EyesStates.laughShut, TO_SHUT);

			if (state == EyesStates.arched) 
				ToFirstTierAnim(EyesStates.arched, EyesStates.laughArched, TO_ARCH);

			if (state == EyesStates.twitch) 
				ToFirstTierAnim(EyesStates.twitch, EyesStates.nullz, TO_TWITCH);

			if (state == EyesStates.shock) 
				ToFirstTierAnim(EyesStates.shock, EyesStates.nullz, TO_SHOCKED);

			if (state == EyesStates.cross) 
				ToFirstTierAnim(EyesStates.cross, EyesStates.nullz, TO_CROSS);

			if (state == EyesStates.sparkle) 
				ToFirstTierAnim(EyesStates.sparkle, EyesStates.nullz, TO_SPARKLE);

			if (state == EyesStates.squint) 
				ToFirstTierAnim(EyesStates.squint, EyesStates.nullz, TO_SQUINT);
		}

		private void SetCurrentEyes()
		{
			var currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
			var currentClipName = currentClipInfo[0].clip.name;

			if (currentClipName == "Eyes_WinkToNormal" || currentClipName == "Eyes_ShutToNormal" ||
				currentClipName == "Eyes_ArchedToNormal" || currentClipName == "Eyes_Normal")
				currentEyes = EyesStates.normal;
			
			if (currentClipName == "Eyes_NormalToWink") currentEyes = EyesStates.wink;
			if (currentClipName == "Eyes_NormalToShut") currentEyes = EyesStates.shut;
			if (currentClipName == "Eyes_NormalToArched") currentEyes = EyesStates.arched;
			if (currentClipName == "Eyes_Twitching") currentEyes = EyesStates.twitch;
			if (currentClipName == "Eyes_Shocked") currentEyes = EyesStates.shock;
			if (currentClipName == "Eyes_CrossShut") currentEyes = EyesStates.cross;
			if (currentClipName == "Eyes_Sprakling") currentEyes = EyesStates.sparkle;
			if (currentClipName == "Eyes_Squinted") currentEyes = EyesStates.squint;

			if (currentClipName == "Eyes_LaughingShut") currentEyes = EyesStates.laughShut;
			if (currentClipName == "Eyes_LaughingArched") currentEyes = EyesStates.laughArched;
		}

		private void ToBaseAnim()
		{
			if (currentEyes == EyesStates.normal) return;

			if (currentEyes == EyesStates.laughArched) ToFirstTierAnim(EyesStates.arched, EyesStates.laughArched, TO_ARCH);
			if (currentEyes == EyesStates.laughShut) ToFirstTierAnim(EyesStates.shut, EyesStates.laughShut, TO_SHUT);

			animator.SetTrigger(TO_NORMAL);
			currentEyes = EyesStates.normal;
		}

		private void ToFirstTierAnim(EyesStates state1, EyesStates state2, string trigger)
		{
			if (currentEyes == state1) return;

			if (state2 == EyesStates.nullz) 
			{
				if (currentEyes != EyesStates.normal) ToBaseAnim();
			}
			else
			{
				if (currentEyes != EyesStates.normal && currentEyes != state2) ToBaseAnim();
			}

			animator.SetTrigger(trigger);
			currentEyes = state1;
		}

		private void ToSecondTierAnim(EyesStates state1, EyesStates state2, string trigger)
		{
			if (currentEyes == state1) return;

			if (currentEyes != EyesStates.normal && currentEyes != state2)
			{
				ToBaseAnim();

				if (state1 == EyesStates.laughShut)
					ToFirstTierAnim(EyesStates.shut, EyesStates.laughShut, TO_SHUT);

				if (state1 == EyesStates.laughArched)
					ToFirstTierAnim(EyesStates.arched, EyesStates.laughArched, TO_ARCH);
			}

			animator.SetTrigger(trigger);
			currentEyes = state1;
		}
	}
}
