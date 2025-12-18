using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("cff98a45656e7cd44a35b7a372fe0187")]
public class FirstWeaponSoundSizeGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current weapon of " + FormulaTargetScope.Current + " sound size";
	}

	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.GetFirstWeapon()?.Blueprint.VisualParameters.SoundSizeSwitch.ValueHash ?? 0);
	}
}
