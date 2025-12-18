using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("f48cb5fd68404437b868e072a7ba6d37")]
public class AbilitySimpleUnitsInPatternGetter : IntPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IRule
{
	public bool IncludeCaster;

	public bool OnlyEnemy;

	protected override int GetBaseValue()
	{
		int num = 0;
		MechanicsContext mechanicContext = this.GetMechanicContext();
		foreach (GridNodeBase item in mechanicContext.SourcePatternNodes ?? throw new Exception("Can't count targets in pattern of not casted ability"))
		{
			BaseUnitEntity firstUnit = item.GetFirstUnit();
			if (firstUnit != null && (!IncludeCaster || firstUnit != mechanicContext.SourceCaster) && (!OnlyEnemy || mechanicContext.SourceCaster == null || !firstUnit.IsAlly(mechanicContext.SourceCaster)))
			{
				num++;
			}
		}
		return num;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!OnlyEnemy)
		{
			return "Count of units in ability pattern";
		}
		return "Count of caster's enemies in ability pattern";
	}
}
