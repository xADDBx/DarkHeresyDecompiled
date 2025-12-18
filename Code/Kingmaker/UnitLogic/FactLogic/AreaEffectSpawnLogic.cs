using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintAreaEffect))]
[TypeId("3e69f4b80be10d0489945af405b0d95f")]
public class AreaEffectSpawnLogic : BlueprintComponent
{
	public void HandleAreaEffectSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnAreaEffectSpawn(context, areaEffect);
	}

	protected virtual void OnAreaEffectSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}
}
