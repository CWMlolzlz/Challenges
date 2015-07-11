using ColossalFramework;
//using ColossalFramework.Plugins;
using ColossalFramework.IO;
using UnityEngine;
using System.Xml;
using System;
using System.IO;
using System.Collections.Generic;
using Challenges;

namespace Challenges{
	public class Data
	{
		public static List<Challenge> m_challenges;
		public static bool m_challengesLoaded = false;
		private const string modXMLPath = "Challenges.xml";
		public static string longLocation = null;

		public static double GetValue(string dataName) {
			District gameWorld = Singleton<DistrictManager>.instance.m_districts.m_buffer[0];
			switch(dataName){
			case("Population"): return gameWorld.m_populationData.m_finalCount;
			case("Cash"): return Math.Round(Singleton<EconomyManager>.instance.LastCashAmount/100.0);
			case("Income"): return Math.Round(Singleton<EconomyManager>.instance.LastCashDelta/100.0);
			case("Average Ground Pollution"): return gameWorld.m_groundData.m_tempPollution;
			case("Land Value"):	return gameWorld.m_groundData.m_finalLandvalue;
			case("Crime Rate"):	return gameWorld.m_finalCrimeRate;
			case("Unemployment"):return gameWorld.GetUnemployment();				
				
			//happiness
			case("Happiness"): return gameWorld.m_finalHappiness;
			case("Residential Happiness"): return gameWorld.m_residentialData.m_finalHappiness;
			case("Commercial Happiness"): return gameWorld.m_commercialData.m_finalHappiness;
			case("Industrial Happiness"): return gameWorld.m_industrialData.m_finalHappiness;
			case("Office Happiness"): return gameWorld.m_officeData.m_finalHappiness;
			}
			throw new System.Exception();
		} 

		public static DateTime GetGameDateTime(){
			return Singleton<SimulationManager>.instance.m_currentGameTime;
		}

		public static List<Challenge> loadXML(){
			Globals.printMessage("Load XML()");
			if (longLocation == null) {				
				//Globals.printMessage(DataLocation.addonsPath);
				longLocation = DataLocation.applicationBase +"\\"+ modXMLPath;
				Globals.printMessage (longLocation);
				//Globals.printMessage(DataLocation.assetsPath);
				//Globals.printMessage(DataLocation.addonsPath);
				//Globals.printMessage(DataLocation.addonsPath);
			}
			if (FileUtils.Exists (longLocation)) {
				Globals.printMessage("File Exists");
				List<Challenge> list = new List<Challenge> ();

				XmlDocument xmldoc = new XmlDocument ();
				FileStream fs = new FileStream (longLocation, FileMode.Open, FileAccess.Read);
				xmldoc.Load (fs); 
				XmlNodeList challengeNodes = xmldoc.SelectNodes ("challengelist") [0].SelectNodes ("challenge");
				foreach (XmlNode challengeNode in challengeNodes) {	//foreach challenge
					XmlAttributeCollection challengeAtts = challengeNode.Attributes;
					FastList<IGoal> goalsToAdd = new FastList<IGoal> ();
					int years = -1, months = -1;
					foreach (XmlNode node in challengeNode.ChildNodes) {

						if (node.Name == "goal") {						
							string name = node.Attributes ["name"].Value; 
							float passValue = float.Parse (node.Attributes ["passValue"].Value);
							float failValue = float.Parse (node.Attributes ["failValue"].Value);
							NumericalGoal goal = new NumericalGoal (name, GoalTypeExtension.FromComparison (failValue, passValue), failValue, passValue);
							if (node.Attributes ["passOnce"] != null) {
								goal.PassOnce = bool.Parse (node.Attributes ["passOnce"].Value);
							}
							if (node.Attributes ["failOnce"] != null) {
								goal.FailOnce = bool.Parse (node.Attributes ["failOnce"].Value);
							}
							goalsToAdd.Add (goal);
						} else if (node.Name == "deadline") {
							if (node.Attributes ["years"] != null)
								years = int.Parse (node.Attributes ["years"].Value);
							if (node.Attributes ["months"] != null)
								months = int.Parse (node.Attributes ["months"].Value);
						}
					}
					Challenge newChallenge = new Challenge (challengeAtts ["name"].Value, challengeAtts ["desc"].Value, goalsToAdd.ToArray ());
					if (challengeAtts ["requires"] != null) {
						newChallenge.PassTolerance = int.Parse (challengeAtts ["requires"].Value);
					}
					if (challengeAtts ["failTolerance"] != null) {
						newChallenge.FailTolerance = int.Parse (challengeAtts ["failTolerance"].Value);
					}
					if (years >= 0) {
						newChallenge.Years = years;
					}
					if (months >= 0) {
						newChallenge.Months = months;
					}

					list.Add (newChallenge);
				}
				m_challengesLoaded = true;
				return list;
				 
				Globals.printMessage (list.Count);
			} else {
				CreateXML ();
				return loadXML ();
			}

		}

		private static void CreateXML (){
			string fileContents = "<?xml version='1.0' encoding='UTF-8' ?>\n<challengelist>\n\t<challenge version='1.0' name='Population 1' desc='Reach a population count of 2000' failTolerance='0'>\n\t\t<goal name='Population' failValue='0' passValue='2000' passOnce='true' failOnce='false'/>\n\t</challenge>\n\t<challenge version='1.0' name='Income 1' desc='Have an income of over $2000 per week' failTolerance='0'>\n\t\t<goal name='Income' failValue='-1000' passValue='2000' passOnce='true' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Population 2' desc='Get a population of 10000 people and keep your crime rate below 8%' failTolerance='0' passTolerance='1'>\n\t\t<goal name='Population' failValue='0' passValue='10000' passOnce='true' failOnce='false'/>\n\t\t<goal name='Crime Rate' failValue='8' passValue='0' passOnce='false' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Income 2' desc='Have an income of over $8000 per week while keeping your commercial zones happy' failTolerance='0' passTolerance='1'>\n\t\t<goal name='Income' failValue='-200' passValue='8000' passOnce='true' failOnce='true'/>\n\t\t<goal name='Commercial Happiness' failValue='60' passValue='100' passOnce='false' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Population 3' desc='Reach a population count of 50000 while keeping them happy' failTolerance='0'>\n\t\t<goal name='Population' failValue='0' passValue='50000' passOnce='true' failOnce='false'/>\n\t\t<goal name='Residential Happiness' failValue='75' passValue='100' passOnce='false' failOnce='true'/>\n\t</challenge>\n\t<challenge version='1.0' name='Income 3' desc='Have an income of over $15000 per week without ever running at a loss' failTolerance='0' passTolerance='1'>\n\t\t<goal name='Income' failValue='0' passValue='8000' passOnce='true' failOnce='true'/>\n\t</challenge>\n</challengelist>";
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(longLocation, true))
			{
				file.WriteLine(fileContents);
			}
		}
	}
}