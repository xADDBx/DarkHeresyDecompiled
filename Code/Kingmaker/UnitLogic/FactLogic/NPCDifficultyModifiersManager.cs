using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
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

	protected override void UpdateModifiers()
	{
		RemoveModifiers();
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
		if (base.Owner.IsPlayerEnemy)
		{
			AddPercentModifier(StatType.HitPoints, SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
			AddPercentModifier(StatType.ArmorDurability, SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
		}
		if (base.Owner.IsPlayerFaction)
		{
			AddModifier(StatType.Resolve, SettingsRoot.Difficulty.AllyResolveModifier);
		}
	}
}
