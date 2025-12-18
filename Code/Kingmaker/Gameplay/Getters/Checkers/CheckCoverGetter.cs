using System;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("4fb715acade14003af9492cef1a63521")]
public class CheckCoverGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[Flags]
	public enum CoverType
	{
		NoCover = 1,
		Cover = 2,
		LosBlocker = 4
	}

	public PropertyTargetType Target;

	[EnumFlagsAsDropdown]
	public CoverType Cover;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Cover type of {Target.Colorized()} from {FormulaTargetScope.Current} is {Cover}";
	}

	protected override bool GetBaseValue()
	{
		LosCalculations.CoverType coverType = LosCalculations.GetWarhammerLos(base.CurrentEntity, this.GetTargetByType(Target)).CoverType;
		return ((uint)Cover & (uint)(coverType switch
		{
			LosCalculations.CoverType.Cover => 2, 
			LosCalculations.CoverType.LosBlocker => 4, 
			_ => 1, 
		})) != 0;
	}
}
