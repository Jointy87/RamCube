using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Qbism.SpriteAnimations
{
	public class SpriteEyesAnimator : MonoBehaviour
	{
		//Config parameters
		[SerializeField] bool hasTwitch, hasSquint, hasAnnoyed, hasLooking;

		//Cache
		Animator animator;

		//States
		EyesStates currentEyes = EyesStates.normal;
		const string TO_NORMAL = "ToNormal";
		const string TO_WINK = "ToWink";
		const string TO_SHUT = "ToShut";
		const string TO_ARCH = "ToArched";
		const string TO_TWITCH = "ToTwitch";
		const string TO_SHOCKED = "ToShocked";
		const string TO_CROSS = "ToCross";
		const string TO_SPARKLE = "ToSparkle";
		const string TO_SQUINT = "ToSquint";
		const string TO_ANNOYED = "ToAnnoyed";
		const string TO_LOOKING = "ToLooking";

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
			animStringList.Add(TO_ARCH);
			animStringList.Add(TO_TWITCH);
			animStringList.Add(TO_SHOCKED);
			animStringList.Add(TO_CROSS);
			animStringList.Add(TO_SPARKLE);
			animStringList.Add(TO_SQUINT);
			animStringList.Add(TO_ANNOYED);
			animStringList.Add(TO_LOOKING);
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

			if (state == EyesStates.annoyed && hasAnnoyed)
				ToFirstTierAnim(EyesStates.annoyed, EyesStates.nullz, TO_ANNOYED);
				
			else if (state == EyesStates.annoyed && !hasAnnoyed)
				ToFirstTierAnim(EyesStates.cross, EyesStates.nullz, TO_CROSS);

			if (state == EyesStates.wink) 
				ToFirstTierAnim(EyesStates.wink, EyesStates.nullz, TO_WINK);

			if (state == EyesStates.shut) 
				ToFirstTierAnim(EyesStates.shut, EyesStates.nullz, TO_SHUT);

			if (state == EyesStates.arched) 
				ToFirstTierAnim(EyesStates.arched, EyesStates.nullz, TO_ARCH);

			if (state == EyesStates.twitch && hasTwitch) 
				ToFirstTierAnim(EyesStates.twitch, EyesStates.nullz, TO_TWITCH);
			
			else if (state == EyesStates.twitch && !hasTwitch)
				Debug.LogError("Character does not have twitch eye animation.");

			if (state == EyesStates.shock) 
				ToFirstTierAnim(EyesStates.shock, EyesStates.nullz, TO_SHOCKED);

			if (state == EyesStates.cross) 
				ToFirstTierAnim(EyesStates.cross, EyesStates.nullz, TO_CROSS);

			if (state == EyesStates.sparkle) 
				ToFirstTierAnim(EyesStates.sparkle, EyesStates.nullz, TO_SPARKLE);

			if (state == EyesStates.squint && hasSquint) 
				ToFirstTierAnim(EyesStates.squint, EyesStates.nullz, TO_SQUINT);

			else if (state == EyesStates.squint && !hasSquint)
				Debug.LogError("Character does not have squint eye animation.");

			if (state == EyesStates.looking && hasLooking)
				ToFirstTierAnim(EyesStates.looking, EyesStates.nullz, TO_LOOKING);
			
			else if (state == EyesStates.looking & !hasLooking)
				Debug.LogError("Character does not have looking eye animation.");
		}

		private void SetCurrentEyes()
		{
			var currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
			var currentClipName = currentClipInfo[0].clip.name;

			if (currentClipName == "Eyes_WinkToNormal" || currentClipName == "Eyes_ShutToNormal" ||
				currentClipName == "Eyes_ArchedToNormal" || currentClipName == "Eyes_Normal" ||
				currentClipName == "Eyes_CrossShutToNormal" || currentClipName == "Eyes_AnnoyedToNormal_Seg01")
				currentEyes = EyesStates.normal;
			
			if (currentClipName == "Eyes_NormalToWink") currentEyes = EyesStates.wink;
			if (currentClipName == "Eyes_NormalToShut") currentEyes = EyesStates.shut;
			if (currentClipName == "Eyes_NormalToArched") currentEyes = EyesStates.arched;
			if (currentClipName == "Eyes_Twitching") currentEyes = EyesStates.twitch;
			if (currentClipName == "Eyes_Shocked") currentEyes = EyesStates.shock;
			if (currentClipName == "Eyes_NormalToCrossShut") currentEyes = EyesStates.cross;
			if (currentClipName == "Eyes_Sprakling") currentEyes = EyesStates.sparkle;
			if (currentClipName == "Eyes_Squinted") currentEyes = EyesStates.squint;
			if (currentClipName == "Eyes_Looking_Wall") currentEyes = EyesStates.looking;
			if (currentClipName == "Eyes_NormalToAnnoyed_Seg01") currentEyes = EyesStates.annoyed;
		}

		private void ToBaseAnim()
		{
			if (currentEyes == EyesStates.normal) return;

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

		//TO DO: Reactivate if we ever get tier2 anims again
		// private void ToSecondTierAnim(EyesStates state1, EyesStates state2, string trigger)
		// {
		// 	if (currentEyes == state1) return;

		// 	if (currentEyes != EyesStates.normal && currentEyes != state2)
		// 	{
		// 		ToBaseAnim();
				
		// 		string newTrigger = null;
		// 		if (state1 == EyesStates.laughShut) newTrigger = TO_SHUT;
		// 		if (state1 == EyesStates.laughArched) newTrigger = TO_ARCH;

		// 		ToFirstTierAnim(state2, state1, newTrigger);
		// 	}

		// 	animator.SetTrigger(trigger);
		// 	currentEyes = state1;
		// }
	}
}
