using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateMoraleChange : RulebookTargetEvent
{
	public readonly CompositeModifiersManager ValueModifier = new CompositeModifiersManager();

	public readonly CompositeModifiersManager PositiveValueModifier = new CompositeModifiersManager();

	public readonly CompositeModifiersManager NegativeValueModifier = new CompositeModifiersManager();

	public readonly CompositeModifiersManager BottomLimitModifier;

	public readonly CompositeModifiersManager TopLimitModifier;

	public readonly FlagModifiersManager InversePositiveFlag = new FlagModifiersManager();

	public readonly FlagModifiersManager InverseNegativeFlag = new FlagModifiersManager();

	public readonly MoraleEventType EventType;

	public readonly int BaseValue;

	private static MoraleRoot Settings => MoraleRoot.Instance;

	public bool ResultInverse { get; private set; }

	public int ResultDeltaRaw { get; private set; }

	public int ResultDelta { get; private set; }

	public int Result { get; private set; }

	[NotNull]
	public new BaseUnitEntity TargetUnit => (base.Target as BaseUnitEntity) ?? throw new InvalidOperationException("Target is not a BaseUnitEntity");

	public RuleCalculateMoraleChange([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, MoraleEventType eventType, int baseValue = 0)
		: base(initiator, target)
	{
		BottomLimitModifier = new CompositeModifiersManager(Settings.MinValue);
		TopLimitModifier = new CompositeModifiersManager(int.MinValue, Settings.MaxValue);
		EventType = eventType;
		BaseValue = baseValue + CalculateAutoRegen();
		BottomLimitModifier.Add(ModifierType.ValAdd, Settings.MinValue, this, ModifierDescriptor.BaseValue);
		TopLimitModifier.Add(ModifierType.ValAdd, Settings.MaxValue, this, ModifierDescriptor.BaseValue);
		ValueModifier.Add(ModifierType.ValAdd, BaseValue, this, ModifierDescriptor.BaseValue);
		AddDeadLeaderModifier();
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (MoraleCheats.MoraleDisableChanges)
		{
			ValueModifier.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Cheat);
		}
		else if (TargetUnit.Morale.IsPhaseLocked)
		{
			ValueModifier.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.MoraleChangeLocked);
		}
		ResultDeltaRaw = ValueModifier.Value;
		IRulebookEvent previous = context.Previous;
		RulePerformMoraleChange performRule = previous as RulePerformMoraleChange;
		if (performRule != null)
		{
			EventBus.RaiseEvent((IMechanicEntity)Target, (Action<IMoraleChangeRawValueCalculatedTrigger>)delegate(IMoraleChangeRawValueCalculatedTrigger h)
			{
				h.HandleMoraleChangeRawValueCalculated(performRule);
			}, isCheckRuntime: true);
		}
		bool flag = ResultDeltaRaw > 0;
		bool flag2 = ResultDeltaRaw < 0;
		if (flag)
		{
			ValueModifier.CopyFrom(PositiveValueModifier);
			ApplyDifficultyMoraleModifier(GetDifficultyPositiveMoraleModifier());
		}
		else if (flag2)
		{
			ValueModifier.CopyFrom(NegativeValueModifier);
			ApplyDifficultyMoraleModifier(GetDifficultyNegativeMoraleModifier());
		}
		ResultInverse = (flag && InversePositiveFlag.Value) || (flag2 && InverseNegativeFlag.Value);
		int num = (ResultInverse ? (-ValueModifier.Value) : ValueModifier.Value);
		bool num2 = (ResultDeltaRaw > 0 && !ResultInverse) || (ResultDeltaRaw < 0 && ResultInverse);
		bool flag3 = (ResultDeltaRaw < 0 && !ResultInverse) || (ResultDeltaRaw > 0 && ResultInverse);
		if (num2)
		{
			num = Math.Max(0, num);
		}
		else if (flag3)
		{
			num = Math.Min(0, num);
		}
		ResultDelta = num;
		if (ResultDelta > 0 && (bool)Target.Features.MoraleCanNotGainMorale)
		{
			ResultDelta = 0;
			ValueModifier.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.CanNotGainMorale);
		}
		int num3 = TargetUnit.Morale.Value;
		if (ResultDelta < 0 && Target.IsInPlayerParty)
		{
			PartMorale morale = TargetUnit.Morale;
			if (morale != null && morale.Phase == MoralePhaseType.Heroic && !morale.IsPhaseLocked)
			{
				num3 = 0;
			}
		}
		if (ResultDelta > 0)
		{
			ResultDelta = ((num3 + ResultDelta > TopLimitModifier.Value) ? Math.Max(0, TopLimitModifier.Value - num3) : ResultDelta);
		}
		else
		{
			ResultDelta = ((num3 + ResultDelta < BottomLimitModifier.Value) ? Math.Min(0, BottomLimitModifier.Value - num3) : ResultDelta);
		}
		Result = num3 + ResultDelta;
	}

	private int GetDifficultyPositiveMoraleModifier()
	{
		if (!Target.IsInPlayerParty)
		{
			if (!Target.IsPlayerEnemy)
			{
				return 0;
			}
			return Math.Min(0, SettingsRoot.Difficulty.EnemyPositiveMoraleChangeModifier);
		}
		return SettingsRoot.Difficulty.PartyPositiveMoraleChangeModifier;
	}

	private int GetDifficultyNegativeMoraleModifier()
	{
		if (!Target.IsInPlayerParty)
		{
			if (!Target.IsPlayerEnemy)
			{
				return 0;
			}
			return SettingsRoot.Difficulty.EnemyNegativeMoraleChangeModifier;
		}
		return SettingsRoot.Difficulty.PartyNegativeMoraleChangeModifier;
	}

	private void ApplyDifficultyMoraleModifier(int value)
	{
		if (value != 0)
		{
			ValueModifier.Add(ModifierType.ValAdd, value, this, ModifierDescriptor.Difficulty);
		}
	}

	private void AddDeadLeaderModifier()
	{
		MoraleController moraleController = Game.Instance.Controllers.MoraleController;
		int num = 0;
		ModifierDescriptor descriptor = ModifierDescriptor.None;
		if (EventType.HasAnyFlag(MoraleEventType.TurnStart) && !moraleController.AreAllLeadersDeadOrUnconscious)
		{
			if (moraleController.IsAllyLeaderDead(TargetUnit))
			{
				num = MoraleRoot.Instance.AllyLeaderDeathPenalty.GetValue();
				descriptor = ModifierDescriptor.AllyLeaderDead;
			}
			else if (moraleController.AreAllEnemyLeadersDead(TargetUnit))
			{
				num = MoraleRoot.Instance.EnemyLeaderDeadAtTurnStartBonus.GetValue();
				descriptor = ModifierDescriptor.EnemyLeaderDead;
			}
		}
		else if (EventType.HasAnyFlag(MoraleEventType.LeaderEnemyDeath))
		{
			num = MoraleRoot.Instance.EnemyLeaderDeathBonus.GetValue();
			descriptor = ModifierDescriptor.EnemyLeaderDead;
		}
		if (num != 0)
		{
			ValueModifier.Add(ModifierType.ValAdd, num, this, descriptor);
		}
	}

	private int CalculateAutoRegen()
	{
		if (!EventType.HasAnyFlag(MoraleEventType.TurnStart))
		{
			return 0;
		}
		int value = Settings.RegenAtTurnStart.GetValue();
		if (TargetUnit.Morale.Value <= 0)
		{
			return -1 * Math.Max(-1 * value, TargetUnit.Morale.Value);
		}
		return -1 * Math.Min(value, TargetUnit.Morale.Value);
	}
}
