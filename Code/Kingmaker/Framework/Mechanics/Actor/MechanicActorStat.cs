using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct MechanicActorStat
{
	private readonly MechanicActor _actor;

	private readonly StatType _type;

	[ThreadStatic]
	private static StatQueryOutput? _SharedQueryOutput;

	private static StatQueryOutput SharedQueryOutput => _SharedQueryOutput ?? (_SharedQueryOutput = new StatQueryOutput());

	public int ModifiedValue => _actor.GetStat(_type, null, default(StatContext), "ModifiedValue").ModifiedValue;

	public int BaseValue => _actor.GetStat(_type, null, default(StatContext), "BaseValue").BaseValue;

	public int PermanentValue => _actor.GetStatPermanent(_type);

	public int UnclampedValue => _actor.GetStatUnclamped(_type);

	public StatType Type => _type;

	public int Bonus => _actor.GetStatBonus(_type);

	public int PermanentBonus => _actor.GetStatPermanentBonus(_type);

	public bool Enabled => _actor.IsStatEnabled(_type);

	public int Damage => 0;

	public int Drain => 0;

	public int RawBaseValue => _actor.GetStatBase(_type);

	public StatType? BaseStat => _actor.GetStat(_type, null, default(StatContext), "BaseStat").BaseStat;

	public StatType? RawBaseStat => MechanicActor.GetStatBaseStat(_type);

	public StatType? FullOverrideStat => _actor.GetStat(_type, null, default(StatContext), "FullOverrideStat").FullOverrideStat;

	public CompositeModifiersManager Modifiers
	{
		get
		{
			StatQueryOutput sharedQueryOutput = SharedQueryOutput;
			sharedQueryOutput.Clear();
			_actor.GetStat(_type, sharedQueryOutput, default(StatContext), "Modifiers");
			return sharedQueryOutput.Modifiers;
		}
	}

	public CompositeModifiersManager RawModifiers
	{
		get
		{
			StatQueryOutput sharedQueryOutput = SharedQueryOutput;
			sharedQueryOutput.Clear();
			_actor.GetStatWithoutOverride(_type, sharedQueryOutput);
			return sharedQueryOutput.Modifiers;
		}
	}

	public bool HasBonuses
	{
		get
		{
			StatQueryOutput sharedQueryOutput = SharedQueryOutput;
			sharedQueryOutput.Clear();
			_actor.GetStat(_type, sharedQueryOutput, default(StatContext), "HasBonuses");
			return sharedQueryOutput.HasNonPermanentBonuses;
		}
	}

	public bool HasPenalties
	{
		get
		{
			StatQueryOutput sharedQueryOutput = SharedQueryOutput;
			sharedQueryOutput.Clear();
			_actor.GetStat(_type, sharedQueryOutput, default(StatContext), "HasPenalties");
			return sharedQueryOutput.HasNonPermanentPenalties;
		}
	}

	public MechanicActorStat(MechanicActor actor, StatType type)
	{
		_actor = actor;
		_type = type;
	}

	public static implicit operator int(MechanicActorStat s)
	{
		return s.ModifiedValue;
	}

	public override string ToString()
	{
		return Type.ToString();
	}
}
