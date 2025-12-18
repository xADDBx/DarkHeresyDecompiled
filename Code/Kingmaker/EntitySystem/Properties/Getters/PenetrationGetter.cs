using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("c204f04abdb243cc881d158d91869f2a")]
public class PenetrationGetter : IntPropertyGetter
{
	public PenetrationParameterType PenetrationParameterType;

	public bool AgainstTarget;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Attacker;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Defender;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = "";
		text = ((!AgainstTarget) ? (" of " + FormulaTargetScope.Current + " against abstract target") : (" of " + Attacker.Colorized() + " against " + Defender.Colorized()));
		return PenetrationParameterType switch
		{
			PenetrationParameterType.ArmorPenetration => "Armor Penetration" + text, 
			PenetrationParameterType.DodgePenetration => "Dodge Penetration" + text, 
			PenetrationParameterType.ArmorPenetrationOverArmor => "Armor Penetration minus Armor" + text, 
			PenetrationParameterType.DodgePenetrationOverDodge => "Dodge Penetration minus Dodge" + text, 
			_ => "", 
		};
	}
}
