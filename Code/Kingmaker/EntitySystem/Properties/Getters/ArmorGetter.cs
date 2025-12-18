using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("be189bcf59b8561448110efad9cf9e3d")]
public class ArmorGetter : IntPropertyGetter
{
	public bool Deflection;

	public bool AgainstTarget;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Attacker;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Defender;

	public bool OnlyBodyArmour;

	public bool OnlyNegativeModifiers;

	public bool ItemBonusOnly;

	public bool WithoutPenetration;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string obj = (Deflection ? "Deflection" : "Absorption");
		string text = (AgainstTarget ? Attacker.Colorized() : FormulaTargetScope.Current);
		string text2 = (AgainstTarget ? Defender.Colorized() : FormulaTargetScope.Current);
		string text3 = ((text == text2) ? ("of " + text) : ("of " + text + " against " + text2));
		return obj + " " + text3;
	}
}
