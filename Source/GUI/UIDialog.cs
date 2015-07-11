using UnityEngine;
using ColossalFramework.UI;

namespace Challenge.GUI
{
	public class UIDialog : UIPanel{

		public static readonly float WIDTH = 500f, HEIGHT = 250f ;
		public static readonly float HEAD = 40f;
		public static readonly float PADDING = 20f;
		public static readonly float BUTTON_WIDTH = 200f, BUTTON_HEIGHT = 80f;

		UILabel m_details;
		UILabel m_title;

		UIButton m_accept;
		UIButton m_decline;

		public delegate void OptionClicked ();
		public event OptionClicked eventAccept,eventDecline;

		public static UIDialog CreateUIDialog(UIView view, string title, string details, OptionClicked accept, OptionClicked decline, bool destroyOnOption){
			UIDialog dialog = (UIDialog)view.AddUIComponent (typeof(UIDialog));
			dialog.Start ();

			dialog.m_title.text = title;
			dialog.m_details.text = details;

			dialog.eventAccept += accept;
			dialog.eventDecline += decline;

			if (destroyOnOption) {
				dialog.eventAccept += () => {GameObject.DestroyImmediate(dialog.gameObject);};
				dialog.eventDecline += () => {GameObject.DestroyImmediate(dialog.gameObject);};
			}

			return dialog;
		}

		public override void Start(){
			base.Start();

			this.backgroundSprite = "MenuPanel";
			this.size = new Vector2 (WIDTH, HEIGHT);
			this.relativePosition = new Vector2 (WIDTH / 2, HEIGHT / 2);

			m_title = this.AddUIComponent<UILabel>();
			m_title.size = new Vector2(WIDTH,HEAD);
			m_title.relativePosition = Vector3.zero;

			m_details = this.AddUIComponent<UILabel>();
			m_details.size = new Vector2 (WIDTH - PADDING * 2, HEIGHT - HEAD - PADDING * 2);
			m_details.relativePosition = new Vector3 (PADDING, HEAD + PADDING);

			m_accept = this.AddUIComponent<UIButton> ();
			m_accept.text = "Yes";
			m_accept.size = new Vector2 (BUTTON_WIDTH, BUTTON_HEIGHT);
			m_accept.normalBgSprite = "ButtonMenu";
			m_accept.hoveredBgSprite = "ButtonMenuHovered";
			m_accept.pressedBgSprite = "ButtonMenuPressed";
			m_accept.disabledBgSprite = "ButtonMenuDisabled";
			m_accept.relativePosition = new Vector2(WIDTH/2 - PADDING/2 - BUTTON_WIDTH, HEIGHT - PADDING - BUTTON_HEIGHT);
			m_accept.eventClick += (component, eventParam) => {eventAccept();};

			m_decline = this.AddUIComponent<UIButton> ();
			m_decline.text = "No";
			m_decline.size = new Vector2 (BUTTON_WIDTH, BUTTON_HEIGHT);
			m_decline.normalBgSprite = "ButtonMenu";
			m_decline.hoveredBgSprite = "ButtonMenuHovered";
			m_decline.pressedBgSprite = "ButtonMenuPressed";
			m_decline.disabledBgSprite = "ButtonMenuDisabled";
			m_decline.relativePosition = new Vector2(WIDTH/2 + PADDING/2, HEIGHT - PADDING - BUTTON_HEIGHT);
			m_decline.eventClick += (component, eventParam) => {eventDecline();};

		}

	}
}

