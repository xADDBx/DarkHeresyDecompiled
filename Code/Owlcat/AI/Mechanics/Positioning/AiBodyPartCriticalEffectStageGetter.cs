using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;
using Owlcat.AI.Mechanics.BodyParts;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/BodyPart/AiBodyPartCriticalEffectStageGetter")]
[TypeId("cb63a64233bd412bbfb1ec714aba907d")]
public class AiBodyPartCriticalEffectStageGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		BlueprintBodyPart currentBodyPart = AiBodyPartsContextData.CurrentBodyPart;
		if (targetByType.BodyParts.Contains(currentBodyPart))
		{
			PartHealth healthOptional = targetByType.GetHealthOptional();
			if (healthOptional != null)
			{
				return healthOptional.GetCriticalStage(currentBodyPart);
			}
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Critical effect stage of " + Target.Colorized() + "'s <color='purple'>body part</color>";
	}
}
