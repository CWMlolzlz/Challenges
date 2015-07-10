﻿using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework.IO;
using System;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;


using Challenges.GUI;

namespace Challenges{

	public class Globals{
		public static readonly bool DEBUG = false;

		public static Challenge m_selectedChallenge;
		public static Challenge m_loadedChallenge;

		public static void printMessage(object msg){
			if (DEBUG && msg != null){
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, msg.ToString());
			}
		}
		public static double Clamp(double value, double min, double max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}
	}

	public class ChallengesMod : LoadingExtensionBase, IUserMod, ISerializableDataExtension{
		bool loaded = false;
		public static string XML_LOCATION = "Challenges.xml";
		ChallengeOptions m_options;
		GameObject sm_optionsManager;

		public string Name {
			get { Init (); return "Challenges"; }
		}

		public string Description {
			get { return "Adds some intereseting challenges to complete within Cities Skylines"; }
		}

		private void Init(){
			if (!loaded) {
				Data.loadXML ();
				if (m_options == null) {
					sm_optionsManager = new GameObject("OptionsManager");
					sm_optionsManager.AddComponent<ChallengeOptions>(); 
				}
				//loaded = true;
			}
		}

		public UIMainPanel m_UIRoot;
		public UIView m_view;

		public readonly string version = "1.0";

		/// <summary>
		/// Called when the level (game, map editor, asset editor) is loaded
		/// </summary>
		public override void OnLevelLoaded(LoadMode mode)
		{			
			// Is it an actual game ?
			base.OnLevelLoaded(mode);

			if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) return;
			if (m_UIRoot != null) return;
			//test (-1);
			Globals.printMessage ("Loaded Game");
			//LoadChallenge ();
			//Challenge challenge = LoadChallenge ();
			if (Globals.m_loadedChallenge != null) {
				Globals.printMessage ("Loaded Challenge: " + Globals.m_loadedChallenge.Name);
				m_view = UIView.GetAView();
				m_UIRoot = (UIMainPanel)m_view.AddUIComponent(typeof(UIMainPanel));

				m_UIRoot.CurrentChallenge = Globals.m_loadedChallenge;
			}

		}

		/*
		public override void OnCreated (ILoading loading)
		{
			base.OnCreated (loading); 
			//Globals.printMessage ("Created Game");
			//m_view = UIView.GetAView();
			//UIComponent find = m_view.FindUIComponent(typeof(UIMainPanel).Name);
			//if (find != null) {
			//	GameObject.DestroyImmediate (find.gameObject);
			//}
			//m_UIRoot = (UIMainPanel)m_view.AddUIComponent(typeof(UIMainPanel));
		}
		*/

		public override void OnLevelUnloading(){
			Exit();
		}

		public override void OnReleased(){
			Globals.printMessage ("Releasing");
			Exit ();
		}

		private void test(int state){
			if (Globals.DEBUG) {
				Globals.printMessage (state + " : " + (m_view != null ? "UIView" : "null") + " , " + (m_UIRoot != null ? m_UIRoot.GetType ().ToString () : "null"));
			}
		}

		private void Exit(){
			Globals.printMessage ("Exiting");
			if (m_UIRoot != null) {
				GameObject.DestroyImmediate(m_UIRoot.gameObject);
			}
			if (m_options != null) {
				Globals.printMessage ("Destroying m_options");
				GameObject.DestroyImmediate (sm_optionsManager.gameObject);
			}
		}	

		ISerializableData serializableData;
		string ID = "Challenges";

		public void OnCreated(ISerializableData data){
			Globals.printMessage ("Created ISerializableData");
			this.serializableData = data;
		}

		public void OnLoadData (){
			Globals.printMessage("Onwerwerwerwerwer");
			Globals.printMessage ("OnLoadData");
			try{
				byte[] data = this.serializableData.LoadData(ID);
				Globals.printMessage(data.LongCount());
				MemoryStream memStream = new MemoryStream();
				BinaryFormatter binForm = new BinaryFormatter();
				memStream.Write(data, 0, data.Length);
				memStream.Seek(0, SeekOrigin.Begin);
				Challenge obj = (Challenge)binForm.Deserialize(memStream);

				Globals.printMessage(obj != null ? "Loaded Challenge from save file" : "Challenge Not Found In Save");
				Globals.m_loadedChallenge = obj;
				Globals.printMessage ("Done Good");
			}catch(Exception e){
				Globals.printMessage (e.ToString());
				Globals.printMessage (e.Source);
				Globals.printMessage (e.Message);
				Globals.printMessage (e.StackTrace);

				Globals.printMessage ("Could not load Challenge from save file");

				if (Globals.m_selectedChallenge != null) {
					Globals.m_loadedChallenge = Globals.m_selectedChallenge;
					Globals.printMessage ("Assigning loaded challenge as seledcted challenge");
				} else {
					Globals.printMessage ("No challenge will be run because a challenge has not been selected");
				}

			}
		}

		public void OnSaveData(){
			
			Globals.printMessage ("OnSaveData");
			if (m_UIRoot != null && m_UIRoot.CurrentChallenge != null) {
				Globals.printMessage ("Attempting to save challenge to save file");
				BinaryFormatter formatter = new BinaryFormatter ();
				Globals.printMessage ("Formatter");
				MemoryStream stream = new MemoryStream ();
				Globals.printMessage ("Memory Stream");
				formatter.Serialize (stream, m_UIRoot.CurrentChallenge);
				Globals.printMessage ("Serialize");
				byte[] bytes = stream.ToArray ();
				Globals.printMessage ("ToArray");
				this.serializableData.SaveData (ID, bytes);
				Globals.printMessage ("SaveData");

				Globals.printMessage ("Count: " + bytes.LongCount());
			} else {
				Globals.printMessage ("No challenge was loaded so no challenge will be saved");

			}
		}




	}

}