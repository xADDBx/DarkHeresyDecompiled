using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.AI.Mechanics.BodyParts;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/BodyPart/AiBodyPartCheckCanBreakTargetConcentrationIfHitGetter")]
[TypeId("1ec18649ac234f5ca5f9b35bec4d01f2")]
public class AiBodyPartCheckCanBreakTargetConcentrationIfHitGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		BlueprintBodyPart currentBodyPart = AiBodyPartsContextData.CurrentBodyPart;
		if (targetByType.BodyParts.Contains(currentBodyPart))
		{
			return currentBodyPart.CanBreakTargetConcentrationIfHit(targetByType);
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Hit <color='purple'>body part</color> can break " + Target.Colorized() + "'s concentration";
	}
}
