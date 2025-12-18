using System;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[Serializable]
[TypeId("89e4bbc67dc7432890ec48888d84fefa")]
public sealed class EntityEnterOrExitAreaEffectTrigger : MechanicEntityFactComponentDelegate, IAreaEffectEntityHandler, ISubscriber<IAreaEffectEntity>, ISubscriber
{
	[SerializeField]
	[HideIf("HasAddAreaEffectComponent")]
	private BpRef<BlueprintAreaEffect> _areaEffect;

	[Tooltip("Триггер срабатывает только на ареа эффекты поспавненные овнером триггера.")]
	[SerializeField]
	[HideIf("HasAddAreaEffectComponent")]
	private bool _onlySpawnedByOwner;

	[Tooltip("Триггер срабатывает только на юнитов, для которых выполнен этот ристрикшен.")]
	public RestrictionCalculator UnitRestriction;

	public ActionList OnUnitEnter;

	public ActionList OnUnitExit;

	public ActionList OnUnitEnterOrExit;

	private AddAreaEffect AddAreaEffect => base.OwnerBlueprint?.GetComponent<AddAreaEffect>();

	private bool HasAddAreaEffectComponent => AddAreaEffect != null;

	private BlueprintAreaEffect Blueprint => AddAreaEffect?.AreaEffect ?? _areaEffect.Blueprint;

	private bool OnlySpawnedByOwner
	{
		get
		{
			if (AddAreaEffect == null)
			{
				return _onlySpawnedByOwner;
			}
			return true;
		}
	}

	void IAreaEffectEntityHandler.HandleEntityEnterAreaEffect(MechanicEntity entity)
	{
		AreaEffectEntity areaEffect = (AreaEffectEntity)ContextData<EventInvoker>.Current.InvokerEntity;
		if (!IsSuitableAreaEffect(areaEffect) || !IsSuitableEntity(entity))
		{
			return;
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(entity))
		{
			OnUnitEnter.Run();
			OnUnitEnterOrExit.Run();
		}
	}

	void IAreaEffectEntityHandler.HandleEntityExitAreaEffect(MechanicEntity entity)
	{
		AreaEffectEntity areaEffect = (AreaEffectEntity)ContextData<EventInvoker>.Current.InvokerEntity;
		if (!IsSuitableAreaEffect(areaEffect) || !IsSuitableEntity(entity))
		{
			return;
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(entity))
		{
			OnUnitExit.Run();
			OnUnitEnterOrExit.Run();
		}
	}

	private bool IsSuitableAreaEffect(AreaEffectEntity areaEffect)
	{
		if (Blueprint != areaEffect.Blueprint)
		{
			return false;
		}
		if (!OnlySpawnedByOwner)
		{
			return true;
		}
		return base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.Contains(areaEffect) ?? false;
	}

	private bool IsSuitableEntity(MechanicEntity entity)
	{
		return UnitRestriction.IsPassed(base.Context, entity);
	}
}
