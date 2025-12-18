using System;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5199ec18b22b8b1459f37128aebfda3d")]
public class CheckInterruptGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool CheckSourceOfInterrupt;

	[ShowIf("CheckSourceOfInterrupt")]
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		if (base.CurrentEntity.Initiative.InterruptingOrder <= 0 && ContextData<InterruptTurnData>.Current?.Unit != base.CurrentEntity)
		{
			return false;
		}
		if (CheckSourceOfInterrupt)
		{
			return ContextData<InterruptTurnData>.Current?.Source == this.GetTargetByType(Target);
		}
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!CheckSourceOfInterrupt)
		{
			return "CurrentEntity's turn is an interrupt";
		}
		return $"CurrentEntity's turn is an interrupt from {Target}";
	}
}
