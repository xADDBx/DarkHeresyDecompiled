using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Gameplay.Utility;

[Serializable]
public class BodyPartsSelector
{
	public enum FilterType
	{
		All,
		Tag,
		Specific,
		Random,
		RandomWithCritEffects,
		RandomWithTag
	}

	public FilterType Filter;

	[EnumFlagsAsButtons]
	[ShowIf("IsTag")]
	public BodyPartTags Tags;

	[ShowIf("IsSpecific")]
	public BpRef<BlueprintBodyPart> BodyPart;

	private bool IsTag
	{
		get
		{
			if (Filter != FilterType.Tag)
			{
				return Filter == FilterType.RandomWithTag;
			}
			return true;
		}
	}

	private bool IsSpecific => Filter == FilterType.Specific;

	public IEnumerable<BlueprintBodyPart> GetBodyParts(MechanicEntity entity)
	{
		return Filter switch
		{
			FilterType.All => entity.BodyParts, 
			FilterType.Tag => entity.BodyParts.Where((BlueprintBodyPart i) => i.Tags.HasAnyFlag(Tags)), 
			FilterType.RandomWithTag => new BlueprintBodyPart[1] { entity.BodyParts.Where((BlueprintBodyPart i) => i.Tags.HasAnyFlag(Tags)).Random(PFStatefulRandom.Mechanics) }, 
			FilterType.Specific => entity.BodyParts.Where((BlueprintBodyPart i) => i == BodyPart.MaybeBlueprint), 
			FilterType.Random => new BlueprintBodyPart[1] { entity.BodyParts.Random(PFStatefulRandom.Mechanics) }, 
			FilterType.RandomWithCritEffects => new BlueprintBodyPart[1] { entity.BodyParts.Where(delegate(BlueprintBodyPart i)
			{
				PartHealth healthOptional = entity.GetHealthOptional();
				return healthOptional != null && healthOptional.GetCriticalStage(i) >= 1;
			}).Random(PFStatefulRandom.Mechanics) }, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public string GetDescription()
	{
		return Filter switch
		{
			FilterType.All => "all body parts", 
			FilterType.Tag => $"body parts with tag ({Tags})", 
			FilterType.Specific => $"{BodyPart}", 
			FilterType.Random => "random body part", 
			FilterType.RandomWithCritEffects => "1 random body part with crit effect", 
			FilterType.RandomWithTag => $"1 random body part with  tag ({Tags})", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
