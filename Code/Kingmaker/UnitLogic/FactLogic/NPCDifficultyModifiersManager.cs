using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("afb82a8851b254243b2a3144d74b0ccf")]
public class NPCDifficultyModifiersManager : UnitDifficultyModifiersManager
{
	[SerializeField]
	private BlueprintFeatureReference m_TroopFeature;

	[SerializeField]
	private BlueprintFeatureReference m_EliteFeature;

	[SerializeField]
	private BlueprintFeatureReference m_BossFeature;

	public BlueprintFeature TroopFeature => m_TroopFeature?.Get();

	public BlueprintFeature EliteFeature => m_EliteFeature?.Get();

	public BlueprintFeature BossFeature => m_BossFeature?.Get();

	protected override void OnDifficultyOrFactionChanged()
	{
		if (base.Owner.Blueprint.Army != null && !base.Owner.Facts.Contains(TroopFeature))
		{
			UnitDifficultyType difficultyType = base.Owner.Blueprint.DifficultyType;
			if (difficultyType == UnitDifficultyType.Swarm || difficultyType == UnitDifficultyType.Common)
			{
				base.Owner.AddFact(TroopFeature);
			}
		}
		if (base.Owner.Blueprint.Army != null && !base.Owner.Facts.Contains(EliteFeature) && base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Elite)
		{
			base.Owner.AddFact(EliteFeature);
		}
		if (base.Owner.Blueprint.Army != null && !base.Owner.Facts.Contains(BossFeature) && base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Boss)
		{
			base.Owner.AddFact(BossFeature);
		}
	}

	protected override void TryApplyDifficultyModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (base.Owner.IsPlayerFaction && stat == StatType.Resolve)
		{
			int num = SettingsRoot.Difficulty.AllyResolveModifier;
			if (num != 0)
			{
				CollectFlatModifier(collector, num);
			}
		}
		if (base.Owner.IsPlayerEnemy && stat == StatType.MovementPoints)
		{
			int num2 = SettingsRoot.Difficulty.EnemyMovementPoints;
			if (num2 != 0)
			{
				CollectFlatModifier(collector, num2);
			}
		}
	}

	protected override void CollectDifficultyAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(StatType.Resolve));
		entries.Add(new AffectedStatEntry(StatType.MovementPoints));
	}
}
