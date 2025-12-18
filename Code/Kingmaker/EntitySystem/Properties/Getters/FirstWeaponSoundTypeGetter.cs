using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("285451c689d01374a9b3a07bc158cad2")]
public class FirstWeaponSoundTypeGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current weapon of " + FormulaTargetScope.Current + " sound type";
	}

	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.GetFirstWeapon()?.Blueprint.VisualParameters.SoundTypeSwitch.ValueHash ?? 0);
	}
}
