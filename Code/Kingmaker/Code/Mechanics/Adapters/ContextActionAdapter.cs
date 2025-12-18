using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("d09a01fc2701473183eda655aace4e9f")]
public class ContextActionAdapter : GameAction
{
	[SerializeReference]
	[CanBeNull]
	public MechanicEntityEvaluator Caster;

	[SerializeReference]
	[CanBeNull]
	public MechanicEntityEvaluator TargetEntity;

	[SerializeReference]
	[CanBeNull]
	public PositionEvaluator TargetPosition;

	public ActionList Actions;

	public override string GetCaption()
	{
		return $"Setup context: Caster [{Caster}]; Target [{TargetEntity}, {TargetPosition}]";
	}

	protected override void RunAction()
	{
		BlueprintScriptableObject blueprint = (base.Owner as BlueprintScriptableObject) ?? throw new Exception("Valid Blueprint is missing");
		MechanicEntity mechanicEntity = Caster?.GetValue() ?? SimpleCaster.GetFree();
		MechanicEntity mechanicEntity2 = TargetEntity?.GetValue();
		Vector3? vector = TargetPosition?.GetValue();
		if (mechanicEntity == null)
		{
			throw new Exception("Caster is missing");
		}
		if (mechanicEntity2 == null && !vector.HasValue)
		{
			throw new Exception("Target is missing");
		}
		TargetWrapper targetWrapper = ((!vector.HasValue) ? new TargetWrapper(mechanicEntity2) : ((mechanicEntity2 == null) ? new TargetWrapper(vector.Value) : new TargetWrapper(vector.Value, null, mechanicEntity2)));
		using MechanicsContext mechanicsContext = MechanicsContext.Claim(blueprint, mechanicEntity, mechanicEntity, null, targetWrapper);
		using (mechanicsContext.SetScope(targetWrapper))
		{
			Actions.Run();
		}
	}
}
