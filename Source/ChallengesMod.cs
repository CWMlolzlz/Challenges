using ICities;
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
using ChallengesMod;
using ChallengesMod.GUI;

namespace ChallengesMod{

	public class Globals{

		public static double Clamp(double value, double min, double max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}
	}

	public class ChallengeSettings{
		public static bool CHALLENGE_PANEL_BACKGROUND = true;
	}

	public class ChallengesMod : LoadingExtensionBase, IUserMod{
		ChallengeManagerPanel m_managerPanel;

		public string Name {
			get{
				//Test.TestChallengeToString(); 
				return "Challenges"; 		
			}
		} 

		public string Description {
			get { return "Adds some intereseting challenges to complete within Cities Skylines"; }
		}

		//public UIMainPanel m_UIRoot;
		public UIView m_view;

		public readonly string version = "1.0";

		public override void OnLevelLoaded(LoadMode mode)
		{			
			base.OnLevelLoaded(mode);
			if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) return;
			if (m_managerPanel != null) return;
			//test (-1);
			Debug.PrintMessage ("Loaded Game");
			//LoadChallenge ();
			//Challenge challenge = LoadChallenge ();

			m_view = UIView.GetAView();
			m_managerPanel = (ChallengeManagerPanel)m_view.AddUIComponent(typeof(ChallengeManagerPanel));
		}

		public override void OnCreated (ILoading loading){
			base.OnCreated (loading); 
			Debug.PrintMessage ("Created Mod instance");
			DestoryOldGUI ();
			m_managerPanel = (ChallengeManagerPanel)m_view.AddUIComponent(typeof(ChallengeManagerPanel));
		}

		/**
		 * used to remove old GUI elements during dev cycle
		 **/
		private void DestoryOldGUI(){
			m_view = UIView.GetAView();
			if (m_view != null) {
				DestroyOld<ChallengeManagerPanel> ();
				DestroyOld<ChallengePanel> ();
				DestroyOld<UIDialog> ();
			}
		}

		private void DestroyOld<T>(){
			int i = 0;
			while (i < 10) {
				try {
					UIComponent comp = m_view.FindUIComponent (typeof(T).Name);
					if(comp == null){return;}
					GameObject.DestroyImmediate (comp.gameObject);
				} catch (Exception) {
					return;
				}
				i++;
			}
		}

		public override void OnLevelUnloading(){
			Exit();
		}

		public override void OnReleased(){
			Debug.PrintMessage ("Releasing");
			Exit ();
		}

		private void Exit(){
			Debug.PrintMessage ("Exiting");
			if (m_managerPanel != null) {
				GameObject.DestroyImmediate(m_managerPanel.gameObject);
			}

		}

	}

}