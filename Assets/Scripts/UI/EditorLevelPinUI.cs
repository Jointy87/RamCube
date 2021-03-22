﻿using System.Collections;
using System.Collections.Generic;
using Qbism.WorldMap;
using UnityEngine;
using UnityEngine.UI;

namespace Qbism.UI
{
	[ExecuteAlways]
	public class EditorLevelPinUI : MonoBehaviour
	{
		//Config parameters
		[SerializeField] Text buttonText;

		private void Start()
		{
			string pinID = NameGameObject();
			buttonText.text = pinID;
		}

		private string NameGameObject()
		{
			string pinID = GetComponent<LevelPinUI>().levelPin.levelID.ToString();
			transform.gameObject.name = "pin UI " + pinID;
			return pinID;
		}
	}
}

	