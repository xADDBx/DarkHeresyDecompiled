using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[ComponentName("Equipment/CostOfEquippedArmorGetter")]
[TypeId("48e483ad0eae4739a04f344ec105de81")]
public sealed class CostOfEquippedArmorGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Cost of equipped armor";
	}

	protected override int GetBaseValue()
	{
		return (base.CurrentEntity.GetBodyOptional()?.Armor.MaybeArmor?.Blueprint.Cost).GetValueOrDefault();
	}
}
