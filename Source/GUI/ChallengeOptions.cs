using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;

namespace Challenges.GUI
{
	public class ChallengeOptions : MonoBehaviour{

		UIOptionPanel m_optionPanel;
		UIPanel m_modPanel;
		void Awake()
		{
			//DontDestroyOnLoad(this);
		}

		void Start(){
			GameObject contentManager = GameObject.Find("(Library) ContentManagerPanel");

			if (contentManager == null)	return;
			Transform mods = contentManager.transform.GetChild(0).FindChild("Mods");
			if (mods == null) return;
			UILabel modLabel = mods.GetComponentsInChildren<UILabel>().FirstOrDefault(p => p.text.Contains("Challenges"));
			//UIListBox listBox = (UIListBox)modLabel.parent.parent.parent.parent.parent.parent.Find<UIListBox>("Categories");

			m_modPanel = (UIPanel)modLabel.parent;

			UICheckBox checkBox = m_modPanel.GetComponentsInChildren<UICheckBox> ()[0];

			m_optionPanel = (UIOptionPanel)modLabel.GetUIView ().FindUIComponent<UIOptionPanel> (UIOptionPanel.cacheName);

			UIButton shareButton = m_modPanel.GetComponentsInChildren<UIButton> ().FirstOrDefault (p => p.text.Contains ("SHARE"));
			UIButton optionButton = m_modPanel.AddUIComponent<UIButton> ();

			optionButton.text = "OPTIONS";
			optionButton.size = new Vector2 (123, 32);
			optionButton.textScale = 0.875f;
			optionButton.textVerticalAlignment = UIVerticalAlignment.Middle;
			optionButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
			optionButton.normalBgSprite = "ButtonMenu";
			optionButton.hoveredBgSprite = "ButtonMenuHovered";
			optionButton.pressedBgSprite = "ButtonMenuPressed";
			optionButton.disabledBgSprite = "ButtonMenuDisabled";
			optionButton.eventClick += (component, eventParam) => {
				OptionClicked ();
			};
			optionButton.relativePosition = new Vector3 (123, 0) + new Vector3 (25 + 10, 93);

			//if (shareButton != null) {				
				//optionButton.relativePosition = shareButton.relativePosition + new Vector3 (shareButton.width + 10f, 0);
			//	optionButton.relativePosition = new Vector3 (123, 0) + new Vector3 (25 + 10, 93);
			//} else {
			//	optionButton.relativePosition = new Vector3 (25, 93);
			//}
			optionButton.enabled = checkBox.isChecked;
			checkBox.eventCheckChanged += (component, value) => {
				if (checkBox.isChecked) {
					optionButton.Enable ();
				} else {
					optionButton.Disable ();
				}
			};


		}

		void OptionClicked(){
			if (m_optionPanel == null) {				
				m_optionPanel = (UIOptionPanel)m_modPanel.GetUIView ().AddUIComponent (typeof(UIOptionPanel));
			}
			if (m_optionPanel.isVisible) {
				m_optionPanel.Hide ();
			} else {
				m_optionPanel.Show ();
			}
		}
	}

	public class UIOptionPanel : UIPanel{

		public static readonly string cacheName = "UIOptionPanel";

		private static readonly float WIDTH = 600f;
		private static readonly float HEIGHT = 400f;
		private static readonly float HEAD = 40f;
		private static readonly float SELECT_BUTTON_HEIGHT = 50f;

		private static readonly float LIST_PANEL_WIDTH = 200f;
		private static readonly float LIST_PANEL_HEIGHT = HEIGHT - HEAD;

		private static readonly float SPACING = 10f;


		UIPanel m_challengeDetailsPanel;
		UILabel m_challengeName;
		UILabel m_challengeDesc; 
		UILabel m_challengeBreakdown;
		UILabel m_challengeDeadline;

		UIListBox m_challengeBrowser;
		UIPanel m_challengeListPanel;
		UIButton m_selectButton;

		UILabel m_title;
		UIButton m_closeButton;
		UIDragHandle m_dragHandle;

		List<Challenge> m_challenges;
		int m_selectedIndex = -1;
		public override void Start ()
		{
			base.Start ();
			this.size = new Vector2 (WIDTH,HEIGHT);
			this.backgroundSprite = "MenuPanel";
			this.canFocus = true;
			this.isInteractive = true; 
			this.BringToFront ();
			this.relativePosition = new Vector3 (50, 50);
			this.Show (); 
			this.cachedName = cacheName;

			m_closeButton = this.AddUIComponent<UIButton> ();
			m_closeButton.normalBgSprite = "buttonclose";
			m_closeButton.hoveredBgSprite = "buttonclosehover";
			m_closeButton.pressedBgSprite = "buttonclosepressed"; 
			m_closeButton.relativePosition = new Vector3 (WIDTH-35,5);
			m_closeButton.eventClick += (component, eventParam) => {this.Hide();};

			m_title = this.AddUIComponent<UILabel> ();
			m_title.text = "CHALLENGES OPTIONS";
			m_title.relativePosition = new Vector3(WIDTH/2 - m_title.width/2, HEAD/2 - m_title.height/2);
			m_title.textAlignment = UIHorizontalAlignment.Center;

			m_dragHandle = this.AddUIComponent<UIDragHandle> ();
			m_dragHandle.size = new Vector2 (WIDTH-30f, HEAD);
			m_dragHandle.relativePosition = new Vector3(0,0,10);
			m_dragHandle.target = this;

			m_challengeListPanel = this.AddUIComponent<UIPanel> ();
			m_challengeListPanel.size = new Vector2 (LIST_PANEL_WIDTH,LIST_PANEL_HEIGHT);
			m_challengeListPanel.relativePosition = new Vector3 (0, HEAD);

			m_challengeBrowser = m_challengeListPanel.AddUIComponent<UIListBox> ();
			m_challengeBrowser.size = new Vector2 (LIST_PANEL_WIDTH - SPACING*2, LIST_PANEL_HEIGHT - SELECT_BUTTON_HEIGHT - SPACING*2);
			m_challengeBrowser.relativePosition = new Vector3(SPACING, SPACING);

			m_challengeBrowser.normalBgSprite = "GenericPanelDark";
			//m_challengeBrowser.bottomColor = Color.green;

			m_challengeBrowser.itemHighlight = "ListItemHighlight";
			m_challengeBrowser.itemHover = "ListItemHover";
			m_challengeBrowser.itemPadding.left = 10;
			m_challengeBrowser.itemPadding.top = 4;
			m_challengeBrowser.listPadding.top = 10;
			m_challengeBrowser.listPadding.bottom = 10;
			m_challengeBrowser.colorizeSprites = true;

			m_challenges = Data.m_challenges;
			m_challenges.Sort ((x, y) => string.Compare(x.Name, y.Name));
			string[] challengeNames = new string[m_challenges.Count];
			for (int i = 0; i < challengeNames.Length; i++){
				challengeNames [i] = m_challenges [i].Name;
			}

			m_challengeBrowser.items = challengeNames;
			m_challengeBrowser.eventSelectedIndexChanged += ChallengeChanged;

			m_selectButton = m_challengeListPanel.AddUIComponent<UIButton> ();
			m_selectButton.text = "SELECT";
			m_selectButton.size = new Vector2 (LIST_PANEL_WIDTH - SPACING*2, SELECT_BUTTON_HEIGHT-SPACING);
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
			m_selectButton.eventClick += (component, eventParam) => {Globals.m_selectedChallenge = m_challenges[this.m_selectedIndex];this.Hide();};

			m_challengeDetailsPanel = this.AddUIComponent<UIPanel> ();
			m_challengeDetailsPanel.size = new Vector2 (WIDTH - LIST_PANEL_WIDTH, HEIGHT - HEAD);
			//m_challengeDetailsPanel.backgroundSprite = "GenericPanel";
			m_challengeDetailsPanel.relativePosition = new Vector3 (LIST_PANEL_WIDTH, HEAD);

			m_challengeName = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeName.text = "Name\n";
			m_challengeName.disabledTextColor = Color.gray;
			m_challengeName.Disable();

			m_challengeDesc = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeDesc.text = "Description\n";
			//m_challengeDesc.backgroundSprite = "GenericPanel";
			m_challengeDesc.width = m_challengeDetailsPanel.width;
			m_challengeDesc.minimumSize = new Vector2(m_challengeDetailsPanel.width-SPACING*2,20);
			m_challengeDesc.wordWrap = true;
			m_challengeDesc.disabledTextColor = Color.gray;
			m_challengeDesc.Disable();

			m_challengeBreakdown = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeBreakdown.text = "Challenge Breakdown\n";
			//m_challengeBreakdown.backgroundSprite = "GenericPanel";
			m_challengeBreakdown.minimumSize = new Vector2(m_challengeDetailsPanel.width-SPACING*2,20);
			m_challengeBreakdown.wordWrap = true;
			m_challengeBreakdown.disabledTextColor = Color.gray;
			m_challengeBreakdown.Disable();

			m_challengeDeadline = m_challengeDetailsPanel.AddUIComponent<UILabel> ();
			m_challengeDeadline.text = "Duration\n";
			m_challengeDeadline.disabledTextColor = Color.gray;
			m_challengeDeadline.Disable();

			FormatDetails ();
		}	

		public void ChallengeChanged(UIComponent comp, int value){
			m_selectButton.Enable();
			this.m_selectedIndex = value;
			Challenge selectedChallenge = m_challenges [value];
			m_challengeName.text = "Name\n    " + selectedChallenge.Name;
			m_challengeDesc.text = "Description\n    " + selectedChallenge.Description;
			m_challengeBreakdown.text = "Challenge Breakdown" + GoalsToString (selectedChallenge.Goals);

			if (selectedChallenge.m_hasDeadline){
				m_challengeDeadline.text = "Duration\n    " + selectedChallenge.Years + " years, " + selectedChallenge.Months + " months";
			} else {
				m_challengeDeadline.text = "Duration\n    Unlimited";
			}
			m_challengeName.Enable ();
			m_challengeDesc.Enable ();
			m_challengeBreakdown.Enable ();
			m_challengeDeadline.Enable ();
			FormatDetails ();
		}

		public void FormatDetails(){
			m_challengeName.relativePosition = new Vector3 (SPACING, SPACING);
			m_challengeDesc.relativePosition = m_challengeName.relativePosition + new Vector3 (0, m_challengeName.height + SPACING, 0);
			m_challengeBreakdown.relativePosition = m_challengeDesc.relativePosition + new Vector3 (0, m_challengeDesc.height + SPACING, 0);
			m_challengeDeadline.relativePosition = m_challengeBreakdown.relativePosition + new Vector3 (0, m_challengeBreakdown.height + SPACING, 0);
		}

		public string GoalsToString(IGoal[] goals){
			string output = "";
			foreach (IGoal goal in goals) {
				output += "\n    - ";
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

	}
}

