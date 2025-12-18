using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("40de30c1797905949ae8903538ea4213")]
public class EntityIsAreaSourceGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalCurrentTargetEntity, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "CurrentTarget is AreaEffectEntity and " + FormulaTargetScope.Current + " is it's source";
	}

	protected override int GetBaseValue()
	{
		if (!(this.GetCurrentTarget() is AreaEffectEntity { SourceFact: { Entity: var entity } }))
		{
			return 0;
		}
		if (entity == null)
		{
			return 0;
		}
		if (entity != base.CurrentEntity)
		{
			return 0;
		}
		return 1;
	}
}
