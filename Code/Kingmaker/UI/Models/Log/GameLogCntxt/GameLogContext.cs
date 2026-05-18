using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.UI.Models.Log.GameLogCntxt;

public static class GameLogContext
{
	private class GameLogContextScope : IDisposable
	{
		private IDisposable m_Counter;

		public void Start()
		{
			if (m_Counter == null)
			{
				m_Counter = Counters.CombatLogs?.Measure();
			}
		}

		public void Dispose()
		{
			if (ScopeFlag.Count < 2)
			{
				m_Counter.Dispose();
				m_Counter = null;
				Clear();
			}
			ScopeFlag.Release();
		}
	}

	public readonly struct Property<T>
	{
		private readonly T m_Value;

		[CanBeNull]
		public T Value
		{
			get
			{
				CheckScopeGet();
				return m_Value;
			}
		}

		public Property(T value)
		{
			CheckScopeSet();
			m_Value = value;
		}

		public override string ToString()
		{
			T value = Value;
			return ((value != null) ? value.ToString() : null) ?? "<null>";
		}

		public static implicit operator Property<T>(T value)
		{
			return new Property<T>(value);
		}

		public static implicit operator T(Property<T> property)
		{
			return property.Value;
		}
	}

	private static readonly CountableFlag ScopeFlag = new CountableFlag();

	private static readonly GameLogContextScope s_Scope = new GameLogContextScope();

	public static Property<IMechanicEntity> UnitEntity;

	public static Property<IMechanicEntity> SourceEntity;

	public static Property<IMechanicEntity> TargetEntity;

	public static Property<IMechanicEntityFact> SourceFact;

	public static Property<string> Formula;

	public static Property<string> Text;

	public static Property<string> SecondText;

	public static Property<string> Description;

	public static Property<int> Count;

	public static Property<int> Price;

	public static Property<int> Roll;

	public static Property<IRuleRollD100> D100;

	public static Property<IRuleRollD100> HitD100;

	public static Property<IRuleRollD100> DefenceD100;

	public static Property<IRuleRollD100> BodyPartD100;

	public static Property<IRuleRollD100> CoverHitD100;

	public static Property<int> HitChance;

	public static Property<int> DefenceChance;

	[Obsolete("Defence")]
	public static Property<int> DodgeChance;

	[Obsolete("Defence")]
	public static Property<int> ParryChance;

	public static Property<int> RfChance;

	public static Property<int> CoverHitChance;

	public static Property<int> TotalHitChance;

	public static Property<int> TargetSuperiorityPenalty;

	public static Property<int> PreMitigationDamage;

	public static Property<int> DamageReduction;

	public static Property<int> DifficultyModifier;

	public static Property<int> ResultDamage;

	public static Property<int> Modifier;

	public static Property<int> DC;

	public static Property<int> ChanceDC;

	public static Property<int> Rations;

	public static Property<int> AttackNumber;

	public static Property<int> AttacksCount;

	public static Property<int> RoundNumber;

	[Obsolete]
	public static Property<object> Tooltip;

	public static Property<int> VeilThicknessDelta;

	public static Property<int> VeilThicknessValue;

	public static Property<string> DamageType;

	public static Property<int> MoraleStartValue;

	public static Property<int> MoraleResultValue;

	public static Property<string> MoralePhase;

	public static Property<int> PreviousValue;

	public static Property<int> CurrentValue;

	public static Property<int> TargetValue;

	public static Property<AbilityData> DescriptionAbility;

	public static Property<BlueprintMechanicEntityFact> DescriptionFactBlueprint;

	public static Property<IMechanicEntity> DescriptionOwner;

	public static Property<BlueprintCase> Case;

	public static Property<BlueprintCaseItem> CaseItem;

	public static Property<BlueprintCaseAnswer> CaseAnswer;

	public static Property<BlueprintArea> CaseItemArea;

	public static Property<TextStyle> TextStyle;

	public static IDisposable Scope
	{
		get
		{
			ScopeFlag.Retain();
			s_Scope.Start();
			return s_Scope;
		}
	}

	public static bool InScope => ScopeFlag;

	public static bool HasSource
	{
		get
		{
			if (SourceEntity.Value == null)
			{
				return SourceFact.Value != null;
			}
			return true;
		}
	}

	private static void CheckScopeGet()
	{
		if (!ScopeFlag)
		{
			LogErrorGet("CheckScopeGet");
		}
	}

	private static void CheckScopeSet()
	{
		if (!ScopeFlag)
		{
			LogErrorSet("CheckScopeSet");
		}
	}

	public static PrefixIcon GetIcon()
	{
		CheckScopeGet();
		if (TargetEntity.Value == null)
		{
			return PrefixIcon.None;
		}
		if (TargetEntity.Value.IsPlayerFaction)
		{
			return PrefixIcon.LeftArrow;
		}
		IMechanicEntity value = SourceEntity.Value;
		if (value != null && value.IsPlayerFaction)
		{
			return PrefixIcon.RightArrow;
		}
		return PrefixIcon.RightGreyArrow;
	}

	private static void Clear()
	{
		SourceEntity = default(Property<IMechanicEntity>);
		TargetEntity = default(Property<IMechanicEntity>);
		Text = "";
		Description = "";
		Count = -1;
		Roll = -1;
		DC = -1;
		Modifier = -1;
		D100 = default(Property<IRuleRollD100>);
		ChanceDC = -1;
		AttackNumber = -1;
		AttacksCount = -1;
		Tooltip = default(Property<object>);
		DescriptionAbility = default(Property<AbilityData>);
		DescriptionFactBlueprint = default(Property<BlueprintMechanicEntityFact>);
		DescriptionOwner = default(Property<IMechanicEntity>);
	}

	private static void LogErrorGet([CallerMemberName] string propertyName = null)
	{
		PFLog.Default.Error("GameLogContext." + propertyName + " getter is called outside of a using scope. Please, wrap it into using(GameLogContext.Scope) { }.");
	}

	private static void LogErrorSet([CallerMemberName] string propertyName = null)
	{
		PFLog.Default.Error("GameLogContext." + propertyName + " setter is called outside of a using scope. Please, wrap it into using(GameLogContext.Scope) { }.");
	}
}
