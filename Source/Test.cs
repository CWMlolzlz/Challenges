using System;
using System.Xml;

namespace ChallengesMod
{
	public class Test
	{
		public static void TestChallengeToString(){
			Debug.PrintMessage ("Testing");
			string str = @"<challenge version='1.0' name='Population 1' desc='Reach a population count of 2000' failTolerance='0'>
		<goal name='Population' failValue='0' passValue='2000' passOnce='true' failOnce='false'/>
		<reward>
			<payment amount='200000'/>
			<boost name='ResidentialDemand' value='100' years='0' months='6'/>
		</reward>
		<penalty>
			<payment amount='-20000'/>			
		</penalty>
	</challenge>";
			string str2 = @"<challenge version='1.0' name='Population 2' desc='Get a population of 10000 people and keep your crime rate below 8%' failTolerance='0' passTolerance='1'>
		<goal name='Population' failValue='0' passValue='10000' passOnce='true' failOnce='false'/>
		<goal name='Crime Rate' failValue='8' passValue='0' passOnce='false' failOnce='true'/>
<deadline months='2' years='1'/>
	</challenge>";
			Challenge challenge = Data.XMLStringToChallenge(str);
			challenge.MapStartTime = System.DateTime.Now;
			challenge.Start ();
			Debug.PrintMessage (Data.ChallengeToXMLString(challenge));

			/*
			XmlDocument doc = new XmlDocument ();
			XmlElement challengeElem = doc.CreateElement ("challenge");
			challengeElem.SetAttribute ("name", challenge.Name);
			challengeElem.SetAttribute ("desc", challenge.Description);
			challengeElem.SetAttribute ("failTolerance", challenge.FailTolerance.ToString());
			challengeElem.SetAttribute ("passTolerance", challenge.PassTolerance.ToString());
			challengeElem.SetAttribute ("startDate", challenge.MapStartTime.ToString());

			foreach (IGoal goal in challenge.Goals) {
				XmlElement goalElem = Data.NumericalGoalToXMLElement(goal as NumericalGoal,doc);
				Debug.PrintMessage (goalElem.OuterXml);
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

			Debug.PrintMessage (challengeElem.OuterXml);*/
		}
	}
}

