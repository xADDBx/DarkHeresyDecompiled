using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a0f6aa422d1512e46833e2365baffae4")]
public class SuicideAttacker : BlueprintComponent
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public BaseUnitEntity UnitOnFinishPosition;
	}

	public ActionList ActionsOnTarget;

	public ActionList ActionOnSelf;
}
