using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.IO;
using UnityEngine;
using System.Xml;
using System;
using System.IO;
using System.Collections.Generic;
using ChallengesMod;


namespace ChallengesMod{
	public class Data
	{

		public interface IDataAccessor{
			ValueID ValueID{ get; set;}
		}

		public enum ValueID{
			Population,
			Cash,
			Income,
			AverageGroundPollution,
			LandValue,
			CrimeRate,
			Unemployment,
			Happiness,
			ResidentialHappiness,
			CommercialHappiness,
			IndustrialHappiness,
			OfficeHappiness,

			ResidentialDemand,
			CommericialDemand,
			IndustrialDemand
		}

		public static List<Challenge> m_challenges;
		private static readonly string basicModFolder = DataLocation.applicationBase + @"\Challenges";
		private static readonly string modXMLPath = "Challenges.xml";

		public static double GetValue(ValueID dataName) {
			District gameWorld = Singleton<DistrictManager>.instance.m_districts.m_buffer[0];
			switch(dataName){
			case(ValueID.Population): return gameWorld.m_populationData.m_finalCount;
			case(ValueID.Cash): return Math.Round(Singleton<EconomyManager>.instance.LastCashAmount/100.0);
			case(ValueID.Income): return Math.Round(Singleton<EconomyManager>.instance.LastCashDelta/100.0);
			case(ValueID.AverageGroundPollution): return gameWorld.m_groundData.m_tempPollution;
			case(ValueID.LandValue): return gameWorld.m_groundData.m_finalLandvalue;
			case(ValueID.CrimeRate): return gameWorld.m_finalCrimeRate;
			case(ValueID.Unemployment): return gameWorld.GetUnemployment();				
				
			//happiness
			case(ValueID.Happiness): return gameWorld.m_finalHappiness;
			case(ValueID.ResidentialHappiness): return gameWorld.m_residentialData.m_finalHappiness;
			case(ValueID.CommercialHappiness): return gameWorld.m_commercialData.m_finalHappiness;
			case(ValueID.IndustrialHappiness): return gameWorld.m_industrialData.m_finalHappiness;
			case(ValueID.OfficeHappiness): return gameWorld.m_officeData.m_finalHappiness;

			case(ValueID.ResidentialDemand): return Singleton<ZoneManager>.instance.m_residentialDemand;
			case(ValueID.CommericialDemand): return Singleton<ZoneManager>.instance.m_commercialDemand;
			case(ValueID.IndustrialDemand): return Singleton<ZoneManager>.instance.m_workplaceDemand;
			}
			return 0;
		}

		public static void AddValue(ValueID id, double value){
			SetValue (id, GetValue (id) + value);
		}

		public static void SetValue(ValueID id, double value){
			switch(id){
			case(ValueID.ResidentialDemand):
				Singleton<ZoneManager>.instance.m_actualResidentialDemand= (int)value;
				Singleton<ZoneManager>.instance.m_residentialDemand = (int)value;
				break;
			case(ValueID.CommericialDemand):
				Singleton<ZoneManager>.instance.m_actualCommercialDemand= (int)value;
				Singleton<ZoneManager>.instance.m_commercialDemand = (int)value;
				break;
			case(ValueID.IndustrialDemand):
				Singleton<ZoneManager>.instance.m_actualWorkplaceDemand= (int)value;
				Singleton<ZoneManager>.instance.m_workplaceDemand= (int)value;
				break;
			}
		}

		public static ValueID StringToValueID(string idName){
			return (ValueID)Enum.Parse (typeof(ValueID), idName.Replace(" ", string.Empty));
		}

		public static DateTime GetGameDateTime(){
			return Singleton<SimulationManager>.instance.m_currentGameTime;
		}

		public static List<Challenge> SearchForMods(){
			List<Challenge> output = new List<Challenge> ();
			if (!Directory.Exists (basicModFolder)) {
				CreateChallengesFolder (basicModFolder);
			}
			SearchFolderForChallenges (basicModFolder, ref output, -1);
			foreach (PluginManager.PluginInfo info in Singleton<PluginManager>.instance.GetPluginsInfo()) {
				//Debug.PrintMessage (info.modPath);
				SearchFolderForChallenges (info.modPath, ref output, 2); //dont go more than 2 folders deep, reduces search time drastically 
			}
			Debug.PrintMessage (output.Count);
			return output;
		}

		public static void SearchFolderForChallenges(string dir, ref List<Challenge> challenges, int depth){
			if (depth != 0) {
				foreach (string subFolder in Directory.GetDirectories(dir)) {
					//Debug.PrintMessage (subFolder);
					SearchFolderForChallenges (subFolder, ref challenges, depth - 1);
				}
			}

			foreach (string file in Directory.GetFiles(dir)) {
				//Debug.PrintMessage (file);
				if (file.Contains (".xml")) {
					LoadXML (file, ref challenges);
				}
			}


		}

		public static void LoadXML(string fileLocation, ref List<Challenge> challenges){
			try{
			//if (FileUtils.Exists (fileLocation)){
				XmlDocument xmldoc = new XmlDocument ();
				FileStream fs = new FileStream (fileLocation, FileMode.Open, FileAccess.Read);
				xmldoc.Load (fs); 
				XmlNodeList challengeNodes = xmldoc.SelectNodes ("challengelist") [0].SelectNodes ("challenge");
				foreach (XmlNode challengeNode in challengeNodes) {	//foreach challenge
					challenges.Add(ChallengeNodeToChallenge(challengeNode,false));
				}

			}catch(Exception e){
				Debug.PrintMessage (e.ToString ());
			}
		}



		public static Challenge ChallengeNodeToChallenge(XmlNode challengeNode, bool loadStates){
			XmlAttributeCollection challengeAtts = challengeNode.Attributes;
			FastList<IGoal> goalsToAdd = new FastList<IGoal> ();
			FastList<IReward> rewardsToAdd = new FastList<IReward> ();
			FastList<IReward> penaltiesToAdd = new FastList<IReward> ();

			int years = -1, months = -1;
			foreach (XmlNode node in challengeNode.ChildNodes) {
				//Debug.PrintMessage(node.Name);
				if (node.Name == "goal") {				
					string name = node.Attributes ["name"].Value; 
					float passValue = float.Parse (node.Attributes ["passValue"].Value);
					float failValue = float.Parse (node.Attributes ["failValue"].Value);
					NumericalGoal goal = new NumericalGoal (StringToValueID (name), GoalTypeExtension.FromComparison (failValue, passValue), failValue, passValue);
					if (node.Attributes ["passOnce"] != null) {
						goal.PassOnce = bool.Parse (node.Attributes ["passOnce"].Value);
					}
					if (node.Attributes ["failOnce"] != null) {
						goal.FailOnce = bool.Parse (node.Attributes ["failOnce"].Value);
					}
					if (loadStates && node.Attributes["hasAlreadyFailed"] != null) {
						goal.HasAlreadyFailed = bool.Parse(node.Attributes ["hasAlreadyFailed"].Value);
					}
					if (loadStates && node.Attributes["hasAlreadyPassed"] != null) {
						goal.HasAlreadyPassed = bool.Parse(node.Attributes ["hasAlreadyPassed"].Value);
					}
					goalsToAdd.Add (goal);
				} else if (node.Name == "deadline") {
					if (node.Attributes ["years"] != null)
						years = int.Parse (node.Attributes ["years"].Value);
					if (node.Attributes ["months"] != null)
						months = int.Parse (node.Attributes ["months"].Value);
					
				} else if (node.Name == "reward") {
					foreach (XmlNode rewardNode in node.ChildNodes) {
						if (rewardNode.Name == "boost") {
							Boost newBoost = new Boost ();
							newBoost.ValueID = Data.StringToValueID(rewardNode.Attributes ["name"].Value);
							newBoost.Value = float.Parse (rewardNode.Attributes ["value"].Value);

							if (rewardNode.Attributes ["years"] != null){
								newBoost.Years = int.Parse (rewardNode.Attributes ["years"].Value);
							}
							if (rewardNode.Attributes ["months"] != null){
								newBoost.Months = int.Parse (rewardNode.Attributes ["months"].Value);
							}
							rewardsToAdd.Add (newBoost);
						} else if (rewardNode.Name == "payment") {
							rewardsToAdd.Add(new Payment (int.Parse (rewardNode.Attributes ["amount"].Value)));
						}
					}
				} else if (node.Name == "penalty") {
					foreach (XmlNode rewardNode in node.ChildNodes) {
						if (rewardNode.Name == "payment") {
							penaltiesToAdd.Add(new Payment (int.Parse(rewardNode.Attributes ["amount"].Value)));
						}
					}
				}
			}
			Challenge newChallenge = new Challenge (challengeAtts ["name"].Value, challengeAtts ["desc"].Value, goalsToAdd.ToArray (), rewardsToAdd.ToArray(), penaltiesToAdd.ToArray());
			Debug.PrintMessage(challengeNode.OuterXml);
			if (challengeAtts ["requires"] != null) {
				newChallenge.PassTolerance = int.Parse (challengeAtts ["requires"].Value);
			}
			if (challengeAtts ["failTolerance"] != null) {
				newChallenge.FailTolerance = int.Parse (challengeAtts ["failTolerance"].Value);
			}
			if (loadStates && challengeAtts ["startDate"] != null) {
				newChallenge.MapStartTime = DateTime.Parse (challengeAtts ["startDate"].Value);
			}
			if (years >= 0) {
				newChallenge.Years = years;
			}
			if (months >= 0) {
				newChallenge.Months = months;
			}
			return newChallenge;
		}

		public static Challenge XMLStringToChallenge(string xml){
			XmlDocument xmldoc = new XmlDocument ();
			xmldoc.LoadXml (xml);
			return ChallengeNodeToChallenge (xmldoc.SelectSingleNode ("challenge"),true);
		}

		public static string ActiveRewardsToXMLString(IReward[] activeRewards){
			XmlDocument doc = new XmlDocument ();
			foreach (IReward r in activeRewards) {
				if (r.GetType ().Equals (typeof(Boost))) {
					Boost b = r as Boost;
					XmlElement boostElem = doc.CreateElement("boost");
					boostElem.SetAttribute ("name",b.ValueID.ToString());
					boostElem.SetAttribute ("value",b.Value.ToString());
					boostElem.SetAttribute ("endDate",b.EndDate.ToString());
					doc.AppendChild (boostElem);
				}
			}
			return doc.OuterXml;
		}

		public static string ChallengeToXMLString(Challenge challenge){
			XmlDocument doc = new XmlDocument ();
			XmlElement challengeElem = doc.CreateElement ("challenge");
			challengeElem.SetAttribute ("name", challenge.Name);
			challengeElem.SetAttribute ("desc", challenge.Description);
			challengeElem.SetAttribute ("failTolerance", challenge.FailTolerance.ToString());
			challengeElem.SetAttribute ("passTolerance", challenge.PassTolerance.ToString());
			challengeElem.SetAttribute ("startDate", challenge.MapStartTime.ToString());

			foreach (IGoal goal in challenge.Goals) {
				XmlElement goalElem = Data.NumericalGoalToXMLElement (goal as NumericalGoal,doc);
				challengeElem.AppendChild (goalElem);
			}

			if (challenge.m_hasDeadline) {
				XmlElement deadlineElem = doc.CreateElement ("deadline");
				deadlineElem.SetAttribute ("years", challenge.Years.ToString ());
				deadlineElem.SetAttribute ("months", challenge.Months.ToString ());
				challengeElem.AppendChild (deadlineElem);
			}

			if (challenge.Rewards != null) {
				XmlElement rewardsElem = doc.CreateElement ("reward");
				foreach (IReward reward in challenge.Rewards) {
					XmlElement rewardElem;
					if (reward.GetType ().Equals (typeof(Boost))) {
						rewardElem = Data.BoostToXmlElement (reward as Boost, doc);
						rewardsElem.AppendChild (rewardElem);
					} else if (reward.GetType ().Equals (typeof(Payment))) {
						rewardElem = Data.PaymentToXmlElement (reward as Payment, doc);
						rewardsElem.AppendChild (rewardElem);
					} else {
						rewardElem = null;
					}
				}
				challengeElem.AppendChild (rewardsElem);
			}

			if (challenge.Penalties != null) {
				XmlElement penaltiesElem = doc.CreateElement ("penalty");
				foreach (IReward reward in challenge.Penalties) {
					XmlElement penaltyElem;
					if (reward.GetType ().Equals (typeof(Boost))) {
						penaltyElem = Data.BoostToXmlElement (reward as Boost, doc);
						penaltiesElem.AppendChild (penaltyElem);
					} else if (reward.GetType ().Equals (typeof(Payment))) {
						penaltyElem = Data.PaymentToXmlElement (reward as Payment, doc);
						penaltiesElem.AppendChild (penaltyElem);
					} else {
						penaltyElem = null;
					}
				}
				challengeElem.AppendChild (penaltiesElem);
			}



			return challengeElem.OuterXml;
		}

		public static XmlElement NumericalGoalToXMLElement(NumericalGoal goal, XmlDocument doc){
			//<goal name='Population' failValue='0' passValue='2000' passOnce='true' failOnce='false'/>
			XmlElement goalElem = doc.CreateElement("goal");
			goalElem.SetAttribute ("name", goal.ValueID.ToString());
			goalElem.SetAttribute ("failValue", goal.FailValue);
			goalElem.SetAttribute ("passValue", goal.PassValue);
			goalElem.SetAttribute ("failOnce", goal.FailOnce.ToString());
			goalElem.SetAttribute ("passOnce", goal.PassOnce.ToString());
			goalElem.SetAttribute ("hasAlreadyFailed", goal.HasAlreadyFailed.ToString());
			goalElem.SetAttribute ("hasAlreadyPassed", goal.HasAlreadyPassed.ToString());
			return goalElem;
		}

		public static XmlElement BoostToXmlElement(Boost boost, XmlDocument doc){
			XmlElement boostElem = doc.CreateElement("boost");
			boostElem.SetAttribute ("name", boost.ValueID.ToString());
			boostElem.SetAttribute ("value", boost.Value.ToString());
			if (!boost.isForever) {
				boostElem.SetAttribute ("years", boost.Years.ToString());
				boostElem.SetAttribute ("months", boost.Months.ToString());
			}
			return boostElem;
		}

		public static XmlElement PaymentToXmlElement(Payment payment, XmlDocument doc){
			XmlElement paymentElem = doc.CreateElement("payment");
			paymentElem.SetAttribute ("amount", payment.Value.ToString());

			return paymentElem;
		}


		private static void CreateChallengesFolder (string location){
			string fileContents = "<?xml version='1.0' encoding='UTF-8' ?>\n<challengelist>\n\t<challenge version='1.0' name='Population 1' desc='Reach a population count of 2000' failTolerance='0'>\n\t\t<goal name='Population' failValue='0' passValue='2000' passOnce='true' failOnce='false'/>\n\t</challenge>\n\t<challenge version='1.0' name='Income 1' desc='Have an income of over $2000 per week' failTolerance='0'>\n\t\t<goal name='Income' failValue='-1000' passValue='2000' passOnce='true' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Population 2' desc='Get a population of 10000 people and keep your crime rate below 8%' failTolerance='0' passTolerance='1'>\n\t\t<goal name='Population' failValue='0' passValue='10000' passOnce='true' failOnce='false'/>\n\t\t<goal name='Crime Rate' failValue='8' passValue='0' passOnce='false' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Income 2' desc='Have an income of over $8000 per week while keeping your commercial zones happy' failTolerance='0' passTolerance='1'>\n\t\t<goal name='Income' failValue='-200' passValue='8000' passOnce='true' failOnce='true'/>\n\t\t<goal name='Commercial Happiness' failValue='60' passValue='100' passOnce='false' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Population 3' desc='Reach a population count of 50000 while keeping them happy' failTolerance='0'>\n\t\t<goal name='Population' failValue='0' passValue='50000' passOnce='true' failOnce='false'/>\n\t\t<goal name='Residential Happiness' failValue='75' passValue='100' passOnce='false' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Income 3' desc='Have an income of over $15000 per week without ever running at a loss' failTolerance='0' passTolerance='1'>\n\t\t<goal name='Income' failValue='0' passValue='8000' passOnce='true' failOnce='true'/>\n\t</challenge>\n</challengelist>";
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(location, true))
			{
				file.WriteLine(fileContents);
			}
		}
	}
}