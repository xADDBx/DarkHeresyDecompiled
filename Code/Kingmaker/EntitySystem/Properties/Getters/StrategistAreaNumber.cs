using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("1e0c797d47a249b1bfae6732f6b443c8")]
public class StrategistAreaNumber : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return Game.Instance.Player.StrategistManager.GetTacticsZoneCount();
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count strategist zones";
	}
}
