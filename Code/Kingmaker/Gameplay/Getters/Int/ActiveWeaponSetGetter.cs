using System;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Int;

[Serializable]
[TypeId("26dfca1f189c45d1a4d44a32fac15956")]
public sealed class ActiveWeaponSetGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Active weapon set of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		PartUnitBody bodyOptional = base.CurrentEntity.GetBodyOptional();
		if (bodyOptional == null)
		{
			return 0;
		}
		return bodyOptional.CurrentHandEquipmentSetIndex + 1;
	}
}
