using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Utility;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("25fd7dfb97f94df087a084c18a77ba0b")]
public class HasCriticalGetter : IntPropertyGetter
{
	public BodyPartsSelector BodyParts;

	public StackMath StackMath;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return StackMath switch
		{
			StackMath.Min => "Minimal stage", 
			StackMath.Max => "Maximal stage", 
			StackMath.Sum => "Sum of all stages", 
			_ => throw new ArgumentOutOfRangeException(), 
		} + " of critical effects on " + BodyParts.GetDescription();
	}

	protected override int GetBaseValue()
	{
		IEnumerable<BlueprintBodyPart> bodyParts = BodyParts.GetBodyParts(base.CurrentEntity);
		PartHealth health = base.CurrentEntity.GetHealthOptional();
		if (bodyParts == null || health == null)
		{
			return 0;
		}
		return StackMath switch
		{
			StackMath.Min => bodyParts.Min((BlueprintBodyPart p) => health.GetCriticalStage(p)), 
			StackMath.Max => bodyParts.Max((BlueprintBodyPart p) => health.GetCriticalStage(p)), 
			StackMath.Sum => bodyParts.Sum((BlueprintBodyPart p) => health.GetCriticalStage(p)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
