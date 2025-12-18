using System;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("3d234e0e901c4c92ba8de2707426007e")]
public sealed class CheckAttackHitBodyPartGetter : BoolPropertyGetter
{
	private enum FilterType
	{
		Tags,
		Specific
	}

	[SerializeField]
	private FilterType _filter;

	public BodyPartTags Tags;

	public BpRef<BlueprintBodyPart> Specific;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check attack hit " + GetFilterDescription();
	}

	protected override bool GetBaseValue()
	{
		BlueprintBodyPart blueprintBodyPart = Rulebook.Instance.Context.LastEventOfType<RulePerformAttack>()?.ResultHitLocation;
		bool flag = blueprintBodyPart != null;
		if (flag)
		{
			flag = _filter switch
			{
				FilterType.Tags => blueprintBodyPart.Tags.HasAnyFlag(Tags), 
				FilterType.Specific => blueprintBodyPart == Specific.MaybeBlueprint, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		return flag;
	}

	private string GetFilterDescription()
	{
		return _filter switch
		{
			FilterType.Tags => $"body part with tag ({Tags})", 
			FilterType.Specific => Specific.ToString(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
