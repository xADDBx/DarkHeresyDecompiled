using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[ComponentName("Ability/FX/AbilitySpawnFx")]
[TypeId("c788cf748ec86bc45b986ac4de28cf45")]
public class AbilitySpawnFx : BlueprintComponent, IResourcesHolder
{
	public PrefabLink PrefabLink;

	public AbilitySpawnFxTime Time;

	public AbilitySpawnFxAnchor Anchor;

	public AbilitySpawnFxWeaponTarget WeaponTarget;

	public bool DestroyOnCast;

	public float Delay;

	[Header("Position")]
	public AbilitySpawnFxAnchor PositionAnchor = AbilitySpawnFxAnchor.None;

	public bool UseExtraAttachment;

	[ShowIf("UseExtraAttachment")]
	public int AttachingSnapperIndex;

	[Header("Orientation")]
	public AbilitySpawnFxAnchor OrientationAnchor = AbilitySpawnFxAnchor.None;

	public AbilitySpawnFxOrientation OrientationMode;

	public bool UsesSelectedTarget
	{
		get
		{
			if (Anchor != AbilitySpawnFxAnchor.SelectedTarget && PositionAnchor != AbilitySpawnFxAnchor.SelectedTarget)
			{
				return OrientationAnchor == AbilitySpawnFxAnchor.SelectedTarget;
			}
			return true;
		}
	}

	public void Spawn([NotNull] IEvalContext context, [CanBeNull] TargetWrapper selectedTarget, List<GameObject> list = null)
	{
		if (context.DisableFx || ((selectedTarget != null) ^ UsesSelectedTarget))
		{
			return;
		}
		TargetWrapper targetWrapper = ResolveAnchor(Anchor, context, selectedTarget);
		if (!(targetWrapper == null))
		{
			if (Delay <= 0f)
			{
				DoSpawn(context, selectedTarget, list, targetWrapper);
			}
			else if (context.Caster?.View != null)
			{
				Game.Instance.Controllers.CoroutinesController.Start(DoSpawnDelayed(context, selectedTarget, list, targetWrapper), context.Caster.View.AsMechanicEntityView());
			}
		}
		IEnumerator DoSpawnDelayed(IEvalContext context, TargetWrapper selectedTarget, List<GameObject> list, TargetWrapper target)
		{
			yield return YieldInstructions.WaitForSecondsGameTime(Delay);
			DoSpawn(context, selectedTarget, list, target);
		}
	}

	private void DoSpawn(IEvalContext context, TargetWrapper selectedTarget, List<GameObject> list, TargetWrapper target)
	{
		GameObject prefab = PrefabLink.Load();
		if (prefab == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IAbilitySoundZoneTrigger h)
		{
			h.TriggerSoundZone(context, prefab);
		});
		GameObject gameObject = null;
		if (target.Entity != null)
		{
			if (target.Entity is UnitEntity unitEntity)
			{
				switch (WeaponTarget)
				{
				case AbilitySpawnFxWeaponTarget.None:
					gameObject = FxHelper.SpawnFxOnEntity(prefab, target.Entity.View.AsMechanicEntityView());
					break;
				case AbilitySpawnFxWeaponTarget.Primary:
					gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View.AsMechanicEntityView(), unitEntity.Body.PrimaryHand.FxSnapMap);
					break;
				case AbilitySpawnFxWeaponTarget.Secondary:
					gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View.AsMechanicEntityView(), unitEntity.Body.SecondaryHand.FxSnapMap);
					break;
				case AbilitySpawnFxWeaponTarget.All:
				{
					foreach (WeaponSlot item in unitEntity.Body.AllSlots.OfType<WeaponSlot>())
					{
						if (item.HasWeapon)
						{
							gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View.AsMechanicEntityView(), item.FxSnapMap);
							if ((bool)gameObject)
							{
								list?.Add(gameObject);
								PositionFx(context, selectedTarget, gameObject);
							}
						}
					}
					return;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		else
		{
			Quaternion value = Quaternion.Euler(0f, target.Orientation, 0f);
			gameObject = FxHelper.SpawnFxOnPoint(prefab, target.Point, value);
		}
		if ((bool)gameObject)
		{
			PositionFx(context, selectedTarget, gameObject);
			list?.Add(gameObject);
		}
	}

	private void PositionFx(IEvalContext context, TargetWrapper selectedTarget, GameObject fx)
	{
		if (!fx)
		{
			return;
		}
		TargetWrapper targetWrapper = ResolveAnchor(PositionAnchor, context, selectedTarget);
		if (targetWrapper != null)
		{
			fx.transform.position = targetWrapper.Point;
			if (UseExtraAttachment)
			{
				FxHelper.Resnap(fx, targetWrapper.Entity?.View.gameObject, AttachingSnapperIndex);
			}
		}
		TargetWrapper targetWrapper2 = ResolveAnchor(OrientationAnchor, context, selectedTarget);
		if (targetWrapper2 != null)
		{
			switch (OrientationMode)
			{
			case AbilitySpawnFxOrientation.Copy:
				fx.transform.rotation = Quaternion.Euler(0f, targetWrapper2.Orientation, 0f);
				break;
			case AbilitySpawnFxOrientation.TurnFrom:
				fx.transform.rotation = Quaternion.LookRotation(fx.transform.position - targetWrapper2.Point);
				break;
			case AbilitySpawnFxOrientation.TurnTo:
				fx.transform.rotation = Quaternion.LookRotation(targetWrapper2.Point - fx.transform.position);
				break;
			default:
				PFLog.Default.Error("Unknown orientation mode {0}", OrientationMode);
				break;
			}
		}
	}

	[CanBeNull]
	private static TargetWrapper ResolveAnchor(AbilitySpawnFxAnchor anchor, [NotNull] IEvalContext context, [CanBeNull] TargetWrapper selectedTarget)
	{
		switch (anchor)
		{
		case AbilitySpawnFxAnchor.None:
			return null;
		case AbilitySpawnFxAnchor.Caster:
			if (context.Caster == null)
			{
				PFLog.Default.Error("Caster is missing");
				return null;
			}
			return context.Caster;
		case AbilitySpawnFxAnchor.ClickedTarget:
			return context.ClickedTarget ?? selectedTarget;
		case AbilitySpawnFxAnchor.SelectedTarget:
			return selectedTarget;
		default:
			PFLog.Default.Error("Unknown anchor {0}", anchor);
			return null;
		}
	}
}
