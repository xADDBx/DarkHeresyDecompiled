using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.FX;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Enums.Sound;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Visual.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Controllers;

public class VisualEffectsController : IController, IAnimationEventHandler, ISubscriber<IMechanicEntity>, ISubscriber, IAbilityExecutionProcessHandler, IApplyAbilityEffectHandler, IProjectileLaunchedHandler, IProjectileHitHandler, IDamageFXHandler, IWarhammerAttackHandler, IAreaEffectHandler, ISubscriber<IAreaEffectEntity>, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IGlobalRulebookSubscriber, IBuffEffectHandler, IUnitCommandStartHandler
{
	private static void TryPlayVisualFX([NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [CanBeNull] AbilityData ability, MappedAnimationEventType? animationEvent, AbilityEventType? abilityEvent)
	{
		if (ability == null)
		{
			ability = (caster.GetCommandsOptional()?.Current as UnitUseAbility)?.Ability;
		}
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = ability?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings != null)
		{
			if (animationEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, animationEvent.Value, ability);
			}
			if (abilityEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, target ?? ((TargetWrapper)caster), abilityEvent.Value, ability);
			}
		}
	}

	private static void TryPlaySoundFX([NotNull] AbilityExecutionContext context, [CanBeNull] TargetWrapper target, [CanBeNull] AbilityData ability, AbilityEventType abilityEvent)
	{
		if (ability == null)
		{
			ability = context.Caster.GetCommandsOptional()?.GetCurrent<UnitUseAbility>()?.Ability;
		}
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = ability?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, context, abilityEvent);
		}
	}

	private static void TryPlaySoundFX([NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, [CanBeNull] AbilityData ability, AbilityEventType abilityEvent)
	{
		if (ability == null)
		{
			ability = caster.GetCommandsOptional()?.GetCurrent<UnitUseAbility>()?.Ability;
		}
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = ability?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, caster, target, abilityEvent);
		}
	}

	private static GameObject[] TryPlayBuffFX([NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [NotNull] IBuff buff, MappedAnimationEventType? animationEvent, AbilityEventType? abilityEvent)
	{
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = buff?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings == null)
		{
			return Array.Empty<GameObject>();
		}
		if (animationEvent.HasValue)
		{
			FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, animationEvent.Value);
		}
		if (abilityEvent.HasValue)
		{
			return FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, target ?? ((TargetWrapper)caster), abilityEvent.Value);
		}
		return Array.Empty<GameObject>();
	}

	private static void TryPlaySoundFX([NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, [NotNull] IBuff buff, AbilityEventType buffEvent)
	{
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = buff?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, caster, target.Entity ?? caster, buffEvent);
		}
	}

	private static void TryPlayAreaEffectFX([NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [NotNull] AreaEffectEntity areaEffect, MappedAnimationEventType? animationEvent, AbilityEventType? abilityEvent)
	{
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = areaEffect?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings != null)
		{
			if (animationEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, animationEvent.Value);
			}
			if (abilityEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, target ?? ((TargetWrapper)caster), abilityEvent.Value);
			}
		}
	}

	private static void TryPlaySoundFX([NotNull] MechanicsContext context, [CanBeNull] TargetWrapper target, [NotNull] AreaEffectEntity areaEffect, AbilityEventType abilityEvent)
	{
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = areaEffect?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, context, abilityEvent);
		}
	}

	private static void TryPlayVisualFX([NotNull] MechanicEntity caster, MappedAnimationEventType eventType)
	{
		TryPlayVisualFX(caster, null, null, eventType, null);
	}

	private static void TryPlayVisualFX([NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, [NotNull] AbilityData ability, AbilityEventType eventType)
	{
		TryPlayVisualFX(caster, target, ability, null, eventType);
	}

	private static void TryPlayProjectileFX(Projectile projectile, TargetWrapper target, AbilityEventType eventType)
	{
		MechanicEntity entity = projectile.Launcher.Entity;
		AbilityData ability = projectile.Ability;
		if (entity != null && !(ability == null))
		{
			TryPlayVisualFX(entity, target, ability, eventType);
			TryPlaySoundFX(entity, target, ability, eventType);
			if (eventType == AbilityEventType.ProjectileHit)
			{
				HitFXPlayer.PlayProjectileHit(projectile, target);
			}
		}
	}

	private static void TryPlayDamageFx(RuleDealDamage rule)
	{
		HitFXPlayer.PlayDamageHit(rule);
	}

	private static void TryPlayAnimationHit(RuleDealDamage rule)
	{
		UnitAnimationManager maybeAnimationManager = rule.Target.MaybeAnimationManager;
		if (!(maybeAnimationManager == null))
		{
			UnitAnimationActionHandle unitAnimationActionHandle = maybeAnimationManager.CreateHandle(UnitAnimationType.Hit, errorOnEmpty: false);
			if (unitAnimationActionHandle != null)
			{
				unitAnimationActionHandle.HitDirection = ((rule.Target != null && rule.Initiator != null) ? (rule.Initiator.Position - rule.Target.Position) : Vector3.zero);
				unitAnimationActionHandle.Spell = rule.Reason.Context?.SourceAbilityBlueprint;
				maybeAnimationManager.Execute(unitAnimationActionHandle);
			}
		}
	}

	private static void TryPlayHitEffect(Projectile projectile, TargetWrapper target)
	{
		if (projectile.Hits.Length != 0 && !projectile.DoNotPlayHitEffect)
		{
			if (projectile.IsCoverHit)
			{
				SoundEventsManager.PostEvent(ConfigRoot.Instance.FxRoot.InCoverHitSoundEvent, projectile.ClosestHit.transform.gameObject);
			}
			Vector3 projectileHitFxSpawnPosition = HitFXPlayer.GetProjectileHitFxSpawnPosition(projectile, target);
			SphereBounds hitFxCullingSphere = new SphereBounds(projectileHitFxSpawnPosition, 0.5f);
			SurfaceHitController.Instance.ProcessProjectileHits(projectile, in hitFxCullingSphere);
		}
	}

	void IAnimationEventHandler.HandleAnimationEvent(MappedAnimationEventType eventType)
	{
		TryPlayVisualFX(EventInvokerExtensions.MechanicEntity, eventType);
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		TryPlayVisualFX(context.Caster, context.ClickedTarget, context.Ability, AbilityEventType.Start);
		TryPlaySoundFX(context, context.ClickedTarget, context.Ability, AbilityEventType.Start);
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		TryPlayVisualFX(context.Caster, context.ClickedTarget, context.Ability, AbilityEventType.End);
		TryPlaySoundFX(context, context.ClickedTarget, context.Ability, AbilityEventType.End);
	}

	void IApplyAbilityEffectHandler.OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		TryPlayVisualFX(context.Caster, target.Target, context.Ability, AbilityEventType.HitTarget);
		TryPlaySoundFX(context, target.Target, context.Ability, AbilityEventType.HitTarget);
	}

	void IApplyAbilityEffectHandler.OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	void IApplyAbilityEffectHandler.OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	void IProjectileLaunchedHandler.HandleProjectileLaunched(Projectile projectile)
	{
		TryPlayProjectileFX(projectile, projectile.Target, AbilityEventType.ProjectileLaunched);
	}

	void IProjectileHitHandler.HandleProjectileHit(Projectile projectile)
	{
		TryPlayHitEffect(projectile, projectile.Target);
		TryPlayProjectileFX(projectile, projectile.Target, AbilityEventType.ProjectileHit);
	}

	void IDamageFXHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		MechanicEntity target = dealDamage.Target;
		TryPlayDamageFx(dealDamage);
		MechanicEntity target2 = dealDamage.Target;
		UnitEntity unitEntity = target2 as UnitEntity;
		if (unitEntity != null)
		{
			if (target.View == null)
			{
				return;
			}
			bool flag = true;
			TryPlayAnimationHit(dealDamage);
			if (true)
			{
				FxHelper.SpawnFxOnEntity(ConfigRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkEffect.Load(), dealDamage.ConcreteTarget.View);
			}
			if (flag && (!(dealDamage.Reason.Fact is Buff buff) || !buff.Blueprint.PlayOnlyFirstHitSound || !buff.PlayedFirstHitSound))
			{
				AkUnitySoundEngine.SetSwitch("HitMainType", dealDamage.ResultIsCritical ? "Crit" : "Normal", dealDamage.ConcreteTarget.View.gameObject);
				AkSwitchReference akSwitchReference = ConfigRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry x) => x.Type == unitEntity.Blueprint.VisualSettings.SurfaceType)?.Switch;
				if (akSwitchReference.IsValid())
				{
					AkUnitySoundEngine.SetSwitch(akSwitchReference.Group, akSwitchReference.Value, unitEntity.View.gameObject);
				}
				if (dealDamage.Reason.Fact is Buff buff2)
				{
					buff2.PlayedFirstHitSound = true;
					AkSwitchReference soundTypeSwitch = buff2.Blueprint.SoundTypeSwitch;
					if (soundTypeSwitch.IsValid())
					{
						AkUnitySoundEngine.SetSwitch(soundTypeSwitch.Group, soundTypeSwitch.Value, unitEntity.View.gameObject);
					}
					AkSwitchReference muffledTypeSwitch = buff2.Blueprint.MuffledTypeSwitch;
					if (muffledTypeSwitch.IsValid())
					{
						AkUnitySoundEngine.SetSwitch(muffledTypeSwitch.Group, muffledTypeSwitch.Value, unitEntity.View.gameObject);
					}
				}
				else if (dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters != null)
				{
					try
					{
						AkSwitchReference akSwitchReference2 = dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters.SoundTypeSwitch;
						if (akSwitchReference2.IsValid())
						{
							AkUnitySoundEngine.SetSwitch(akSwitchReference2.Group, akSwitchReference2.Value, unitEntity.View.gameObject);
						}
						AkSwitchReference akSwitchReference3 = dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters.MuffledTypeSwitch;
						if (akSwitchReference3.IsValid())
						{
							AkUnitySoundEngine.SetSwitch(akSwitchReference3.Group, akSwitchReference3.Value, unitEntity.View.gameObject);
						}
					}
					catch
					{
						PFLog.Default.Error($"{dealDamage.SourceAbility?.Weapon} don't have sound type switch");
					}
				}
				else
				{
					AkSwitchReference damageSoundSwitch = ConfigRoot.Instance.HitSystemRoot.GetDamageSoundSwitch(dealDamage.ResultDamage.Type);
					if (damageSoundSwitch.IsValid())
					{
						AkUnitySoundEngine.SetSwitch(damageSoundSwitch.Group, damageSoundSwitch.Value, unitEntity.View.gameObject);
					}
				}
				string text = dealDamage.SourceAbility?.Weapon?.Blueprint?.VisualParameters?.SoundTypeSwitch?.Value ?? string.Empty;
				string text2 = unitEntity.Blueprint?.VisualSettings?.BodyTypeSoundSwitch?.Value ?? string.Empty;
				AkUnitySoundEngine.SetRTPCValue("RTPC_SharpCheck", (text == "Sharp" && text2 == "Flesh") ? 1 : 0, unitEntity.View.gameObject);
				SoundEventPlayer.PlaySound(ConfigRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkSoundSettings, dealDamage.ConcreteTarget.View.gameObject);
			}
		}
		target2 = dealDamage.Target;
		DestructibleEntity destructibleEntity = target2 as DestructibleEntity;
		if (destructibleEntity == null)
		{
			return;
		}
		HitEntry hitEntry = ConfigRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry x) => x.Type == destructibleEntity.SurfaceType);
		if (hitEntry == null)
		{
			return;
		}
		FxHelper.SpawnFxOnEntity(hitEntry.StaticHitEffects.HitEffectLink.Load(), target.View);
		AkUnitySoundEngine.SetSwitch("HitMainType", dealDamage.ResultIsCritical ? "Crit" : "Normal", target.View.gameObject);
		AkUnitySoundEngine.SetSwitch(hitEntry.Switch.Group, hitEntry.Switch.Value, destructibleEntity.View.gameObject);
		try
		{
			AkSwitchReference akSwitchReference4 = dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters.SoundTypeSwitch;
			if (akSwitchReference4.IsValid())
			{
				AkUnitySoundEngine.SetSwitch(akSwitchReference4.Group, akSwitchReference4.Value, destructibleEntity.View.gameObject);
			}
		}
		catch
		{
			PFLog.Default.Error($"{dealDamage.SourceAbility?.Weapon} don't have sound type switch");
		}
		SoundEventPlayer.PlaySound(ConfigRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkSoundSettings, target.View.gameObject);
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
	}

	void IWarhammerAttackHandler.HandleAttack(RulePerformAttack rule)
	{
		if (rule.Result != AttackResult.Defended)
		{
			return;
		}
		UnitAnimationManager maybeAnimationManager = rule.Target.MaybeAnimationManager;
		if (!(maybeAnimationManager == null))
		{
			UnitAnimationActionHandle unitAnimationActionHandle = maybeAnimationManager.CreateHandle(UnitAnimationType.Defence, errorOnEmpty: false);
			if (unitAnimationActionHandle != null)
			{
				maybeAnimationManager.Execute(unitAnimationActionHandle);
			}
		}
	}

	public void HandleAreaEffectSpawned()
	{
		if (EventInvokerExtensions.Entity is AreaEffectEntity { Context: { MaybeCaster: not null } context } areaEffectEntity)
		{
			TryPlayAreaEffectFX(context.MaybeCaster, context.ClickedTarget, areaEffectEntity, null, AbilityEventType.Start);
			TryPlaySoundFX(context, context.ClickedTarget, areaEffectEntity, AbilityEventType.Start);
		}
	}

	public void HandleAreaEffectDestroyed()
	{
		if (!(EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity))
		{
			return;
		}
		AbilityExecutionContext asAbilityContext = areaEffectEntity.Context.AsAbilityContext;
		if (asAbilityContext != null)
		{
			TryPlayVisualFX(asAbilityContext.Caster, asAbilityContext.ClickedTarget, asAbilityContext.Ability, AbilityEventType.EndAreaEffect);
			TryPlaySoundFX(asAbilityContext, asAbilityContext.ClickedTarget, asAbilityContext.Ability, AbilityEventType.EndAreaEffect);
			TryPlayAreaEffectFX(asAbilityContext.Caster, asAbilityContext.ClickedTarget, areaEffectEntity, null, AbilityEventType.EndAreaEffect);
			TryPlaySoundFX(asAbilityContext, asAbilityContext.ClickedTarget, areaEffectEntity, AbilityEventType.EndAreaEffect);
		}
		else if (areaEffectEntity.Context != null)
		{
			if (areaEffectEntity.Context.MaybeCaster != null)
			{
				TryPlayAreaEffectFX(areaEffectEntity.Context.MaybeCaster, areaEffectEntity.Context.ClickedTarget, areaEffectEntity, null, AbilityEventType.EndAreaEffect);
			}
			TryPlaySoundFX(areaEffectEntity.Context, areaEffectEntity.Context.ClickedTarget, areaEffectEntity, AbilityEventType.EndAreaEffect);
		}
	}

	public GameObject[] OnBuffEffectApplied(IBuff buff)
	{
		MechanicEntity caster = buff.Caster;
		TargetWrapper target = buff.Target;
		TryPlaySoundFX(caster, target, buff, AbilityEventType.Start);
		return TryPlayBuffFX(caster, target, buff, null, AbilityEventType.Start);
	}

	public void OnBuffEffectRemoved(IBuff buff)
	{
		MechanicEntity caster = buff.Caster;
		TargetWrapper target = buff.Target;
		TryPlaySoundFX(caster, target, buff, AbilityEventType.End);
		TryPlayBuffFX(caster, target, buff, null, AbilityEventType.End);
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command is UnitUseAbility unitUseAbility && unitUseAbility.Target != null)
		{
			TryPlayVisualFX(unitUseAbility.Executor, unitUseAbility.Target, unitUseAbility.Ability, AbilityEventType.StarUseAbilityCommand);
		}
	}
}
