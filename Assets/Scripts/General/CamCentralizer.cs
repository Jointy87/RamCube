﻿using System.Collections;
using System.Collections.Generic;
using Qbism.Cubes;
using UnityEngine;

namespace Qbism.General
{
	[ExecuteInEditMode]
	public class CamCentralizer : MonoBehaviour
	{
		//Config parameters
		[SerializeField] Camera cam;

		//States
		float highest, lowest, leftest, rightest;
		bool firstValueAssigned = false;

		void Start()
		{
			PositionCenterPoint();
		}

		private void PositionCenterPoint()
		{
			CubeHandler handler = FindObjectOfType<CubeHandler>();

			foreach (KeyValuePair<Vector2Int, FloorCube> cube in handler.floorCubeDic)
			{
				if (!firstValueAssigned)
				{
					highest = cube.Value.transform.position.z;
					lowest = cube.Value.transform.position.z;
					leftest = cube.Value.transform.position.x;
					rightest = cube.Value.transform.position.x;

					firstValueAssigned = true;
				}

				if (cube.Value.transform.position.z > highest) highest = cube.Value.transform.position.z;
				if (cube.Value.transform.position.z < lowest) lowest = cube.Value.transform.position.z;
				if (cube.Value.transform.position.x < leftest) leftest = cube.Value.transform.position.x;
				if (cube.Value.transform.position.x > rightest) rightest = cube.Value.transform.position.x;
			}

			float zPos = lowest + (highest - lowest) / 2;
			float xPos = leftest + (rightest - leftest) / 2;

			transform.position = new Vector3(xPos, 0, zPos);
		}
	}
}

