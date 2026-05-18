using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Area Effect/SpawnAreaEffectOnAbilityCast")]
[TypeId("0368d351dda74eafa920effae9c1998d")]
public class SpawnAreaEffectOnAbilityCast : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintAreaEffectReference m_AreaEffect = new BlueprintAreaEffectReference();

	[SerializeField]
	private ContextDurationValue m_DurationValue;

	[SerializeField]
	private bool m_OnUnit;

	[HideIf("m_OnUnit")]
	[SerializeField]
	[InspectorReadOnly]
	private bool m_GetOrientationFromCaster;

	[SerializeField]
	private bool m_ShowPredictionForMelee;

	[ShowIf("m_ShowPredictionForMelee")]
	[SerializeField]
	private bool m_NeedCurrentTargetForPrediction;

	public ActionList ActionsOnAllTargetsOnApply = new ActionList();

	public bool GetOrientationFromCaster => m_GetOrientationFromCaster;

	private BlueprintAreaEffect AreaEffect => m_AreaEffect.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (m_ShowPredictionForMelee)
		{
			base.Owner.GetOrCreate<PartAbilityPredictionForAreaEffect>().Add(this);
		}
	}

	protected override void OnDeactivate()
	{
		if (m_ShowPredictionForMelee)
		{
			base.Owner.GetOptional<PartAbilityPredictionForAreaEffect>()?.Remove(this);
		}
	}

	public BlueprintAreaEffect GetBlueprintAbilityAreaEffect(AbilityData ability)
	{
		if (!ability.IsMelee)
		{
			return null;
		}
		IEvalContext ctx;
		if (!m_NeedCurrentTargetForPrediction)
		{
			using (EvalContext.PushAbility(ability, base.Owner).Get(out ctx))
			{
				return m_Restrictions.IsPassed(ctx, base.Owner) ? AreaEffect : null;
			}
		}
		PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
		TargetWrapper target = Game.Instance.Controllers.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, ability, ability.Caster.Position);
		IEvalContext ctx2;
		if (target.Entity != null && target.Entity != ability.Caster && ability.Caster.DistanceToInCells(target.Entity) == 1)
		{
			using (EvalContext.PushAbility(ability, target).Get(out ctx2))
			{
				return m_Restrictions.IsPassed(ctx2, base.Owner) ? AreaEffect : null;
			}
		}
		return null;
	}

	private void TryToTrigger(RulePerformAbility evt)
	{
		if (!m_Restrictions.IsPassed(base.Context, null, null, evt) || (bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			return;
		}
		TimeSpan seconds = m_DurationValue.Calculate(base.Context).Seconds;
		AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(AreaEffect, base.Context, evt.AbilityTarget).Duration(seconds).OnUnit(m_OnUnit)
			.Spawn();
		if (areaEffectEntity == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || evt.Initiator.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(evt.Context, new AbilityDeliveryTarget(u));
				});
				if (ActionsOnAllTargetsOnApply.HasActions)
				{
					base.Fact.RunActionInContext(ActionsOnAllTargetsOnApply, u);
				}
			}
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		TryToTrigger(evt);
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
	}
}
