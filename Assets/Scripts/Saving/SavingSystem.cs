﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qbism.Saving
{
	public static class SavingSystem
	{
		public static void SaveProgData(ProgressHandler progHandler, SerpentProgress serpProg)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			string path = Application.persistentDataPath + "/progression.sav";
			FileStream stream = new FileStream(path, FileMode.Create);

			ProgData data = new ProgData(progHandler, serpProg);

			formatter.Serialize(stream, data);
			stream.Close();
		}

		public static ProgData LoadProgData()
		{
			string path = Application.persistentDataPath + "/progression.sav";
			if (File.Exists(path))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				FileStream stream = new FileStream(path, FileMode.Open);

				ProgData data = formatter.Deserialize(stream) as ProgData;
				stream.Close();

				return data;
			}
			else
			{
				Debug.LogError("Progress Handler save file not found in " + path);
				return null;
			}
		}
	}
}

