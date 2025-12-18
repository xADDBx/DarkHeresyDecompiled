using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("5122bc0b20863d749bd0fc23b8ac58d7")]
public class CoverGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[Flags]
	public enum CoverType
	{
		None = 1,
		Half = 2,
		Full = 4,
		Invisible = 8
	}

	public PropertyTargetType Target;

	public bool UseBestShootingPosition;

	[EnumFlagsAsDropdown]
	public CoverType Covers;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Cover type of " + Target.Colorized() + " from " + FormulaTargetScope.Current;
	}
}
