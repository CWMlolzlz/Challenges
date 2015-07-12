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
		void Use();
	}
		
	public class Payment : IReward{
		int m_amount;

		public Payment(float amount){
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
		public void Use(){
			Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, m_amount*100,null);
		}

		public override string ToString(){
			return this.m_amount.ToString(Settings.moneyFormat, (IFormatProvider) LocaleManager.cultureInfo);
		}
	}

	public class Boost : MonoBehaviour , IReward{
		float m_mult;
		float m_baseValue;
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
			get{ return m_mult; }
			set{ m_mult = value;}
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

		public void Use(){
			CalculateEndDate ();
			if (!isForever) {
				m_baseValue = (float)Data.GetValue (m_valueID);
			}
			m_active = true;
		}

		private void CalculateEndDate(){
			m_endDate = Data.GetGameDateTime().AddYears (m_years).AddMonths (m_months);
		}

		void Update(){
			if (m_active) {
				if (isForever || Data.GetGameDateTime () < m_endDate) {
					Data.SetValue(m_valueID, m_baseValue * m_mult);
				} else {
					//boost over
					GameObject.DestroyImmediate(this.gameObject);
				}
			}
		}

	}
}

