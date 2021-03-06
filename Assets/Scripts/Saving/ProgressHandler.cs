using System;
using System.Collections;
using System.Collections.Generic;
using Qbism.WorldMap;
using UnityEngine;

namespace Qbism.Saving
{
	public class ProgressHandler : MonoBehaviour
	{
		//Cache
		SerpentProgress serpProg = null;
		PinSelectionTracker pinSelTrack = null;
		LevelPinInitiator initiator = null;
		LevelPinUI[] pinUIs = null;

		//States
		public LevelIDs currentLevelID { get; set; }
		public Biomes currentBiome { get; set ; }
		public bool currentHasSegment { get ; set ; }
		public List<LevelStatusData> levelDataList;
		public List<LevelPin> levelPinList;

		private void Awake() 
		{
			serpProg = GetComponent<SerpentProgress>();
			BuildLevelDataList();
			LoadProgHandlerData();
		}

		private void BuildLevelDataList()
		{
			foreach (LevelIDs ID in Enum.GetValues(typeof(LevelIDs)))
			{
				LevelStatusData newData = new LevelStatusData();
				newData.levelID = ID;
				levelDataList.Add(newData);
			}
		}

		public void FixMapDelegateLinks()
		{
			initiator = FindObjectOfType<LevelPinInitiator>();
			if (initiator != null) initiator.onPinInitation += InitiatePins;

			pinSelTrack = FindObjectOfType<PinSelectionTracker>();
			if (pinSelTrack != null) pinSelTrack.onSavedPinFetch += FetchCurrentPin;

			pinUIs = FindObjectsOfType<LevelPinUI>();
			foreach (LevelPinUI pinUI in pinUIs)
			{
				if (pinUI != null)
				{
					pinUI.onSetCurrentData += SetCurrentData;
					pinUI.onFetchLevelData += FetchLevelDataList;
					pinUI.onFetchLevelPins += FetchLevelPinList;
				} 
			}
		}

		public void BuildLevelPinList()
		{
			foreach (LevelPin pin in FindObjectsOfType<LevelPin>())
			{
				levelPinList.Add(pin);
			}

			levelPinList.Sort(PinsByID);
		}

		private int PinsByID(LevelPin A, LevelPin B)
		{
			if(A.levelID < B.levelID) return -1;
			else if(A.levelID > B.levelID) return 1;
			return 0;
		}

		public void InitiatePins() //Done every time world map is loaded
		{
			levelPinList.Clear();
			BuildLevelPinList();
			HandleLevelPins();
		}

		public void HandleLevelPins()
		{
			for (int i = 0; i < levelPinList.Count; i++)
			{
				if (levelPinList[i].levelID != levelDataList[i].levelID)
				{
					Debug.LogError("levelPinList " + levelPinList[i].levelID + " and levelDataList "
					+ levelDataList[i].levelID + " are not in the same order.");
					continue;
				}

				if(levelDataList[i].levelID == LevelIDs.a_01)
				{
					if (!levelDataList[i].unlocked) levelDataList[i].unlocked = true;
					if (!levelDataList[i].unlockAnimPlayed) levelDataList[i].unlockAnimPlayed = true; 
				}

				EditorSetPinValues pinValues = levelPinList[i].GetComponent<EditorSetPinValues>();
				LevelIDs unlock1ID = pinValues.levelUnlock_1;
				LevelIDs unlock2ID = pinValues.levelUnlock_2;
				LevelStatusData unlock1Data = FetchUnlockStatusData(unlock1ID);
				LevelStatusData unlock2Data = FetchUnlockStatusData(unlock2ID);
				List<LevelPin> originPins = new List<LevelPin>();
				AddOriginPins(levelPinList[i], originPins);
				
				var sheetLocks = pinValues.locks;
				var savedLocks = levelDataList[i].locks;
				var dottedAnimPlayed = levelDataList[i].dottedAnimPlayed;
				var unlockAnimPlayed = levelDataList[i].unlockAnimPlayed;
				var unlocked = levelDataList[i].unlocked;
				var completed = levelDataList[i].completed;
				var pathDrawn = levelDataList[i].pathDrawn;
				
				levelPinList[i].justCompleted = false;
				if (completed && !pathDrawn) levelPinList[i].justCompleted = true;
				bool lessLocks = (sheetLocks > savedLocks) && savedLocks != 0;

				levelPinList[i].CheckRaiseStatus(unlocked, unlockAnimPlayed);
				levelPinList[i].CheckPathStatus(unlock1Data, unlock2Data, completed);

				levelPinList[i].pinUI.SelectPinUI();

				if(unlockAnimPlayed) levelPinList[i].pinUI.ShowOrHideUI(true);
				else levelPinList[i].pinUI.ShowOrHideUI(false);

				if (completed) levelPinList[i].pinUI.SetUIComplete();

				levelPinList[i].pinUI.DisableLockIcon();

				if(lessLocks && !dottedAnimPlayed)
				{
					levelDataList[i].dottedAnimPlayed = true;
					levelPinList[i].DrawPath(originPins);
				}

				if(unlocked && !unlockAnimPlayed)
				{
					if(savedLocks == 0)
					{
						levelPinList[i].InitiateRaising(unlocked, unlockAnimPlayed, originPins);
						levelDataList[i].unlockAnimPlayed = true;
					}
					else
					{
						levelPinList[i].DrawPath(originPins);
					}
				}

				if(completed && !pathDrawn) levelDataList[i].pathDrawn = true;
			}

			SaveProgData();
		}

		private void AddOriginPins(LevelPin incomingPin, List<LevelPin> originPins)
		{
			foreach (LevelPin pin in levelPinList)
			{
				var editValues = pin.GetComponent<EditorSetPinValues>();
				if (editValues.levelUnlock_1 == incomingPin.levelID ||
					editValues.levelUnlock_2 == incomingPin.levelID)
					originPins.Add(pin);
			}
		}

		private LevelStatusData FetchUnlockStatusData(LevelIDs ID)
		{
			foreach (LevelStatusData data in levelDataList)
			{
				if (data.levelID == ID)
				{
					return data;
				} 
			}
			Debug.LogWarning("Can't fetch unlock data");
			return null;
		}

		public void SetLevelToComplete(LevelIDs id, bool value)
		{
			foreach (LevelStatusData data in levelDataList)
			{
				if (data.levelID == id)
					data.completed = value;	
			}

			CheckLevelsToUnlock(id);
		}

		private void CheckLevelsToUnlock(LevelIDs incomingID)
		{
			//Need to get this from sheets bc EditorSetPinValues is not available during gameplay
			var sheetID = QbismDataSheets.levelData[incomingID.ToString()];

			LevelIDs levelToUnlock_1 = LevelIDs.empty;
			LevelIDs levelToUnlock_2 = LevelIDs.empty;
			bool unlock1Found = false;
			bool unlock2Found = false;

			foreach (LevelIDs ID in Enum.GetValues(typeof(LevelIDs)))
			{
				if (ID.ToString() == sheetID.lVL_Unlock_1)
				{
					levelToUnlock_1 = ID;
					unlock1Found = true;
				}
				if (ID.ToString() == sheetID.lVL_Unlock_2)
				{
					levelToUnlock_2 = ID;
					unlock2Found = true;
				}
			}

			if(unlock1Found) SetUnlockedStatus(levelToUnlock_1, true);
			else levelToUnlock_1 = LevelIDs.empty;

			if(unlock2Found) SetUnlockedStatus(levelToUnlock_2, true);
			else levelToUnlock_2 = LevelIDs.empty;
		}

		public void SetUnlockedStatus(LevelIDs id, bool value)
		{
			foreach (LevelStatusData data in levelDataList)
			{
				if (data.levelID == id)
				{
					if(data.locks > 0) data.locks--;
					if(data.locks == 0) data.unlocked = value;
				}	
			}			
		}

		public void SetUnlockAnimPlayedStatus(LevelIDs id, bool value)
		{
			foreach (LevelStatusData data in levelDataList)
			{
				if (data.levelID == id)
					data.unlockAnimPlayed = value;
			}
		}

		private List<LevelStatusData> FetchLevelDataList()
		{
			return levelDataList;
		}

		private List<LevelPin> FetchLevelPinList()
		{
			return levelPinList;
		}

		private void SetCurrentData(LevelIDs id, bool serpent, Biomes biome)
		{
			currentLevelID = id;
			currentBiome = biome;
			foreach (LevelStatusData data in levelDataList)
			{
				if(data.levelID != id) continue;
				
				if(data.completed) currentHasSegment = false;
				else currentHasSegment = serpent;
			}
		}

		private void FetchCurrentPin()
		{
			foreach (LevelPin pin in levelPinList)
			{
				if (pin.levelID != currentLevelID) continue;
				pinSelTrack.selectedPin = pin;
				pinSelTrack.currentBiome = pin.GetComponent<EditorSetPinValues>().biome;
			}
		}

		public void SaveProgData()
		{
			SavingSystem.SaveProgData(this, serpProg);
		}

		public void LoadProgHandlerData()
		{
			ProgData data = SavingSystem.LoadProgData();

			levelDataList = data.savedLevelDataList;
			currentLevelID = data.currentLevelID;
		}

		public void WipeProgData() //TO DO: Make this debug only
		{
			for (int i = 0; i < levelDataList.Count; i++)
			{
				var sheetID = QbismDataSheets.levelData[levelDataList[i].levelID.ToString()];
				//If errors when deleting: Check if sheet.count and leveldatalist.count are similar and that sheet has an 'empty'
				levelDataList[i].locks = sheetID.locks;
				levelDataList[i].dottedAnimPlayed = false;
				levelDataList[i].unlocked = false;
				levelDataList[i].unlockAnimPlayed = false;
				levelDataList[i].completed = false;		
				levelDataList[i].pathDrawn = false;		
			}

			currentLevelID = LevelIDs.a_01;

			for (int i = 0; i < serpProg.serpentDataList.Count; i++)
			{
				serpProg.serpentDataList[i] = false;
			}

			SavingSystem.SaveProgData(this, serpProg);
		}

		private void OnDisable()
		{
			if (initiator != null) initiator.onPinInitation -= InitiatePins;
			if (pinSelTrack != null) pinSelTrack.onSavedPinFetch -= FetchCurrentPin;
			foreach (LevelPinUI pinUI in pinUIs)
			{
				if (pinUI != null)
				{
					pinUI.onSetCurrentData -= SetCurrentData;
					pinUI.onFetchLevelData -= FetchLevelDataList;
					pinUI.onFetchLevelPins -= FetchLevelPinList;
				}
			}
		}
	}
}
