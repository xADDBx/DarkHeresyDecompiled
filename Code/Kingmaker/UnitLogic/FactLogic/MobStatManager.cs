using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Stats/MobStatManager")]
[TypeId("276c921d2c4c446cb44a4272bfc6323d")]
public class MobStatManager : UnitDifficultyModifiersManager
{
	protected override void TryApplyDifficultyModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (base.Owner.Blueprint.Army != null && (uint)(stat - 29) <= 1u)
		{
			int num = SettingsRoot.Difficulty.EnemyDodgeModifier;
			if (num != 0)
			{
				CollectFlatModifier(collector, num);
			}
		}
	}

	protected override void CollectDifficultyAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(StatType.Defence));
		entries.Add(new AffectedStatEntry(StatType.ArmorDamageReduction));
	}
}
