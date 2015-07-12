using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ColossalFramework.UI;
using ColossalFramework.Math;
using Challenges;
using UnityEngine;

namespace Challenges
{
	public enum ChallengeEventType{
		ACED,
		COMPLETED,
		TOO_LATE,
		FAILED
	}
	public delegate void ChallengeEvent(ChallengeEventType type);

	[Serializable()]
	public class Challenge{
		[field:NonSerialized]
		bool m_active = false;

		string m_name;
		string m_desc;
		int m_failTolerance;
		int m_passTolerance;
		public bool m_finished = false;
		public bool m_started = false;
		public bool m_hasDeadline = false;

		private int m_years;
		private int m_months;
		public DateTime m_mapStart;
		DateTime m_deadline;
		IGoal[] m_goals;
		IReward[] m_rewards;
		IReward[] m_penalties;

		int m_failedGoals;
		int m_passedGoals;

		public int Years{
			get{return m_years;}
			set{ m_hasDeadline = true;m_years = value;}
		}

		public int Months{
			get{ return m_months;}
			set{ m_hasDeadline = true; m_months = value; }
		}

		public int PassTolerance{
			get { return m_passTolerance;}
			set { this.m_passTolerance = value;}
		}

		public int FailTolerance{
			get { return m_failTolerance;}
			set { this.m_failTolerance = value;}
		}

		public event ChallengeEvent OnChallengeFinished;

		public Challenge(string name, string desc, IGoal[] goals, IReward[] rewards, IReward[] penalties){
			this.m_name = name;
			this.m_desc = desc;
			this.m_goals = goals;
			this.m_rewards = rewards;
			this.m_penalties = penalties;
			this.m_failTolerance = 0;
			this.m_passTolerance = -1;
			AttachEvents();
		}

		public string Name{ get { return m_name;}}
		public string Description{get{ return m_desc;}}
		public IGoal[] Goals{get{ return m_goals;}}
		public IReward[] Rewards{get{ return m_rewards;}}
		public IReward[] Penalties{get{ return m_penalties;}}

		public void AttachEvents(){
			for (int i = 0; i < m_goals.Length; i++) {
				IGoal goal = m_goals [i];
				goal.OnFailed += () => {
					this.m_failedGoals++;
					if (this.m_failedGoals > m_failTolerance) {
						Finish (ChallengeEventType.FAILED);
					}
				};
				goal.OnPassed += () => {
					this.m_passedGoals++;
					if (this.m_passedGoals == this.m_goals.Length) {//completed all goals
						Finish (ChallengeEventType.ACED);
					} else if (this.m_passTolerance != -1 && this.m_passedGoals >= this.m_passTolerance) {//completed enough goals
						Finish (ChallengeEventType.COMPLETED);
					}
				};
			}
		}

		public void Reset(){
			m_started = false;
			m_finished = false;
			if (m_rewards != null) {
				foreach (IReward r in m_rewards) {
					r.Reset ();
				}
			}
			if (m_penalties != null) {
				foreach (IReward p in m_penalties) {
					p.Reset ();
				}
			}
		}

		public void Start(DateTime levelStart){
			m_mapStart = levelStart;
			if (m_hasDeadline) { //if the challenge has a defined period
				m_deadline = m_mapStart.AddYears (m_years).AddMonths (m_months);
			}
			m_started = true;
			m_active = true;
		}

		public void Resume(){
			m_active = true;
		}

		public void UpdateGoals(){
			if (m_started && !m_finished && m_active) {
				foreach (IGoal goal in this.m_goals) {
					goal.Update ();
				}
				if (this.m_hasDeadline && Data.GetGameDateTime () > this.m_deadline) { //problems may occur with this.m_deadline != null removed
					TimeUp ();
				} else if (!m_hasDeadline) {
					HasFinishedChallengesAnytime();
				}
			}
		}

		private void Finish(ChallengeEventType finishType){
			
			if (!m_finished) { //prevents multiple finishes triggering
				m_finished = true;
				OnChallengeFinished (finishType);
			}
		}

		private void HasFinishedChallengesAnytime(){
			int failed = 0;
			int passed = 0;
			foreach (IGoal goal in this.m_goals) {
				if (goal.IsPassing() && goal.PassOnce) {
					passed++;
				} else if (goal.IsFailing() && goal.FailOnce) {
					failed++;
				} 
			} 

			if (passed == this.m_goals.Length) {
				Finish (ChallengeEventType.ACED);
			} else 	if (failed > m_failTolerance) {
				Finish (ChallengeEventType.FAILED);
			} else if (m_passTolerance != -1 && passed >= m_passTolerance) {
				Finish (ChallengeEventType.COMPLETED);
			}
		}

		private void TimeUp(){
			int failed = 0;
			int passed = 0;
			foreach (IGoal goal in this.m_goals) {
				if (goal.IsPassing()) {
					passed++;
				} else if (goal.IsFailing() && goal.FailOnce) {
					failed++;
				} 
			} 

			if (passed == this.m_goals.Length) {
				Finish (ChallengeEventType.ACED);
			} else 	if (failed > m_failTolerance) {
				Finish (ChallengeEventType.FAILED);
			} else if (m_passTolerance != -1 && passed >= m_passTolerance) {
				Finish (ChallengeEventType.COMPLETED);
			} else {
				Finish (ChallengeEventType.TOO_LATE);
			}
		}
	}

	public enum GoalType{
		MAXIMISE,
		MINIMISE
	}

	public class GoalTypeExtension{
		public static GoalType FromComparison(float fail, float pass){
			if (fail < pass) {
				return GoalType.MAXIMISE;
			} else {
				return GoalType.MINIMISE;
			}
		}
	}

	public delegate void GoalEvent();

	public interface IGoal{
		void Update();
		float GetValue();
		float GetProportion();
		bool IsPassing();
		bool IsFailing();
		bool HasPassed();
		bool HasFailed();

		bool HasAlreadyPassed();
		bool HasAlreadyFailed();

		string Name{ get; } 
		string PassValue{ get; }
		string FailValue{ get; }
		GoalType GoalType{ get; }

		bool PassOnce {
			get;
			set;
		}

		bool FailOnce {
			get;
			set;
		}
		 
		//events
		event GoalEvent OnPassed;
		event GoalEvent OnFailed;
		event GoalEvent OnUpdate;
	}

	[Serializable()]
	public class NumericalGoal : IGoal{
		float m_failValue;
		float m_passValue;
	 	float m_value;
		public Data.ValueID m_valueID;
		public GoalType m_goalType;
		bool m_passOnce = true;
		bool m_failOnce = true;

		public bool m_hasPassed = false;
		public bool m_hasFailed = false;

		public NumericalGoal(Data.ValueID valueID, GoalType type, float failValue, float passValue){
			this.m_valueID = valueID;
			this.m_goalType = type;
			this.m_passValue = passValue;
			this.m_failValue = failValue;
			//this.m_value = GetValue ();
		} 

		public bool PassOnce{
			get{ return m_passOnce;}
			set{ m_passOnce = value;}
		}

		public bool FailOnce{
			get{ return m_failOnce;}
			set{ m_failOnce = value;}
		}

		public string PassValue{
			get { return (this.m_goalType == GoalType.MAXIMISE ? this.m_passValue : this.m_failValue).ToString();}
		}

		public string FailValue{
			get { return (this.m_goalType == GoalType.MAXIMISE ? this.m_failValue : this.m_passValue).ToString();}
		}

		public void Update(){
			if (!(m_hasPassed || m_hasFailed)) {
				this.m_value = (float)Data.GetValue(this.m_valueID);
				HasPassed();
				HasFailed();
				OnUpdate();
			}
		}

		public string Name{
			get { return this.m_valueID.ToString();}
		}
		public GoalType GoalType{
			get { return this.m_goalType;}
		}

		public float GetValue(){
			return this.m_value;
		}

		public float GetProportion(){
			float p = 0.5f;
			switch (this.m_goalType) {
			case(GoalType.MAXIMISE):
				p = (this.m_value - m_failValue) / (this.m_passValue - this.m_failValue);
				break;
			case(GoalType.MINIMISE):
				p = (this.m_value - m_passValue) / (this.m_failValue - this.m_passValue);
				break; 
			}
			return (float)Globals.Clamp(p,0,1);
		}

		public bool HasPassed(){
			if (m_hasPassed) {
				return true;
			}

			bool passing = this.IsPassing();
			if (passing && m_passOnce) {
				m_hasPassed = true;
				if (OnPassed != null) {
					OnPassed ();
				}
			}
			return passing;
		}


		public bool IsPassing(){
			if (this.m_goalType == GoalType.MAXIMISE) {
				return (this.m_value >= this.m_passValue);
			} else {
				return this.m_value <= this.m_passValue; 
			}
		}

		public bool IsFailing(){
			if (this.m_goalType == GoalType.MAXIMISE) {
				return (this.m_value <= this.m_failValue);
			} else {
				return this.m_value >= this.m_failValue;
			}
		}


		public bool HasFailed(){
			if (m_hasFailed) {
				return true;
			}
			bool failing = IsFailing ();
			if (failing && m_failOnce) {
				m_hasFailed = true;
				if (OnFailed != null) {
					OnFailed();
				}
			}
			return failing;
		} 

		public bool HasAlreadyPassed(){
			return m_hasPassed;
		}
		public bool HasAlreadyFailed(){
			return m_hasFailed;
		}


		[field:NonSerialized]
		public event GoalEvent OnPassed, OnFailed;
		[field:NonSerialized]
		public event GoalEvent OnUpdate;
	}
}