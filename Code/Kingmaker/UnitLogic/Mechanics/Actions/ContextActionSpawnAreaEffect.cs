using System;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("4e5ac5e97bccb29429a528734d2051b2")]
public class ContextActionSpawnAreaEffect : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("AreaEffect")]
	private BlueprintAreaEffectReference m_AreaEffect;

	public ContextDurationValue DurationValue;

	public bool OnUnit;

	[Tooltip("Set FactData ContextData as SourceFact")]
	public bool SetSourceFact;

	public bool ForcedInitiative;

	[ShowIf("ForcedInitiative")]
	public AreaEffectForcedInitiative ForcedInitiativeType;

	public BlueprintAreaEffect AreaEffect => m_AreaEffect?.Get();

	public override string GetCaption()
	{
		string arg = ((AreaEffect != null) ? AreaEffect.ToString() : "<undefined>");
		return $"Spawn {arg} for {DurationValue}";
	}

	protected override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			return;
		}
		if (OnUnit && base.Target.Entity == null)
		{
			throw new InvalidOperationException("Target unit is missing");
		}
		TargetWrapper target = base.Target;
		if (!OnUnit)
		{
			Vector3 vector = base.Target.Entity?.Center ?? base.Target.Point;
			Vector3 normalized = (vector - base.Caster.Center).normalized;
			float value = ((normalized.sqrMagnitude > 0.001f) ? Quaternion.LookRotation(normalized).eulerAngles.y : base.Caster.Orientation);
			target = new TargetWrapper(((base.Target.Entity != null) ? base.Target.Entity.GetInnerNodeNearestToTarget(base.Caster.Center) : vector.GetNearestNodeXZUnwalkable()).Vector3Position(), value, base.Target.Entity);
		}
		TimeSpan seconds = DurationValue.Calculate(base.Context).Seconds;
		AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(AreaEffect, base.Context, target).Duration(seconds).OnUnit(OnUnit)
			.Initiative(ForcedInitiative ? new float?(ForcedInitiativeType.GetInitiativeValue()) : null)
			.Spawn();
		if (SetSourceFact && areaEffectEntity != null)
		{
			MechanicEntityFact mechanicEntityFact = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.Fact;
			if (mechanicEntityFact is UnitFact unitFact)
			{
				EntityFact sourceFact = mechanicEntityFact.SourceFact;
				if (sourceFact != null && sourceFact.Owner != null)
				{
					areaEffectEntity.SourceFact = new EntityFactRef(unitFact.SourceFact);
				}
			}
		}
		if (areaEffectEntity == null || base.AbilityContext == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || !base.AbilityContext.Caster.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(base.AbilityContext, new AbilityDeliveryTarget(u));
				});
			}
		}
	}
}
