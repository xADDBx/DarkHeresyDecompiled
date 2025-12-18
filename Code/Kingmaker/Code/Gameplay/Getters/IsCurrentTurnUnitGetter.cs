using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Getters;

[Serializable]
[TypeId("25370656972246b5a9868474d05fbbe4")]
public class IsCurrentTurnUnitGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		return baseUnitEntity == Game.Instance.Controllers.TurnController.CurrentUnit;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + Target.Colorized() + " Current Turn Unit";
	}
}
