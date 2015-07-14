using System;
using UnityEngine;
using Challenges;
using ColossalFramework;
using ColossalFramework.Globalization;

namespace Challenges
{

	public interface IReward{
		float Value {
			get;
			set;
		}
		Data.ValueID ValueID {
			get;
			set;
		}

		string Name {
			get;
		}	

		void Reset();
		bool Use();
	}
		
	public class Payment : IReward{
		int m_amount;

		public Payment(int amount){
			this.Value = amount;
		}

		public Data.ValueID ValueID {
			get{ return Data.ValueID.Cash;}
			set{ }
		}

		public float Value{
			get{ return m_amount;}
			set{ m_amount = (int)value;}
		}
		public string Name{
			get{ return "Payment";}
		}
		public bool Use(){
			try{
				Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, m_amount*100, new ItemClass());
				return true;
			}catch(Exception e){
				Globals.printMessage (e.ToString ());
			}
			return false;
		}

		public void Reset(){}

		public override string ToString(){
			return this.m_amount.ToString(Settings.moneyFormat, (IFormatProvider) LocaleManager.cultureInfo);
		}
	}

	public class Boost : IReward{
		float m_value;
		int m_years = 0;
		int m_months = 0;
		public bool isForever = true;
		DateTime m_endDate;
		Data.ValueID m_valueID;

		bool m_active = false;

		public Data.ValueID ValueID{
			get{ return m_valueID;}
			set{ m_valueID = value;}
		}

		public float Value{
			get{ return m_value; }
			set{ m_value = value;}
		}

		public string Name{
			get{ return m_valueID.ToString();}
		}

		public int Years{
			get{ return m_years;}
			set{ m_years = value;isForever = false;}
		}

		public int Months{
			get{ return m_months;}
			set{ m_months = value;isForever = false;}
		}

		public void Reset(){
			m_active = false;
		}

		private void CalculateEndDate(){
			m_endDate = Data.GetGameDateTime().AddYears (m_years).AddMonths (m_months);
			Globals.printMessage ("Years, Months: " + m_years + ", " + m_months);
			Globals.printMessage (m_endDate.ToString());
		}

		public bool Use(){
			if (m_active) {
				//Globals.printMessage ("Using Boost");
				if (isForever || Data.GetGameDateTime () < m_endDate) {
					Data.SetValue (m_valueID, m_value);
				} else {
					Globals.printMessage ("Boost Ended");
					return true;
				}
			} else {
				CalculateEndDate ();
				//Globals.printMessage ("Using boost type: " + m_valueID.ToString ());
				m_active = true;
			}
			return false;
		}

	}
}

