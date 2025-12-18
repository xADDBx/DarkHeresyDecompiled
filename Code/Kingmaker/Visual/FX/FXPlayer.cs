using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Sound;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.FX;

public static class FXPlayer
{
	public static GameObject[] Play([NotNull] IFXSettingsProvider effectsProvider, [NotNull] MechanicEntity caster, MappedAnimationEventType eventType, [CanBeNull] AbilityData ability = null)
	{
		IEnumerable<IFXSettings> fXs = effectsProvider.GetFXs(eventType);
		if (fXs == null)
		{
			return Array.Empty<GameObject>();
		}
		List<GameObject> list = new List<GameObject>();
		foreach (IFXSettings item in fXs)
		{
			list.AddRange(Play(item, caster, null, ability));
		}
		return list.ToArray();
	}

	public static GameObject[] Play([NotNull] IFXSettingsProvider effectsProvider, [NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, AbilityEventType eventType, [CanBeNull] AbilityData ability = null)
	{
		IEnumerable<IFXSettings> fXs = effectsProvider.GetFXs(eventType);
		if (fXs == null || !fXs.Any())
		{
			return Array.Empty<GameObject>();
		}
		List<GameObject> list = new List<GameObject>();
		foreach (IFXSettings item in fXs)
		{
			list.AddRange(Play(item, caster, target, ability));
		}
		return list.ToArray();
	}

	public static GameObject[] Play(IFXSettings effect, MechanicEntity caster, [CanBeNull] TargetWrapper target, [CanBeNull] AbilityData ability = null)
	{
		if (effect.Settings.UseRandomVisualFX)
		{
			VisualFXSettings visualFXSettings = effect.Settings.FXs.Random(PFStatefulRandom.Visuals.Fx);
			if (visualFXSettings != null)
			{
				return PlayFX(visualFXSettings, caster, target, effect, ability);
			}
			return Array.Empty<GameObject>();
		}
		List<GameObject> list = new List<GameObject>();
		VisualFXSettings[] fXs = effect.Settings.FXs;
		foreach (VisualFXSettings fx in fXs)
		{
			list.AddRange(PlayFX(fx, caster, target, effect, ability));
		}
		return list.ToArray();
	}

	private static GameObject[] PlayFX([NotNull] VisualFXSettings fx, [NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [NotNull] IFXSettings settings, [CanBeNull] AbilityData ability)
	{
		GameObject gameObject = fx.Prefab?.Load();
		SoundFx component;
		bool flag = ability?.FXSettings?.SoundFXSettings != null && gameObject != null && gameObject.TryGetComponent<SoundFx>(out component);
		Quaternion value = ((((settings.Target == FXTarget.TargetPoint && target != null) || (settings.Target == FXTarget.Target && (object)target != null && target.IsPoint)) && settings.OrientationFromCasterToTarget) ? Quaternion.LookRotation(target.Point - caster.Position) : Quaternion.identity);
		switch (settings.Target)
		{
		case FXTarget.Caster:
		{
			GameObject[] array = new GameObject[1] { FxHelper.SpawnFxOnEntity(gameObject, caster.View, enableFxObject: true, settings.OverrideTargetOrientationSource) };
			if (flag && array.Length != 0)
			{
				GameObject[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i].TryGetComponent<SoundFx>(out var component2))
					{
						component2.BlockSoundFXPlaying = true;
					}
				}
			}
			return array;
		}
		case FXTarget.Target:
		{
			GameObject[] array = ((target != null && target.Entity != null) ? new GameObject[1] { FxHelper.SpawnFxOnEntity(gameObject, target.Entity.View, enableFxObject: true, settings.OverrideTargetOrientationSource) } : ((!(target != null) || !target.IsPoint) ? Array.Empty<GameObject>() : new GameObject[1] { FxHelper.SpawnFxOnPoint(gameObject, target.Point, value) }));
			if (flag && array.Length != 0)
			{
				GameObject[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i].TryGetComponent<SoundFx>(out var component3))
					{
						component3.BlockSoundFXPlaying = true;
					}
				}
			}
			return array;
		}
		case FXTarget.CasterWeapon:
		{
			WeaponSlot weaponSlot2 = (ability?.Weapon?.HoldingSlot ?? caster.GetFirstWeapon()?.HoldingSlot) as WeaponSlot;
			return new GameObject[1] { FxHelper.SpawnFxOnWeapon(gameObject, caster.View, weaponSlot2?.FxSnapMap) };
		}
		case FXTarget.CasterOffHandWeapon:
		{
			WeaponSlot weaponSlot = caster.GetSecondWeapon()?.HoldingSlot as WeaponSlot;
			return new GameObject[1] { FxHelper.SpawnFxOnWeapon(gameObject, caster.View, weaponSlot?.FxSnapMap) };
		}
		case FXTarget.CasterAllWeapon:
			if (caster is UnitEntity unitEntity)
			{
				List<GameObject> list = new List<GameObject>();
				foreach (HandsEquipmentSet handsEquipmentSet in unitEntity.Body.HandsEquipmentSets)
				{
					foreach (HandSlot hand in handsEquipmentSet.Hands)
					{
						if (hand?.FxSnapMap != null)
						{
							list.Add(FxHelper.SpawnFxOnWeapon(gameObject, caster.View, hand.FxSnapMap));
						}
					}
				}
				return list.ToArray();
			}
			return Array.Empty<GameObject>();
		case FXTarget.TargetPoint:
			return (!(target != null)) ? Array.Empty<GameObject>() : new GameObject[1] { FxHelper.SpawnFxOnPoint(gameObject, target.Point, value) };
		default:
			throw new ArgumentOutOfRangeException("Target", settings.Target, null);
		}
	}
}
