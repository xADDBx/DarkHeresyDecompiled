using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Predictions.PredictionProviders;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateSkillCheck : RulebookTargetEvent<MechanicEntity, MechanicEntity>
{
	public readonly CompositeModifiersManager DifficultyModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager();

	public readonly ValueModifiersManager ForceResultModifiers = new ValueModifiersManager();

	private StatContext _statCtx;

	public StatType StatType { get; }

	public StatType EffectiveStatType { get; private set; }

	public int BaseDifficulty { get; }

	public SkillCheckType Type { get; }

	public bool IsSaveFromMaxCritStage { get; }

	private int StatValue => ContextData<PredictionHackContext>.Current?.ModifyStat(StatType, base.Initiator.Actor.GetStat(StatType, null, _statCtx, "StatValue")) ?? ((int)base.Initiator.Actor.GetStat(StatType, null, _statCtx, "StatValue"));

	public int Difficulty => BaseDifficulty + DifficultyModifiers.Value;

	private int EffectiveSkill => StatValue + Difficulty;

	public int ResultChance { get; private set; }

	public RuleCalculateSkillCheck([NotNull] MechanicEntity unit, StatType statType, int baseDifficulty, SkillCheckType type = SkillCheckType.Default, [CanBeNull] MechanicEntity attacker = null, bool isSaveFromMaxCritStage = false)
		: base(unit, attacker ?? unit)
	{
		StatType = statType;
		BaseDifficulty = baseDifficulty;
		Type = type;
		IsSaveFromMaxCritStage = isSaveFromMaxCritStage;
		if (Type != SkillCheckType.Inspect && base.Initiator.IsPlayerFaction)
		{
			int num = SettingsRoot.Difficulty.SkillCheckModifier;
			if (num != 0)
			{
				DifficultyModifiers.Add(ModifierType.ValAdd, num, this, ModifierDescriptor.Difficulty);
			}
		}
		if (Type != SkillCheckType.Inspect && base.Initiator.IsPlayerEnemy)
		{
			int num2 = SettingsRoot.Difficulty.EnemySkillModifier;
			if (num2 != 0)
			{
				DifficultyModifiers.Add(ModifierType.ValAdd, num2, this, ModifierDescriptor.Difficulty);
			}
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		_statCtx = new StatContext(null, null, null, null, null, null, this);
		StatQueryOutput statQueryOutput = new StatQueryOutput();
		base.Initiator.Actor.GetStat(StatType, statQueryOutput, _statCtx, "OnTrigger");
		Modifiers.CopyFrom(statQueryOutput.Modifiers);
		EffectiveStatType = base.Initiator.Actor.GetStat(StatType, null, _statCtx, "OnTrigger").FullOverrideStat ?? StatType;
		ResultChance = ((StatType == StatType.Unknown) ? 100 : EffectiveSkill);
	}
}
