using System;
using System.Linq;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a31c541c19ff40d788ba5df947daa2ca")]
public class IsolatedGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IOptionalRule
{
	protected override bool GetBaseValue()
	{
		return GameHelper.GetTargetsAround(base.CurrentEntity.Position, 1).ToList().Any((BaseUnitEntity p) => p.IsAlly(base.CurrentEntity) && p != base.CurrentEntity);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "1 if " + FormulaTargetScope.Current + " has no allies around it, 0 if it has some";
	}
}
