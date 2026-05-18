using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Controllers;

public class AbilityExecutionProcess
{
	private static readonly AbilityExecutionHandler _DefaultHandler = new AbilityExecutionHandler();

	private readonly IEnumerator<object> m_Process;

	private readonly IAbilityPipelineHandler m_Handler;

	private bool m_InstantDeliver;

	public AbilityExecutionContext Context { get; }

	public bool IsEnded { get; private set; }

	public bool IsEngageUnit => Context.AbilityBlueprint.GetComponent<AbilityDeliverEffect>()?.IsEngageUnit ?? false;

	public AbilityExecutionProcess(AbilityExecutionContext context)
		: this(context, _DefaultHandler)
	{
	}

	public AbilityExecutionProcess(AbilityExecutionContext context, IAbilityPipelineHandler handler)
	{
		Context = context;
		m_Handler = handler;
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
				IsEnded = !m_Process.MoveNext();
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
			m_Handler.OnProcessCleanup(Context);
		}
	}

	private IEnumerator<object> ProcessRoutine()
	{
		AbilityDeliverEffect component = Context.Ability.Blueprint.GetComponent<AbilityDeliverEffect>();
		AbilitySelectTarget component2 = Context.Ability.Blueprint.GetComponent<AbilitySelectTarget>();
		AbilityApplyEffect[] applyEffectComponents = Context.Ability.Blueprint.GetComponents<AbilityApplyEffect>().ToArray();
		IEnumerable<AbilityHaloEffect> components = Context.Ability.Blueprint.GetComponents<AbilityHaloEffect>();
		IEnumerable<AbilityAdditionalTargets> components2 = Context.Ability.Blueprint.GetComponents<AbilityAdditionalTargets>();
		using (EvalContext.PushContext(Context))
		{
			m_Handler.OnProcessStart(Context);
			m_Handler.PrepareCast(Context);
			m_Handler.SpawnFxs(Context, AbilitySpawnFxTime.OnStart);
		}
		IEnumerator deliverAndApplyEffect = DeliverAndApplyEffect(component, component2, applyEffectComponents, components, components2).GetEnumerator();
		try
		{
			bool hasNext;
			do
			{
				using (EvalContext.PushContext(Context))
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
		using (EvalContext.PushContext(Context))
		{
			m_Handler.OnProcessEnd(Context);
		}
	}

	private IEnumerable DeliverAndApplyEffect(AbilityDeliverEffect deliverEffect, AbilitySelectTarget selectTargets, AbilityApplyEffect[] applyEffectComponents, IEnumerable<AbilityHaloEffect> haloEffectComponents, IEnumerable<AbilityAdditionalTargets> additionalTargetsComponents)
	{
		if (deliverEffect != null && !m_InstantDeliver)
		{
			IEnumerator<AbilityDeliveryTarget> deliverProcess = m_Handler.DeliverTargets(deliverEffect, Context, Context.ClickedTarget);
			while (TickDeliveryProcess(deliverProcess, selectTargets, applyEffectComponents))
			{
				yield return null;
			}
		}
		else
		{
			AbilityDeliveryTarget deliveryTarget = new AbilityDeliveryTarget(Context.ClickedTarget);
			ApplyEffect(deliveryTarget, applyEffectComponents, selectTargets, m_InstantDeliver);
		}
		ApplyHaloEffect(haloEffectComponents);
		ApplyEffectToAdditionalTargets(additionalTargetsComponents, applyEffectComponents, selectTargets);
	}

	private void ApplyHaloEffect(IEnumerable<AbilityHaloEffect> haloEffectComponents)
	{
		foreach (AbilityHaloEffect haloEffectComponent in haloEffectComponents)
		{
			m_Handler.InvokeHaloEffect(haloEffectComponent, Context);
		}
		Context.Ability.GetPatternSettings()?.OverrideHaloSize(null);
	}

	private void ApplyEffectToAdditionalTargets(IEnumerable<AbilityAdditionalTargets> additionalTargetsComponents, AbilityApplyEffect[] applyEffectComponents, AbilitySelectTarget selectTargets)
	{
		foreach (AbilityAdditionalTargets additionalTargetsComponent in additionalTargetsComponents)
		{
			foreach (AbilityDeliveryTarget additionalTarget in m_Handler.GetAdditionalTargets(additionalTargetsComponent, Context))
			{
				ApplyEffect(additionalTarget, applyEffectComponents, selectTargets, m_InstantDeliver);
			}
		}
	}

	private bool TickDeliveryProcess(IEnumerator<AbilityDeliveryTarget> deliveryProcess, [CanBeNull] AbilitySelectTarget selectTargets, [CanBeNull] IEnumerable<AbilityApplyEffect> effects)
	{
		try
		{
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(Context.HitPolicy))
			{
				using (ContextData<DamagePolicyContextData>.Request().Setup(Context.DamagePolicy))
				{
					bool flag;
					while (true)
					{
						TimeSpan? delayBetweenActions = Context.DelayBetweenActions;
						if (delayBetweenActions.HasValue)
						{
							double totalSeconds = delayBetweenActions.GetValueOrDefault().TotalSeconds;
							if (totalSeconds >= 0.001)
							{
								double totalSeconds2 = (Game.Instance.Controllers.TimeController.GameTime - Context.CastTime).TotalSeconds;
								int num = Math.Clamp(max: Context.Ability.ActionsCount, value: (int)(totalSeconds2 / totalSeconds) + 1, min: 1);
								while (Context.ActionIndex < num)
								{
									Context.NextAction();
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
							ApplyEffect(deliveryProcess.Current, effects, selectTargets, instant: false);
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

	private void ApplyEffect(AbilityDeliveryTarget deliveryTarget, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffect, [CanBeNull] AbilitySelectTarget selectTargets, bool instant)
	{
		if (m_Handler.ShouldApplyToDeliveryTarget(Context, deliveryTarget))
		{
			ApplyEffectHit(deliveryTarget, applyEffect, selectTargets, instant);
		}
	}

	private void ApplyEffectHit(AbilityDeliveryTarget deliveryTarget, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffects, [CanBeNull] AbilitySelectTarget selectTargets, bool instant)
	{
		if (selectTargets != null)
		{
			List<TargetWrapper> list;
			using (selectTargets.Select(Context, deliveryTarget.Target).ToPooledList(out list))
			{
				Context.TargetsInPatternCount = list.Count;
				foreach (TargetWrapper item in list)
				{
					AbilityDeliveryTarget target = new AbilityDeliveryTarget(item, deliveryTarget);
					DoApplyEffects(target, applyEffects);
				}
			}
		}
		else
		{
			DoApplyEffects(deliveryTarget, applyEffects);
		}
		m_Handler.OnEffectApplied(Context);
	}

	private void DoApplyEffects(AbilityDeliveryTarget target, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffects)
	{
		if (applyEffects.Empty())
		{
			DoApplyEffect(target, null);
			return;
		}
		foreach (AbilityApplyEffect applyEffect in applyEffects)
		{
			DoApplyEffect(target, applyEffect);
		}
	}

	private void DoApplyEffect(AbilityDeliveryTarget target, [CanBeNull] AbilityApplyEffect applyEffect)
	{
		if (Context.MaybeCaster == null)
		{
			PFLog.Default.Error(Context.AbilityBlueprint, "Caster is missing");
		}
		else
		{
			if (m_Handler.IsImmune(Context, target))
			{
				return;
			}
			using (ContextData<DamagePolicyContextData>.Request().Setup(Context.DamagePolicy))
			{
				using (AbilityExecutionContext.GetAbilityDataScope(target.AttackRule, target.Projectile))
				{
					using (EvalContext.Current.PushTarget(target.Target))
					{
						m_Handler.OnTryToApplyToTarget(Context, target);
						m_Handler.InvokeApplyEffect(applyEffect, Context, target.Target);
						m_Handler.OnEffectAppliedToTarget(Context, target);
					}
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
