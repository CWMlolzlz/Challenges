using ColossalFramework.Plugins;

namespace ChallengesMod
{
	public class Debug{

		public static readonly bool DEBUG = true;
		public static void PrintMessage(object msg){
			if (DEBUG && msg != null){
				ForcePrintMessage(msg.ToString());
			}
		}
		public static void ForcePrintMessage(object msg){
			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, msg.ToString());
		}

	}
}

