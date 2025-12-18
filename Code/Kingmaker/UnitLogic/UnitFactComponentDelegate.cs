using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

namespace Kingmaker.UnitLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
public abstract class UnitFactComponentDelegate : EntityFactComponentDelegate<BaseUnitEntity>
{
	protected ITargetWrapper OwnerTargetWrapper => base.Owner.ToITargetWrapper();

	protected new UnitFact Fact
	{
		[return: NotNull]
		get
		{
			return (base.Fact as UnitFact) ?? throw new Exception($"Component on invalid fact: {base.Fact.Blueprint}.{GetType().Name}");
		}
	}

	protected FeatureParam Param => base.Runtime.Param;
}
