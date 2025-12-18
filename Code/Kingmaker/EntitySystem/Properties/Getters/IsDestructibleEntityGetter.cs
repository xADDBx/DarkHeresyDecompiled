using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ca45e2fbf9ca3c6478bd4533289cf12d")]
public class IsDestructibleEntityGetter : BoolPropertyGetter
{
	protected override bool GetBaseValue()
	{
		return base.CurrentEntity is DestructibleEntity;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is destructible entity";
	}
}
