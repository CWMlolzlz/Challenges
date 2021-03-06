﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using ChallengesMod;
using ICities;

namespace ChallengesMod.GUI
{
	public class ChallengeManagerPanel : UIPanel, ISerializableDataExtension{

		public static readonly string cacheName = "ChallengeManagerPanel";

		private static readonly float WIDTH = 600f;
		private static readonly float HEIGHT = 600f;
		private static readonly float HEAD = 40f;
		private static readonly float SELECT_BUTTON_HEIGHT = 50f;

		private static readonly float LIST_PANEL_WIDTH = 200f;
		private static readonly float LIST_PANEL_HEIGHT = HEIGHT - HEAD;

		private static readonly float SPACING = 10f;

		static ChallengePanel m_challengePanel; //needs to be static so OnSaveData() can 'see' it
		static Challenge m_temp_challenge = null;
		static FastList<IReward> m_activeRewards = new FastList<IReward> ();

		UIPanel m_challengeDetailsPanel;
		UILabel m_challengeName;
		UILabel m_challengeDesc; 
		UILabel m_challengeBreakdown;
		UILabel m_challengeReward;
		UILabel m_challengePenalty;
		UILabel m_challengeDeadline;

		UIListBox m_challengeBrowser; 
		UIPanel m_challengeListPanel;
		UIButton m_selectButton;
		UIButton m_refresh;

		UILabel m_title;
		UIButton m_closeButton;
		UIDragHandle m_dragHandle;

		List<Challenge> m_challenges;
		int m_selectedIndex = -1;

		private static readonly string CHALLENGE_KEY = "challenge";
		private static readonly string ACTIVE_REWARDS_KEY = "activeRewards";
		static ISerializableData m_dataSerializer;

		public void OnCreated(ISerializableData data){
			Debug.PrintMessage ("OnCreated");
			m_dataSerializer = data;
		}

		public void OnLoadData(){
			Debug.PrintMessage ("OnLoadChallenge");
			try{
				byte[] bytes = m_dataSerializer.LoadData(CHALLENGE_KEY);
				string xmlString = System.Text.Encoding.UTF8.GetString(bytes);
				Challenge challenge = Data.XMLStringToChallenge(xmlString);
				Debug.PrintMessage(xmlString);
				Debug.PrintMessage ("Bytes Read = " + bytes.Length);
				m_temp_challenge = challenge;
			}catch(Exception e){
				Debug.PrintMessage(e.ToString());
			}

			Debug.PrintMessage ("OnLoadActiveBoosts");
			try{
				byte[] bytes = m_dataSerializer.LoadData(ACTIVE_REWARDS_KEY);
				string xmlString = System.Text.Encoding.UTF8.GetString(bytes);
				IReward[] rewards = Data.XMLStringToActiveRewards(xmlString);
				Debug.PrintMessage("Number of active rewards found = " + rewards.Length);
				AddToActiveRewards(rewards);
				Debug.PrintMessage(xmlString);
				Debug.PrintMessage ("Bytes Read = " + bytes.Length);
			}catch(Exception e){
				Debug.PrintMessage(e.ToString());
			}
		}

		public void OnSaveData(){
			Debug.PrintMessage ("OnSaveData");
			if (m_challengePanel != null && m_challengePanel.CurrentChallenge != null) {
				try {
					byte[] bytes = System.Text.Encoding.UTF8.GetBytes (Data.ChallengeToXMLString (m_challengePanel.CurrentChallenge));
					Debug.PrintMessage ("Bytes Ready = " + bytes.Length);
					m_dataSerializer.SaveData (CHALLENGE_KEY, bytes);
				} catch (Exception e) {
					Debug.PrintMessage (e.ToString ());
					Debug.PrintMessage ("Source " + e.Source);
				}
			} else {
				Debug.PrintMessage ("No challenge can be saved");
				m_dataSerializer.SaveData (CHALLENGE_KEY, new byte[]{1});
			}
			Debug.PrintMessage (m_activeRewards.m_size);
			if (m_activeRewards != null && m_activeRewards.m_size > 0) {
				Debug.PrintMessage ("Saving active rewards");
				Debug.PrintMessage (m_activeRewards.m_size);
				try {
					string xml = Data.ActiveRewardsToXMLString (m_activeRewards.ToArray ());
					Debug.PrintMessage (xml);
					byte[] activeRewardBytes = System.Text.Encoding.UTF8.GetBytes (xml);
					m_dataSerializer.SaveData (ACTIVE_REWARDS_KEY, activeRewardBytes);
				} catch (Exception e) {
					Debug.PrintMessage (e.ToString ());
				}
			} else {
				Debug.PrintMessage ("No rewards can be saved");
				m_dataSerializer.SaveData (ACTIVE_REWARDS_KEY, new byte[]{1});
			}
			Debug.PrintMessage ("out of saving");
		}

		public void OnReleased(){Debug.PrintMessage ("Released");}

		public ChallengePanel CurrentChallengePanel{
			get{ return m_challengePanel; }
		}

		public override void Update ()
		{
			base.Update ();
			if ((Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) && Input.GetKeyDown (KeyCode.C)) {
				this.CycleVisibility ();
			}
			foreach (IReward r in m_activeRewards) {				
				if (r.Use ()) {
					m_activeRewards.Remove (r);
				}
			}
		}

		public override void Start ()
		{
			base.Start ();
			Debug.PrintMessage ("Start");
			this.size = new Vector2 (WIDTH, HEIGHT);
			this.backgroundSprite = "MenuPanel";
			this.canFocus = true;
			this.isInteractive = true; 
			this.BringToFront ();
			this.relativePosition = new Vector3 (50, 50);
			this.Show (); 
			this.cachedName = cacheName;

			m_challengePanel = (ChallengePanel) this.GetUIView ().AddUIComponent (typeof(ChallengePanel));
			Debug.PrintMessage("Start() - Does ChallengePanel Exists? " + (m_challengePanel != null).ToString());
			m_challengePanel.manager = this;
			m_challengePanel.OnChallengeStarted += () => {m_selectButton.Disable ();};
			m_challengePanel.OnChallengeEnded += () => {m_selectButton.Enable ();};

			m_challengePanel.Hide ();

			m_title = this.AddUIComponent<UILabel> ();
			m_title.text = "CHALLENGES OPTIONS";
			m_title.relativePosition = new Vector3 (WIDTH / 2 - m_title.width / 2, HEAD / 2 - m_title.height / 2);
			m_title.textAlignment = UIHorizontalAlignment.Center;

			m_dragHandle = this.AddUIComponent<UIDragHandle> ();
			m_dragHandle.size = new Vector2(WIDTH,HEIGHT);
			m_dragHandle.relativePosition = Vector3.zero;
			m_dragHandle.target = this;

			m_closeButton = this.AddUIComponent<UIButton> ();
			m_closeButton.normalBgSprite = "buttonclose";
			m_closeButton.hoveredBgSprite = "buttonclosehover";
			m_closeButton.pressedBgSprite = "buttonclosepressed"; 
			m_closeButton.relativePosition = new Vector3 (WIDTH - 35, 5,10);
			m_closeButton.eventClick += (component, eventParam) => {
				this.Hide();
			};

			m_challengeListPanel = this.AddUIComponent<UIPanel> ();
			m_challengeListPanel.size = new Vector2 (LIST_PANEL_WIDTH, LIST_PANEL_HEIGHT);
			m_challengeListPanel.relativePosition = new Vector3 (0, HEAD);

			m_challengeBrowser = m_challengeListPanel.AddUIComponent<UIListBox> ();
			m_challengeBrowser.size = new Vector2 (LIST_PANEL_WIDTH - SPACING * 2, LIST_PANEL_HEIGHT - SELECT_BUTTON_HEIGHT - SPACING * 2);
			m_challengeBrowser.relativePosition = new Vector3 (SPACING, SPACING);

			m_challengeBrowser.normalBgSprite = "GenericPanelDark";
			//m_challengeBrowser.bottomColor = Color.green;

			m_challengeBrowser.itemHighlight = "ListItemHighlight";
			m_challengeBrowser.itemHover = "ListItemHover";
			m_challengeBrowser.itemPadding.left = 10;
			m_challengeBrowser.itemPadding.top = 4;
			m_challengeBrowser.listPadding.top = 10;
			m_challengeBrowser.listPadding.bottom = 10;
			m_challengeBrowser.colorizeSprites = true;

			LoadChallenges ();

			m_selectButton = m_challengeListPanel.AddUIComponent<UIButton> ();
			m_selectButton.text = "START";
			m_selectButton.size = new Vector2 (LIST_PANEL_WIDTH - SPACING * 2, SELECT_BUTTON_HEIGHT - SPACING);
			m_selectButton.textScale = 1.25f;
			m_selectButton.textVerticalAlignment = UIVerticalAlignment.Middle;
			m_selectButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
			m_selectButton.normalBgSprite = "ButtonMenu";
			m_selectButton.hoveredBgSprite = "ButtonMenuHovered";
			m_selectButton.pressedBgSprite = "ButtonMenuPressed";
			m_selectButton.disabledBgSprite = "ButtonMenuDisabled";
			m_selectButton.color = Color.green;
			m_selectButton.focusedColor = m_selectButton.color;
			m_selectButton.hoveredColor = m_selectButton.color;
			m_selectButton.pressedColor = m_selectButton.color;
			m_selectButton.relativePosition = new Vector3 (SPACING, LIST_PANEL_HEIGHT - SELECT_BUTTON_HEIGHT);
			m_selectButton.Disable ();
			m_selectButton.eventClick += ChallengeSelected;

			m_refresh = this.AddUIComponent<UIButton> ();
			m_refresh.size = new Vector2 (60,24);
			m_refresh.text = "Refresh";
			m_refresh.textScale = 0.875f;
			m_refresh.normalBgSprite = "ButtonMenu";
			m_refresh.hoveredBgSprite = "ButtonMenuHovered";
			m_refresh.pressedBgSprite = "ButtonMenuPressed";
			m_refresh.disabledBgSprite = "ButtonMenuDisabled";
			//m_refresh.color = new Color32(255,0,0,255);
			//m_refresh.focusedColor = m_refresh.color;
			//m_refresh.hoveredColor = m_refresh.color;
			//m_refresh.pressedColor = m_refresh.color;
			m_refresh.relativePosition = m_closeButton.relativePosition + new Vector3 (-60 -SPACING,6f);
			m_refresh.eventClick += (component, eventParam) => {LoadChallenges();};

			m_challengeDetailsPanel = this.AddUIComponent<UIPanel> ();
			m_challengeDetailsPanel.size = new Vector2 (WIDTH - LIST_PANEL_WIDTH, HEIGHT - HEAD);
			//m_challengeDetailsPanel.backgroundSprite = "GenericPanel";
			m_challengeDetailsPanel.relativePosition = new Vector3 (LIST_PANEL_WIDTH, HEAD);

			m_challengeName = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeName.text = "Name\n";
			m_challengeName.disabledTextColor = Color.gray;
			m_challengeName.Disable ();

			m_challengeDesc = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeDesc.text = "Description\n";
			//m_challengeDesc.backgroundSprite = "GenericPanel";
			m_challengeDesc.width = m_challengeDetailsPanel.width;
			m_challengeDesc.minimumSize = new Vector2 (m_challengeDetailsPanel.width - SPACING * 2, 20);
			m_challengeDesc.wordWrap = true;
			m_challengeDesc.disabledTextColor = Color.gray;
			m_challengeDesc.Disable ();

			m_challengeBreakdown = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeBreakdown.text = "Breakdown\n";
			//m_challengeBreakdown.backgroundSprite = "GenericPanel";
			m_challengeBreakdown.minimumSize = new Vector2 (m_challengeDetailsPanel.width - SPACING * 2, 20);
			m_challengeBreakdown.wordWrap = true;
			m_challengeBreakdown.disabledTextColor = Color.gray;
			m_challengeBreakdown.Disable ();

			m_challengeReward = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeReward.text = "Reward\n";
			//m_challengeBreakdown.backgroundSprite = "GenericPanel";
			m_challengeReward.minimumSize = new Vector2 (m_challengeDetailsPanel.width - SPACING * 2, 20);
			m_challengeReward.wordWrap = true;
			m_challengeReward.autoSize = true;
			m_challengeReward.disabledTextColor = Color.gray;
			m_challengeReward.Disable ();

			m_challengePenalty = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengePenalty.text = "Penalty\n";
			//m_challengeBreakdown.backgroundSprite = "GenericPanel";
			m_challengePenalty.minimumSize = new Vector2 (m_challengeDetailsPanel.width - SPACING * 2, 20);
			m_challengePenalty.wordWrap = true;
			m_challengePenalty.autoSize = true;
			m_challengePenalty.disabledTextColor = Color.gray;
			m_challengePenalty.Disable ();

			m_challengeDeadline = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeDeadline.text = "Duration\n";
			m_challengeDeadline.disabledTextColor = Color.gray;
			m_challengeDeadline.Disable ();

			FormatDetails ();

			if (m_temp_challenge != null) {
				m_challengePanel.SetCurrentChallenge (m_temp_challenge,true);
				//m_temp_challenge = null;
			}
		}

		public void AddToActiveRewards(IReward[] rewards){
			foreach (IReward r in rewards) {
				m_activeRewards.Add (r);
			}
		}
		 
		public void LoadChallenges(){
			
			m_challenges = Data.SearchForMods();
			m_challenges.Sort ((x, y) => string.Compare(x.Name, y.Name));
			string[] challengeNames = new string[m_challenges.Count];
			for (int i = 0; i < challengeNames.Length; i++){
				challengeNames [i] = m_challenges [i].Name;
			}
			m_challengeBrowser.items = challengeNames;
			m_challengeBrowser.eventSelectedIndexChanged += ChallengeChanged;

		}

		public void ChallengeChanged(UIComponent comp, int value){
			m_selectButton.Enable();
			this.m_selectedIndex = value;
			Challenge selectedChallenge = m_challenges [value];
			m_challengeName.text = "Name\n    " + selectedChallenge.Name;
			m_challengeDesc.text = "Description\n    " + selectedChallenge.Description;
			m_challengeBreakdown.text = "Breakdown" + GoalsToString (selectedChallenge.Goals); 
			if (selectedChallenge.Rewards != null && selectedChallenge.Rewards.Length >= 0) {
				m_challengeReward.text = "Reward" + RewardsToString (selectedChallenge.Rewards);
			} else {
				m_challengeReward.text = "";
			}

			if (selectedChallenge.Penalties != null && selectedChallenge.Penalties.Length >= 0) {
				m_challengePenalty.text = "Penalty" + RewardsToString (selectedChallenge.Penalties);
			} else {
				m_challengePenalty.text = "";
			}

			if (selectedChallenge.m_hasDeadline){
				m_challengeDeadline.text = "Duration\n    " + selectedChallenge.Years + " years, " + selectedChallenge.Months + " months";
			} else {
				m_challengeDeadline.text = "Duration\n    Unlimited";
			}

			m_challengeName.Enable ();
			m_challengeDesc.Enable ();
			m_challengeBreakdown.Enable ();
			m_challengeReward.Enable ();
			m_challengePenalty.Enable ();
			m_challengeDeadline.Enable ();
			FormatDetails ();
		}

		public void FormatDetails(){
			FastList<UIComponent> comps = new FastList<UIComponent> ();
			comps.Add (m_challengeName);
			comps.Add (m_challengeDesc);
			comps.Add (m_challengeBreakdown);
			if (!m_challengeReward.text.Equals ("")) {	
				comps.Add(m_challengeReward);
			}
			if (!m_challengePenalty.text.Equals ("")) {
				comps.Add (m_challengePenalty);
			}
			comps.Add (m_challengeDeadline);
			AutoSpace (comps.ToArray ());
		}

		private void AutoSpace(params UIComponent[] components){
			UIComponent prevComp = null;
			foreach (UIComponent comp in components) {
				if (prevComp == null) {
					comp.relativePosition = new Vector3 (SPACING, SPACING);
				} else {
					comp.relativePosition = prevComp.relativePosition + new Vector3 (0, prevComp.height + SPACING);
				}
				prevComp = comp;
			}
		}

		public string GoalsToString(IGoal[] goals){
			string output = "";
			foreach (IGoal goal in goals) {
				output += "\n    ";
				if (goal.GoalType == GoalType.MAXIMISE) {
					if (goal.PassOnce && goal.FailOnce) {
						output += "Get your " + goal.Name + " to " + goal.PassValue + " once without ever dropping below " + goal.FailValue;
					} else if (goal.PassOnce) {
						output += "Get your " + goal.Name + " to " + goal.PassValue + " just once";
					} else if (goal.FailOnce) {
						output += "Get your " + goal.Name + " to " + goal.PassValue + " while staying above " + goal.FailValue + " at all times";
					} else {
						output += "Get your " + goal.Name + " to " + goal.PassValue;
					}
				} else if (goal.GoalType == GoalType.MINIMISE) {
					if (goal.PassOnce && goal.FailOnce) {
						output += "Get your " + goal.Name + " to " + goal.PassValue + " once without ever going above " + goal.FailValue;
					} else if (goal.PassOnce) {
						output += "Get your " + goal.Name + " to " + goal.PassValue + " just once";
					} else if (goal.FailOnce) {
						output += "Get your " + goal.Name + " to " + goal.PassValue + " while staying below " + goal.FailValue + " at all times";
					} else {
						output += "Get your " + goal.Name + " to " + goal.PassValue;
					}
				}
			}
			return output;
		}

		public string RewardsToString(IReward[] rewards){
			string output = "";
			foreach (IReward reward in rewards) {
				output += "\n    ";
				if (reward.GetType ().Equals (typeof(Boost))) {
					Boost boost = (Boost)reward;
					output += "Boosts " + boost.Name + " by a factor of " + boost.Value;
					if (!boost.isForever) {
						output += " for ";
						if (boost.Years >= 0 && boost.Months >= 0) {
							output += boost.Years + " year(s) and " + boost.Months + " month(s)";
						} else if (boost.Years >= 0) {
							output += boost.Years + " year(s)";
						} else if (boost.Months >= 0) {
							output += boost.Months + " month(s)";
						}
					}
				} else if (reward.GetType ().Equals (typeof(Payment))) {
					output += reward.ToString ();
				}
			}
			return output;
		}

		private void ChallengeSelected(UIComponent component, UIMouseEventParameter eventArgs){
			
			Debug.PrintMessage ("Challenge Selected: " + m_selectedIndex);
			Debug.PrintMessage("ChallengeSelected() - Does ChallengePanel Exists? " + (m_challengePanel != null).ToString());
			if (m_selectedIndex != -1) {
				m_challengePanel.SetCurrentChallenge(m_challenges [this.m_selectedIndex],false);
			}
			//this.Hide ();
		}

		private void CycleVisibility(){ 
			if (!m_challengePanel.isVisible && m_challengePanel.CurrentChallenge != null) {
				m_challengePanel.Show ();
			} else if (!this.isVisible) {
				this.Show ();
			} else {
				this.Hide ();
				m_challengePanel.Hide ();
			}
		}

	}
}