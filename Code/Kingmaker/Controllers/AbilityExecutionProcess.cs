using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Controllers;

public class AbilityExecutionProcess
{
	public class Scope : SimpleContextData<AbilityExecutionProcess, Scope>
	{
	}

	private readonly IEnumerator<object> m_Process;

	private bool m_InstantDeliver;

	public AbilityExecutionContext Context { get; }

	public bool IsEnded { get; private set; }

	public bool IsEngageUnit => Context.AbilityBlueprint.GetComponent<AbilityDeliverEffect>()?.IsEngageUnit ?? false;

	public AbilityExecutionProcess(AbilityExecutionContext context)
	{
		Context = context;
		m_Process = ProcessRoutine();
	}

	public void Tick()
	{
		TimeSpan gameTime = Game.Instance.Player.GameTime;
		if (IsEnded)
		{
			Game.Instance.Player.GameTime = gameTime;
			PFLog.Default.Error("Ability already ended");
			return;
		}
		try
		{
			using (ContextData<GameLogDisabled>.RequestIf(Context.DisableLog))
			{
				using (SimpleContextData<AbilityExecutionProcess, Scope>.Set(this))
				{
					IsEnded = !m_Process.MoveNext();
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			IsEnded = true;
		}
		finally
		{
			Game.Instance.Player.GameTime = gameTime;
		}
		if (IsEnded)
		{
			Context.AbilityBlueprint.CallComponents(delegate(AbilityCustomLogic c)
			{
				c.Cleanup(Context);
			});
		}
	}

	private IEnumerator<object> ProcessRoutine()
	{
		OnStartProcess();
		AbilityDeliverEffect component = Context.Ability.Blueprint.GetComponent<AbilityDeliverEffect>();
		AbilitySelectTarget component2 = Context.Ability.Blueprint.GetComponent<AbilitySelectTarget>();
		AbilityApplyEffect[] applyEffectComponents = Context.Ability.Blueprint.GetComponents<AbilityApplyEffect>().ToArray();
		IEnumerable<AbilityHaloEffect> components = Context.Ability.Blueprint.GetComponents<AbilityHaloEffect>();
		IEnumerable<AbilityAdditionalTargets> components2 = Context.Ability.Blueprint.GetComponents<AbilityAdditionalTargets>();
		PrepareCast(Context);
		SpawnFxs(Context, AbilitySpawnFxTime.OnStart);
		IEnumerator deliverAndApplyEffect = DeliverAndApplyEffect(component, component2, applyEffectComponents, components, components2).GetEnumerator();
		try
		{
			bool hasNext;
			do
			{
				using (Context.SetScope())
				{
					hasNext = deliverAndApplyEffect.MoveNext();
				}
				if (hasNext)
				{
					yield return null;
				}
			}
			while (hasNext);
		}
		finally
		{
			IDisposable disposable = deliverAndApplyEffect as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		OnEndProcess();
	}

	private static void PrepareCast(AbilityExecutionContext context)
	{
		using (context.SetScope())
		{
			context.Recalculate();
			context.AbilityBlueprint.CallComponents(delegate(IAbilityOnCastLogic c)
			{
				c.OnCast(context);
			});
		}
	}

	private void OnStartProcess()
	{
		using (Context.SetScope())
		{
			Game.Instance.GetController<PsychicPhenomenaController>()?.TryTriggerPsychicPhenomenaBeforeCast(Context);
			EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
			{
				h.HandleExecutionProcessStart(Context);
			}, isCheckRuntime: true);
		}
	}

	private void OnEndProcess()
	{
		using (Context.SetScope())
		{
			Game.Instance.GetController<PsychicPhenomenaController>()?.TryTriggerPsychicPhenomenaAfterCast(Context);
			EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
			{
				h.HandleExecutionProcessEnd(Context);
			}, isCheckRuntime: true);
		}
	}

	private IEnumerable DeliverAndApplyEffect(AbilityDeliverEffect deliverEffect, AbilitySelectTarget selectTargets, AbilityApplyEffect[] applyEffectComponents, IEnumerable<AbilityHaloEffect> haloEffectComponents, IEnumerable<AbilityAdditionalTargets> additionalTargetsComponents)
	{
		if (deliverEffect != null && !m_InstantDeliver)
		{
			IEnumerator<AbilityDeliveryTarget> deliverProcess = deliverEffect.Deliver(Context, Context.ClickedTarget);
			while (TickDeliveryProcess(deliverProcess, Context, selectTargets, applyEffectComponents))
			{
				yield return null;
			}
		}
		else
		{
			AbilityDeliveryTarget deliveryTarget = new AbilityDeliveryTarget(Context.ClickedTarget);
			ApplyEffect(Context, deliveryTarget, applyEffectComponents, selectTargets, m_InstantDeliver);
		}
		ApplyHaloEffect(haloEffectComponents);
		ApplyEffectToAdditionalTargets(additionalTargetsComponents, applyEffectComponents, selectTargets);
	}

	private void ApplyHaloEffect(IEnumerable<AbilityHaloEffect> haloEffectComponents)
	{
		foreach (AbilityHaloEffect haloEffectComponent in haloEffectComponents)
		{
			haloEffectComponent.Apply(Context);
		}
		Context.Ability.GetPatternSettings()?.OverrideHaloSize(null);
	}

	private void ApplyEffectToAdditionalTargets(IEnumerable<AbilityAdditionalTargets> additionalTargetsComponents, AbilityApplyEffect[] applyEffectComponents, AbilitySelectTarget selectTargets)
	{
		foreach (AbilityDeliveryTarget item in additionalTargetsComponents.SelectMany((AbilityAdditionalTargets i) => i.GetTargets(Context)))
		{
			ApplyEffect(Context, item, applyEffectComponents, selectTargets, m_InstantDeliver);
		}
	}

	private static bool TickDeliveryProcess(IEnumerator<AbilityDeliveryTarget> deliveryProcess, AbilityExecutionContext context, [CanBeNull] AbilitySelectTarget selectTargets, [CanBeNull] IEnumerable<AbilityApplyEffect> effects)
	{
		try
		{
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(context.HitPolicy))
			{
				using (ContextData<DamagePolicyContextData>.Request().Setup(context.DamagePolicy))
				{
					bool flag;
					while (true)
					{
						TimeSpan? delayBetweenActions = context.DelayBetweenActions;
						if (delayBetweenActions.HasValue)
						{
							double totalSeconds = delayBetweenActions.GetValueOrDefault().TotalSeconds;
							if (totalSeconds >= 0.001)
							{
								double totalSeconds2 = (Game.Instance.Controllers.TimeController.GameTime - context.CastTime).TotalSeconds;
								int num = Math.Clamp(max: context.Ability.ActionsCount, value: (int)(totalSeconds2 / totalSeconds) + 1, min: 1);
								while (context.ActionIndex < num)
								{
									context.NextAction();
								}
							}
						}
						using (ProfileScope.New("TickDelivery"))
						{
							flag = deliveryProcess.MoveNext();
						}
						if (!flag || deliveryProcess.Current == null)
						{
							break;
						}
						using (ProfileScope.New("ApplyEffect"))
						{
							ApplyEffect(context, deliveryProcess.Current, effects, selectTargets, instant: false);
						}
					}
					return flag;
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
	}

	private static void SpawnFxs([NotNull] AbilityExecutionContext context, AbilitySpawnFxTime time, [CanBeNull] TargetWrapper selectedTarget = null)
	{
		foreach (AbilitySpawnFx fxSpawner in context.FxSpawners)
		{
			if (fxSpawner.Time == time)
			{
				fxSpawner.Spawn(context, selectedTarget);
			}
		}
	}

	private static void ApplyEffect(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffect, [CanBeNull] AbilitySelectTarget selectTargets, bool instant)
	{
		if (context.Ability.Blueprint.GetComponent<AbilityEffectMissIsHit>() != null || deliveryTarget.AttackRule == null || deliveryTarget.AttackRule.ResultIsHit)
		{
			ApplyEffectHit(context, deliveryTarget, applyEffect, selectTargets, instant);
		}
	}

	private static void ApplyEffectHit(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffects, [CanBeNull] AbilitySelectTarget selectTargets, bool instant)
	{
		if (selectTargets != null)
		{
			List<TargetWrapper> list;
			using (selectTargets.Select(context, deliveryTarget.Target).ToPooledList(out list))
			{
				context.TargetsInPatternCount = list.Count;
				foreach (TargetWrapper item in list)
				{
					AbilityDeliveryTarget target = new AbilityDeliveryTarget(item, deliveryTarget);
					DoApplyEffects(context, target, applyEffects);
				}
			}
		}
		else
		{
			DoApplyEffects(context, deliveryTarget, applyEffects);
		}
		SpawnFxs(context, AbilitySpawnFxTime.OnApplyEffect);
		EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
		{
			h.OnAbilityEffectApplied(context);
		});
	}

	private static void DoApplyEffects(AbilityExecutionContext context, AbilityDeliveryTarget target, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffects)
	{
		if (applyEffects.Empty())
		{
			DoApplyEffect(context, target, null);
			return;
		}
		foreach (AbilityApplyEffect applyEffect in applyEffects)
		{
			DoApplyEffect(context, target, applyEffect);
		}
	}

	private static void DoApplyEffect(AbilityExecutionContext context, AbilityDeliveryTarget target, [CanBeNull] AbilityApplyEffect applyEffect)
	{
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error(context.AbilityBlueprint, "Caster is missing");
			return;
		}
		MechanicEntity entity = target.Target.Entity;
		if (entity != null)
		{
			PartAbilityActionsImmunity optional = entity.GetOptional<PartAbilityActionsImmunity>();
			if (optional != null && optional.IsImmune(context))
			{
				return;
			}
		}
		using (ContextData<DamagePolicyContextData>.Request().Setup(context.DamagePolicy))
		{
			using (AbilityExecutionContext.GetAbilityDataScope(target.AttackRule, target.Projectile))
			{
				using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(target.Target))
				{
					EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
					{
						h.OnTryToApplyAbilityEffect(context, target);
					});
					applyEffect?.Apply(context, target.Target);
					SpawnFxs(context, AbilitySpawnFxTime.OnApplyEffect, target.Target);
					EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
					{
						h.OnAbilityEffectAppliedToTarget(context, target);
					});
				}
			}
		}
	}

	public void InstantDeliver()
	{
		m_InstantDeliver = true;
	}

	public void Detach()
	{
		Game.Instance.Controllers.AbilityExecutor.Detach(this);
	}
}
