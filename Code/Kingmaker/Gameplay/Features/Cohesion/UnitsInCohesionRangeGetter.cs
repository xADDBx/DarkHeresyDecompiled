using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[TypeId("9b774c8b3c77427eb3ffb576e69f3be9")]
public sealed class UnitsInCohesionRangeGetter : IntPropertyGetter
{
	public TargetType Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Type switch
		{
			TargetType.Enemy => "Enemies in cohesion range", 
			TargetType.Ally => "Allies in cohesion range", 
			TargetType.Any => "Units in cohesion range", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override int GetBaseValue()
	{
		PartCohesion optional = base.CurrentEntity.GetOptional<PartCohesion>();
		if (optional == null)
		{
			return 0;
		}
		return Type switch
		{
			TargetType.Enemy => optional.EnemiesInRangeCount, 
			TargetType.Ally => optional.AlliesInRangeCount, 
			TargetType.Any => optional.UnitsInRangeCount, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
