using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("d354ef733c658134a8b5bb61d3dd8201")]
public class PCDifficultyModifier : UnitDifficultyModifiersManager
{
	protected override void UpdateModifiers()
	{
		RemoveModifiers();
		if (base.Owner.IsPlayerEnemy)
		{
			float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
			AddPercentModifier(StatType.HitPoints, (int)((float)(int)SettingsRoot.Difficulty.EnemyHitPointsPercentModifier * num));
			AddPercentModifier(StatType.ArmorDurability, (int)((float)(int)SettingsRoot.Difficulty.EnemyHitPointsPercentModifier * num));
		}
		if (base.Owner.IsPlayerFaction)
		{
			AddModifier(StatType.Resolve, SettingsRoot.Difficulty.AllyResolveModifier);
		}
	}
}
