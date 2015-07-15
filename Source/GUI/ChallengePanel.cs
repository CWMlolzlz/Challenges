using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using ChallengesMod.GUI;
using System;
using System.Collections.Generic;

namespace ChallengesMod
{
	public class ChallengePanel : UIPanel{
		public static readonly float WIDTH = 400f;
		public static readonly float HEAD = 40f;
		public static readonly float SPACING = 10f;
		private static readonly float BUTTON_WIDTH = 60f,BUTTON_HEIGHT = 24f,CLOSE_BUTTON_SIZE = 32f;

		public ChallengeManagerPanel manager;
		public delegate void PanelEvent();

		public event PanelEvent OnChallengeEnded;
		public event PanelEvent OnChallengeStarted;

		UILabel m_titleLabel;
		UILabel m_startTimeLabel;
		UILabel m_endTimeLabel;

		UIButton m_startButton;
		UIButton m_endButton;
		UIButton m_closeButton;

		UIDragHandle m_drag;
		FastList<GoalProgressPanel> goalPanels = new FastList<GoalProgressPanel>();
		Challenge m_challenge;
		public override void Start()
		{
			base.Start ();
			Debug.PrintMessage ("Creating ChallengePanel");
			this.width = WIDTH;
			this.height = HEAD;
			if (m_challenge != null && m_challenge.Goals != null) { //covers case when the panel reassignes hieght to be head even with a challenge being loaded
				this.height = HEAD + GoalProgressPanel.SPACING + m_challenge.Goals.Length * (GoalProgressPanel.HEIGHT+GoalProgressPanel.SPACING);
			}
			this.backgroundSprite = "MenuPanel";
			this.Hide ();
			this.relativePosition = new Vector3 (10,GetUIView().fixedHeight - height - 135);

			m_titleLabel = this.AddUIComponent<UILabel> ();
			m_titleLabel.text = "Challenges";
			m_titleLabel.textScale = 1.25f;
			m_titleLabel.size = new Vector2 (WIDTH,HEAD);
			m_titleLabel.relativePosition = new Vector3(SPACING,SPACING);

			m_drag = this.AddUIComponent<UIDragHandle> ();
			m_drag.width = this.width;
			m_drag.height = HEAD;
			m_drag.relativePosition = new Vector3(0,0,10);
			m_drag.target = this;

			m_closeButton = this.AddUIComponent<UIButton> ();
			m_closeButton.size = new Vector2 (CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE);
			m_closeButton.normalBgSprite = "buttonclose";
			m_closeButton.hoveredBgSprite = "buttonclosehover";
			m_closeButton.pressedBgSprite = "buttonclosepressed"; 
			m_closeButton.relativePosition = new Vector3 (WIDTH - 3f - m_closeButton.width, 5);
			Debug.PrintMessage(m_closeButton.size);
			m_closeButton.eventClick += (component, eventParam) => {
				this.Hide ();
			};

			m_startButton = this.AddUIComponent<UIButton> ();
			m_startButton.size = new Vector2 (BUTTON_WIDTH,BUTTON_HEIGHT);
			m_startButton.text = "Start";
			m_startButton.textScale = 0.875f;
			m_startButton.normalBgSprite = "ButtonMenu";
			m_startButton.hoveredBgSprite = "ButtonMenuHovered";
			m_startButton.pressedBgSprite = "ButtonMenuPressed";
			m_startButton.disabledBgSprite = "ButtonMenuDisabled";
			m_startButton.color = Color.green;
			m_startButton.focusedColor = m_startButton.color;
			m_startButton.hoveredColor = m_startButton.color;
			m_startButton.pressedColor = m_startButton.color;
			m_startButton.relativePosition = m_closeButton.relativePosition + new Vector3 (-BUTTON_WIDTH -SPACING,6f);

			//m_startButton.eventClick += StartChallenge;
			m_startButton.Disable ();
			m_startButton.Hide ();

			m_endButton = this.AddUIComponent<UIButton> ();
			m_endButton.size = m_startButton.size;
			m_endButton.text = "Forfeit";
			m_endButton.textScale = 0.875f;
			m_endButton.normalBgSprite = "ButtonMenu";
			m_endButton.hoveredBgSprite = "ButtonMenuHovered";
			m_endButton.pressedBgSprite = "ButtonMenuPressed";
			m_endButton.disabledBgSprite = "ButtonMenuDisabled";
			m_endButton.color = new Color32(255,0,0,255);
			m_endButton.focusedColor = m_endButton.color;
			m_endButton.hoveredColor = m_endButton.color;
			m_endButton.pressedColor = m_endButton.color;
			m_endButton.relativePosition = m_startButton.relativePosition;
			m_endButton.eventClick += AskForfeit;
			//m_startButton.disabledColor = Color.gray;
			//m_endButton.Disable ();
			//m_endButton.Hide ();
		}

		public Challenge CurrentChallenge{
			get{ 
				return m_challenge;
			}
		}

		public void SetCurrentChallenge(Challenge challenge, bool resuming){
			Debug.PrintMessage("SetCurrentChallenge()");
			if(m_challenge != null){ //ensures the previous challenge has been removed
				DestroyChallenge ();
			}
			Debug.PrintMessage("Post DestroyChallenge()");
			m_challenge = challenge;
			//populate goal panels
			IGoal[] goals = m_challenge.Goals;
			this.height = HEAD + GoalProgressPanel.SPACING + goals.Length * (GoalProgressPanel.HEIGHT+GoalProgressPanel.SPACING);
			Debug.PrintMessage ("Number of goals = " + goals.Length);
			Debug.PrintMessage ("Height of panel = " + this.height);
			for (int pos = 0; pos < goals.Length; pos++) {
				goalPanels.Add (GoalProgressPanel.CreateProgressPanel (this, pos, goals[pos]));
			}

			if (resuming) {
				
				if (m_challenge.m_finished) {
					Debug.PrintMessage ("Challenges have finished already");
				} else if (m_challenge.m_started) { //has started before hand
					Debug.PrintMessage ("Challenge had been started");
				} else {
					Debug.PrintMessage ("Challenge has neither started nor finished but was found in save");
				}
				m_challenge.Resume();
			} else {
				Debug.PrintMessage("Starting Challenges First Time");
				m_challenge.MapStartTime = Data.GetGameDateTime ();
				m_challenge.Start();
			}

			OnChallengeStarted ();

			m_challenge.OnChallengeFinished += (ChallengeEventType type) => {
				switch(type){
				case(ChallengeEventType.ACED):
				case(ChallengeEventType.COMPLETED):
					Debug.PrintMessage("Completed");
					CompleteChallenge(); 
					break;
				case(ChallengeEventType.FAILED):
				case(ChallengeEventType.TOO_LATE):			
					Debug.PrintMessage("Failed");
					ForfeitChallenge();
					break;
				}
			};
			Debug.PrintMessage ("Showing ChallengePanel");
			this.Show ();

		}

		private void AskForfeit(UIComponent source, UIMouseEventParameter eventParam){
			UIDialog forfeitDialog = UIDialog.CreateUIDialog(this.GetUIView(), "FORFEIT CHALLENGE", "Are you sure you would like to forfeit the challenge?", 
				() => {ForfeitChallenge();},
				() => {},
				true);
		}

		private void CompleteChallenge(){
			Debug.PrintMessage ("Completing");
			if (m_challenge.Rewards != null) {
				manager.AddToActiveRewards (m_challenge.Rewards);
			}
			DestroyChallenge ();
		}

		private void ForfeitChallenge(){
			Debug.PrintMessage("Forfeiting");
			if (m_challenge.Penalties != null) {
				manager.AddToActiveRewards (m_challenge.Penalties);
			}
			DestroyChallenge ();
		}

		private void DestroyChallenge(){
			foreach (GoalProgressPanel goalPanel in goalPanels) {
				GameObject.DestroyImmediate(goalPanel);
			}
			Debug.PrintMessage ("Finsihed destroying");
			//m_challenge.Reset ();
			m_challenge = null;
			OnChallengeEnded ();
			this.Hide();
			Debug.PrintMessage ("Hidden");
		}

		public override void Update(){
			base.Update ();
			if (m_challenge != null) {
				m_challenge.UpdateGoals ();
			}
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
				Debug.PrintMessage ("Color changed");
				m_bar.color = new Color32 (255, 255, 0, 255);
			} else {
				m_bar.color = Color.green;
			}
			m_bar.size = m_barBackground.size;
			m_bar.relativePosition = m_barBackground.relativePosition;

			if (m_goal.HasAlreadyPassed) {
				Debug.PrintMessage ("Already Passed");
				this.m_bar.color = Color.cyan;
				this.m_barBackground.color = Color.cyan;
				this.color = new Color32 (0, 128, 128, 255);
			} else if (m_goal.HasAlreadyFailed) {
				Debug.PrintMessage ("Already Failed");
				this.m_bar.color = Color.red;
				this.m_barBackground.color = Color.red;
				this.color = new Color32 (128, 0, 0, 255);
			} else {

				Debug.PrintMessage ("Attaching Pass/Fail events");
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
