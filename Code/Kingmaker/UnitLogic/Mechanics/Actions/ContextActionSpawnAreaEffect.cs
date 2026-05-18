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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
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

	[InfoBox("По умолчанию Area Effect поворачивается по направлению от кастера к таргету (или по направлению взгляда кастера, если эффект спавнится в клетку кастера).")]
	public bool DontGetOrientationFromCaster;

	public bool ForcedInitiative;

	[ShowIf("ForcedInitiative")]
	public AreaEffectForcedInitiative ForcedInitiativeType;

	public BlueprintAreaEffect AreaEffect => m_AreaEffect?.Get();

	public bool GetOrientationFromCaster => !DontGetOrientationFromCaster;

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
			float value = ((!GetOrientationFromCaster) ? 0f : ((normalized.sqrMagnitude > 0.001f) ? Quaternion.LookRotation(normalized).eulerAngles.y : base.Caster.Orientation));
			target = new TargetWrapper(((base.Target.Entity != null) ? base.Target.Entity.GetInnerNodeNearestToTarget(base.Caster.Center) : vector.GetNearestNodeXZUnwalkable()).Vector3Position(), value, base.Target.Entity);
		}
		TimeSpan seconds = DurationValue.Calculate(base.Context).Seconds;
		AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(AreaEffect, base.Context, target).Duration(seconds).OnUnit(OnUnit)
			.Initiative(ForcedInitiative ? new float?(ForcedInitiativeType.GetInitiativeValue()) : null)
			.Spawn();
		if (SetSourceFact && areaEffectEntity != null)
		{
			MechanicEntityFact fact = base.Context.Fact;
			if (fact is UnitFact unitFact)
			{
				EntityFact sourceFact = fact.SourceFact;
				if (sourceFact != null && sourceFact.Owner != null)
				{
					areaEffectEntity.SourceFact = new EntityFactRef(unitFact.SourceFact);
				}
			}
		}
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (areaEffectEntity == null || abilityContext == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || !abilityContext.Caster.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(abilityContext, new AbilityDeliveryTarget(u));
				});
			}
		}
	}
}
