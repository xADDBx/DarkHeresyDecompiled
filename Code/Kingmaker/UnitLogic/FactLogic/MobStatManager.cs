using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Stats/MobStatManager")]
[TypeId("276c921d2c4c446cb44a4272bfc6323d")]
public class MobStatManager : UnitDifficultyModifiersManager
{
	protected override void UpdateModifiers()
	{
		RemoveModifiers();
		if (base.Owner.Blueprint.Army != null)
		{
			float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier);
			float num2 = (float)(int)SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier * num;
			int flatModifier = (int)((float)(int)SettingsRoot.Difficulty.EnemyDodgePercentModifier * num);
			float resultPercentModifier = 1f + num2 / 100f;
			AddModifier(StatType.WeaponSkill, GetFlatModifier(resultPercentModifier, StatType.WeaponSkill));
			AddModifier(StatType.BallisticSkill, GetFlatModifier(resultPercentModifier, StatType.BallisticSkill));
			AddModifier(StatType.Strength, GetFlatModifier(resultPercentModifier, StatType.Strength));
			AddModifier(StatType.Toughness, GetFlatModifier(resultPercentModifier, StatType.Toughness));
			AddModifier(StatType.Agility, GetFlatModifier(resultPercentModifier, StatType.Agility));
			AddModifier(StatType.Perception, GetFlatModifier(resultPercentModifier, StatType.Perception));
			AddModifier(StatType.Intelligence, GetFlatModifier(resultPercentModifier, StatType.Intelligence));
			AddModifier(StatType.Willpower, GetFlatModifier(resultPercentModifier, StatType.Willpower));
			AddModifier(StatType.Fellowship, GetFlatModifier(resultPercentModifier, StatType.Fellowship));
			AddModifier(StatType.Defence, flatModifier);
			AddModifier(StatType.ArmorDamageReduction, flatModifier);
		}
	}

	private int GetFlatModifier(float resultPercentModifier, StatType stat)
	{
		return 0;
	}
}
