using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("81d1333a815e48c4baf215e1b7adf8d6")]
public class AddClassLevels : UnitFactComponentDelegate
{
	public class DoNotCreatePlan : ContextData<DoNotCreatePlan>
	{
		protected override void Reset()
		{
		}
	}
}
