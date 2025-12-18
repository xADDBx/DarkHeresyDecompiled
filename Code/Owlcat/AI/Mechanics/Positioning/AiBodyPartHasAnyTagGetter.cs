using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.AI.Mechanics.BodyParts;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/BodyPart/AiBodyPartHasAnyTagGetter")]
[TypeId("92f0f212e2984fc380c46128389a397b")]
public class AiBodyPartHasAnyTagGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	[EnumFlagsAsButtons]
	public BodyPartTags Tags;

	protected override bool GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		BlueprintBodyPart currentBodyPart = AiBodyPartsContextData.CurrentBodyPart;
		if (targetByType.BodyParts.Contains(currentBodyPart))
		{
			return currentBodyPart.Tags.HasAnyFlag(Tags);
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{Target.Colorized()}'s <color='purple'>body part</color> has any tag of {Tags}";
	}
}
