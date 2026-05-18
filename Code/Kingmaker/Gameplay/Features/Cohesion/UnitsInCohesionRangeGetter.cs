using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[TypeId("9b774c8b3c77427eb3ffb576e69f3be9")]
public sealed class UnitsInCohesionRangeGetter : IntPropertyGetter
{
	public TargetType Type;

	[SerializeField]
	private PropertyCalculator m_Value = new PropertyCalculator();

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
		if (m_Value.Empty)
		{
			return Type switch
			{
				TargetType.Enemy => optional.EnemiesInRangeCount, 
				TargetType.Ally => optional.AlliesInRangeCount, 
				TargetType.Any => optional.UnitsInRangeCount, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		return Type switch
		{
			TargetType.Enemy => optional.UnitsInRange.Count((UnitEntity u) => base.CurrentEntity.IsEnemy(u) && m_Value.GetBoolValue(u)), 
			TargetType.Ally => optional.UnitsInRange.Count((UnitEntity u) => base.CurrentEntity.IsAlly(u) && m_Value.GetBoolValue(u)), 
			TargetType.Any => optional.UnitsInRange.Count((UnitEntity u) => m_Value.GetBoolValue(u)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
