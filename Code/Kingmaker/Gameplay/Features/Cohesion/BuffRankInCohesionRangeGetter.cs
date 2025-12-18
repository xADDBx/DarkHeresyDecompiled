using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[ComponentName("Cohesion/BuffRankInCohesionRangeGetter")]
[TypeId("f28d4ba8a78a422bbd06f55b0b8dc75b")]
public sealed class BuffRankInCohesionRangeGetter : IntPropertyGetter
{
	public enum BuffRankCountRule
	{
		Sum,
		Max,
		Min
	}

	[ValidateNotNull]
	public BpRef<BlueprintBuff> Buff;

	public TargetType TargetFilter;

	public BuffRankCountRule CountRule;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = TargetFilter switch
		{
			TargetType.Enemy => "enemies", 
			TargetType.Ally => "allies", 
			TargetType.Any => "all units", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		string text2 = CountRule switch
		{
			BuffRankCountRule.Sum => "sum of ranks", 
			BuffRankCountRule.Max => "max rank", 
			BuffRankCountRule.Min => "min rank", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		return $"Count {text2} of {Buff} on {text} in {FormulaTargetScope.Current}'s cohesion range ";
	}

	protected override int GetBaseValue()
	{
		PartCohesion optional = base.CurrentEntity.GetOptional<PartCohesion>();
		if (optional == null)
		{
			return 0;
		}
		int num = 0;
		foreach (UnitEntity item in optional.UnitsInRange)
		{
			if (TargetFilter switch
			{
				TargetType.Enemy => base.CurrentEntity.IsEnemy(item), 
				TargetType.Ally => base.CurrentEntity.IsAlly(item), 
				TargetType.Any => true, 
				_ => throw new ArgumentOutOfRangeException(), 
			})
			{
				int rank = item.Buffs.GetRank(Buff);
				num = CountRule switch
				{
					BuffRankCountRule.Sum => num + rank, 
					BuffRankCountRule.Max => Math.Max(num, rank), 
					BuffRankCountRule.Min => Math.Min(num, rank), 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
		}
		return num;
	}
}
