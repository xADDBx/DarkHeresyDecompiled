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
[ComponentName("AI/BodyPart/AiBodyPartCheckIsVitalGetter")]
[TypeId("084f9d2cf3304970a028751616ce6dd5")]
public class AiBodyPartCheckIsVitalGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		BlueprintBodyPart currentBodyPart = AiBodyPartsContextData.CurrentBodyPart;
		if (targetByType.BodyParts.Contains(currentBodyPart))
		{
			return currentBodyPart.IsVital;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + "'s <color='purple'>body part</color> is vital";
	}
}
