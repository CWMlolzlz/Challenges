using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using Challenges;
using System;
using System.Collections.Generic;

namespace Challenges.GUI
{
	public class ChallengePanel : UIPanel{
		public static readonly float WIDTH = 400f;
		public static readonly float HEAD = 40f;
		UILabel m_titleLabel;
		UIDragHandle m_drag;
		FastList<GoalProgressPanel> goalPanels = new FastList<GoalProgressPanel>();
		Challenge m_challenge;
		public override void Start()
		{
			base.Start ();
			this.width = WIDTH;


			this.backgroundSprite = "MenuPanel";
			//this.color = new Color32(255,0,0,100);

			isVisible = true;
			this.canFocus = true;
			this.isInteractive = true;
			CreateTitleLabel ();

			m_drag = this.AddUIComponent<UIDragHandle> ();
			m_drag.width = this.width - 20f;
			m_drag.height = HEAD;
			m_drag.relativePosition = Vector3.zero;
			m_drag.target = this;
		}

		public Challenge CurrentChallenge{
			get{ return m_challenge;}
			set{  
				m_challenge = value;
				IGoal[] goals = m_challenge.Goals;
				this.height = HEAD + GoalProgressPanel.SPACING + goals.Length * (GoalProgressPanel.HEIGHT+GoalProgressPanel.SPACING);
				this.relativePosition = new Vector3 (10,GetUIView().fixedHeight - height - 135);
				for (int pos = 0; pos < goals.Length; pos++) {
					goalPanels.Add (GoalProgressPanel.CreateProgressPanel (this, pos, goals[pos]));
				}
				if (m_challenge.m_finished) {
					Globals.printMessage("Challenges have finished already");
				}else if (m_challenge.m_started) { //has started before hand
					Globals.printMessage("Continuing Challenges");
					m_challenge.Start (m_challenge.m_mapStart);
				} else { //first time running challenge
					Globals.printMessage("Starting Challenges First Time");
					m_challenge.Start(Data.GetGameDateTime());
				}
				m_challenge.OnChallengeFinished += (ChallengeEventType type) => {
					switch(type){
					case(ChallengeEventType.ACED):
						Globals.printMessage("Aced");
						break;
					case(ChallengeEventType.COMPLETED):
						Globals.printMessage("Completed");
						break;
					case(ChallengeEventType.FAILED):
						Globals.printMessage("Failed");
						break;
					case(ChallengeEventType.TOO_LATE):
						Globals.printMessage("Too Late");
						break;
					}
				};

			}
		}

		private void CreateTitleLabel(){
			m_titleLabel = this.AddUIComponent<UILabel> ();
			m_titleLabel.text = "Challenges";
			m_titleLabel.textScale = 1.4f;
			m_titleLabel.textAlignment = UIHorizontalAlignment.Center;
			m_titleLabel.transform.localPosition = Vector3.zero;
			m_titleLabel.transform.parent = this.transform;
			m_titleLabel.position = new Vector3((this.width / 2f) - (m_titleLabel.width / 2f), -20f + (m_titleLabel.height / 2f));
		}

		public void Toggle(){			
			isVisible = !isVisible;
			if (isVisible){
				this.BringToFront();
				this.Focus ();
			}
		}

		public override void Update(){
			base.Update ();

			if ((Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) && Input.GetKeyDown (KeyCode.C)) {
				this.Toggle();
			}
			m_challenge.UpdateGoals();

		}

	}

	public class GoalProgressPanel : UIPanel{
		public static readonly float SPACING = 10f;

		public static readonly float WIDTH = 380f;
		public static readonly float BAR_HEIGHT = 15f;
		public static readonly float BAR_WIDTH = WIDTH - SPACING*2;
		public static readonly float HEIGHT = SPACING * 3 + BAR_HEIGHT*2;

		private UISprite m_bar;
		private UISprite m_barBackground;
		private UILabel m_label;
		private UILabel m_failLabel;
		private UILabel m_passLabel;
		bool m_loaded = false;
		private string text, passText, failText; 

		private IGoal m_goal; 

		public static GoalProgressPanel CreateProgressPanel(UIComponent parent, int pos, IGoal goal){
			
			GoalProgressPanel progressPanel = parent.AddUIComponent<GoalProgressPanel> ();
			progressPanel.Start ();
			progressPanel.PopulatePanel (parent, pos, goal);
			return progressPanel;
		}

		public void PopulatePanel(UIComponent parent, int pos, IGoal goal){

			m_label = this.AddUIComponent<UILabel> ();
			m_passLabel = this.AddUIComponent<UILabel> ();
			m_failLabel = this.AddUIComponent<UILabel> ();
			m_barBackground = this.AddUIComponent<UISprite> ();
			m_bar = this.AddUIComponent<UISprite> ();

			this.m_goal = goal;

			this.backgroundSprite = "GenericPanel";
			this.color = new Color32(164,164,164,255);
			this.size = new Vector2(WIDTH, HEIGHT);

			this.transform.parent = parent.transform;
			this.transform.localPosition = Vector3.zero;
			this.relativePosition = new Vector3 (SPACING, ChallengePanel.HEAD + SPACING + pos * (HEIGHT + SPACING));

			//label
			m_label.text = m_goal.Name;
			m_label.relativePosition = new Vector3 (this.width/2 - m_label.width/2, SPACING);
			//goal
			m_passLabel.text = m_goal.PassValue;
			m_passLabel.textAlignment = UIHorizontalAlignment.Right;
			m_passLabel.relativePosition = new Vector3 (this.width - m_passLabel.width - SPACING,SPACING);

			m_failLabel.text = m_goal.FailValue;
			m_failLabel.textAlignment = UIHorizontalAlignment.Right;
			m_failLabel.relativePosition = new Vector3 (SPACING,SPACING);

			//bar
			m_barBackground.spriteName = "GenericPanel";
			m_barBackground.color = new Color32 (64, 64, 64, 255); 
			m_barBackground.size = new Vector2 (BAR_WIDTH, BAR_HEIGHT);
			m_barBackground.relativePosition = new Vector2 (this.width / 2 - m_barBackground.width / 2, this.height - m_barBackground.height - 10f);

			m_bar.spriteName = "GenericPanel";

			if (m_goal.GoalType == GoalType.MINIMISE) {
				Globals.printMessage ("Color changed");
				m_bar.color = new Color32 (255, 255, 0, 255);
			} else {
				m_bar.color = Color.green;
			}
			m_bar.size = m_barBackground.size;
			m_bar.relativePosition = m_barBackground.relativePosition;

			if (m_goal.HasAlreadyPassed ()) {
				Globals.printMessage ("Already Passed");
				this.m_bar.color = Color.cyan;
				this.m_barBackground.color = Color.cyan;
				this.color = new Color32 (0, 128, 128, 255);
			} else if (m_goal.HasAlreadyFailed ()) {
				Globals.printMessage ("Already Failed");
				this.m_bar.color = Color.red;
				this.m_barBackground.color = Color.red;
				this.color = new Color32 (128, 0, 0, 255);
			} else {

				Globals.printMessage ("Attaching Pass/Fail events");
				this.m_goal.OnPassed += () => {
					this.m_bar.color = Color.cyan;
					this.m_barBackground.color = Color.cyan;
					this.color = new Color32 (0, 128, 128, 255);
				};
				this.m_goal.OnFailed += () => {
					this.m_bar.color = Color.red;
					this.m_barBackground.color = Color.red;
					this.color = new Color32 (128, 0, 0, 255);
				};
				this.m_goal.OnUpdate += () => {
					this.m_bar.transform.localScale = new Vector3 (this.m_goal.GetProportion (), 1, 1);
				};
			}
		}
	}
}