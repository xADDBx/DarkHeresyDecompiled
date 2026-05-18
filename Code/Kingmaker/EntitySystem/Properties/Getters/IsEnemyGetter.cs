using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("fc2e6424f1444ac2ada8eeb070e46488")]
public class IsEnemyGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		if (!(EvalContext.Current.GetEntityByType(Target) is BaseUnitEntity entity))
		{
			return false;
		}
		return base.CurrentEntity.IsEnemy(entity);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + FormulaTargetScope.Current + " enemy to " + Target.Colorized();
	}
}
