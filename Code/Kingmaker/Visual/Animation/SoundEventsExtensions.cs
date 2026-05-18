using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums.Sound;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Visual.Animation;

public static class SoundEventsExtensions
{
	public static uint PostSoundEvent(this MechanicEntityView view, string eventName, float volume = 1f)
	{
		return view.PostSoundEvent(eventName, volume, withPrefix: false);
	}

	public static uint PostSoundEventWithPrefix(this MechanicEntityView view, string eventName, float volume = 1f)
	{
		return view.PostSoundEvent(eventName, volume, withPrefix: true);
	}

	public static uint PostSoundEvent(this MechanicEntityView view, string eventName, float volume, bool withPrefix)
	{
		if (view != null && view.EntityData != null)
		{
			float in_value = ((!view.EntityData.IsInFogOfWar) ? 1f : (view.EntityData.IsRevealed ? 0.5f : 0f));
			AkUnitySoundEngine.SetRTPCValue("Audibility", in_value, view.gameObject);
			float in_value2 = view.Data?.MaybeAnimationManager?.CurrentAction?.SpeedScale ?? 1f;
			AkUnitySoundEngine.SetRTPCValue("CombatSpeed", in_value2, view.gameObject);
			return SoundEventsManager.PostEvent(((withPrefix && view is AbstractUnitEntityView abstractUnitEntityView) ? abstractUnitEntityView.Blueprint.VisualSettings.FoleySoundPrefix : "") + eventName, view.gameObject);
		}
		return 0u;
	}

	public static void StopPlayingSoundById(this MechanicEntityView unitView, uint playingId)
	{
		SoundEventsManager.StopPlayingById(playingId);
	}

	public static void PostSoundEventMapped(this MechanicEntityView view, MappedAnimationEventType evt)
	{
		if (view == null || !(view is AbstractUnitEntityView abstractUnitEntityView))
		{
			return;
		}
		AbstractUnitEntity data = abstractUnitEntityView.Data;
		if (data == null || data.IsDisposed)
		{
			return;
		}
		AskWrapper askWrapper = abstractUnitEntityView.Asks?.SelectAnimationBark(evt);
		if (askWrapper == null || !askWrapper.HasBarks)
		{
			return;
		}
		using (EvalContext.PushAsksContext(abstractUnitEntityView.EntityData, abstractUnitEntityView.EntityData))
		{
			askWrapper.Schedule();
		}
	}

	public static void AbilityAnimationEvent(this MechanicEntityView view, UnitSoundAnimationEventType eventType)
	{
		if (view == null || !(view is UnitEntityView unitEntityView))
		{
			return;
		}
		BlueprintAbility blueprintAbility = ((UnitAnimationActionHandle)(unitEntityView.AnimationManager?.CurrentAction))?.Spell;
		if (blueprintAbility != null)
		{
			BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = (unitEntityView.Data?.Abilities.GetAbility(blueprintAbility)?.Data.FXSettings ?? blueprintAbility.FXSettings)?.SoundFXSettings;
			if (blueprintAbilitySoundFXSettings != null && unitEntityView.Data.Context.MaybeCaster != null)
			{
				SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, unitEntityView.Data.Context.MaybeCaster, eventType);
			}
		}
	}

	public static void PostMainWeaponEquipEvent(this MechanicEntityView view)
	{
		if (!(view == null) && view is UnitEntityView unitEntityView)
		{
			ItemEntityWeapon itemEntityWeapon = unitEntityView.EntityData?.Body.PrimaryHand.MaybeWeapon;
			if (itemEntityWeapon != null)
			{
				string equipSound = itemEntityWeapon.Blueprint.VisualParameters.EquipSound;
				unitEntityView.PostSoundEvent(equipSound, 1f, withPrefix: false);
			}
		}
	}

	public static void PostOffWeaponEquipEvent(this MechanicEntityView view)
	{
		if (!(view == null) && view is UnitEntityView unitEntityView)
		{
			ItemEntityWeapon itemEntityWeapon = unitEntityView.EntityData?.Body.SecondaryHand.MaybeWeapon;
			if (itemEntityWeapon != null)
			{
				string equipSound = itemEntityWeapon.Blueprint.VisualParameters.EquipSound;
				unitEntityView.PostSoundEvent(equipSound, 1f, withPrefix: false);
			}
		}
	}

	public static void PostMainWeaponUnequipEvent(this MechanicEntityView view)
	{
		if (!(view == null) && view is UnitEntityView { HandsEquipment: not null } unitEntityView)
		{
			ItemEntity itemEntity = ((!unitEntityView.HandsEquipment.IsMainHandMismatched) ? unitEntityView.EntityData?.Body.PrimaryHand.MaybeItem : unitEntityView.EntityData?.Body.HandsEquipmentSets[(unitEntityView.EntityData.Body.CurrentHandEquipmentSetIndex + 1) % 2].PrimaryHand.MaybeItem);
			if (itemEntity != null && itemEntity.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				string unequipSound = blueprintItemWeapon.VisualParameters.UnequipSound;
				unitEntityView.PostSoundEvent(unequipSound, 1f, withPrefix: false);
			}
		}
	}

	public static void PostOffWeaponUnequipEvent(this MechanicEntityView view)
	{
		if (!(view == null) && view is UnitEntityView { HandsEquipment: not null } unitEntityView)
		{
			ItemEntity itemEntity = ((!unitEntityView.HandsEquipment.IsOffHandMismatched) ? unitEntityView.EntityData?.Body.SecondaryHand.MaybeItem : unitEntityView.EntityData?.Body.HandsEquipmentSets[(unitEntityView.EntityData.Body.CurrentHandEquipmentSetIndex + 1) % 2].SecondaryHand.MaybeItem);
			if (itemEntity != null && itemEntity.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				string unequipSound = blueprintItemWeapon.VisualParameters.UnequipSound;
				unitEntityView.PostSoundEvent(unequipSound, 1f, withPrefix: false);
			}
		}
	}

	public static void PostArmorFoleyEvent(this MechanicEntityView view)
	{
		if (!(view == null) && view is UnitEntityView unitEntityView)
		{
			string eventName = ConfigRoot.Instance.Sound.NoArmorDefaultFoleySound;
			if (unitEntityView.EntityData?.Body.Armor.MaybeItem is ItemEntityArmor itemEntityArmor)
			{
				eventName = itemEntityArmor.Blueprint.VisualParameters.AnimationFoleySound;
			}
			unitEntityView.PostSoundEvent(eventName, 1f, withPrefix: false);
		}
	}

	public static void PostEventWithSurface(this MechanicEntityView view, string eventName)
	{
		if (!(view == null))
		{
			SpawnDustFx(eventName, view);
			SetTerrainSwitch(view);
			view.PostSoundEvent(eventName, 1f, withPrefix: false);
		}
	}

	public static void PlayBodyFall(this MechanicEntityView view, string eventName)
	{
		if (!(view == null) && view is UnitEntityView unitEntityView)
		{
			SpawnDustFx(eventName, unitEntityView);
			AkSwitchReference akSwitchReference = unitEntityView.EntityData?.Body.Armor.MaybeArmor?.Blueprint.VisualParameters.SoundSwitch;
			if (akSwitchReference != null && !akSwitchReference.Value.IsNullOrEmpty())
			{
				akSwitchReference.Set(unitEntityView.gameObject);
			}
			else
			{
				unitEntityView.Blueprint?.VisualSettings?.BodyTypeSoundSwitch.Set(unitEntityView.gameObject);
			}
			unitEntityView.Blueprint?.VisualSettings?.BodySizeSoundSwitch.Set(unitEntityView.gameObject);
			SetTerrainSwitch(unitEntityView);
			unitEntityView.PostSoundEvent(eventName, 1f, withPrefix: false);
		}
	}

	public static void PlayFootstep(this MechanicEntityView view, string eventName)
	{
		if (!(view == null) && view is AbstractUnitEntityView abstractUnitEntityView)
		{
			SpawnDustFx(eventName, abstractUnitEntityView);
			UnitVisualSettings unitVisualSettings = abstractUnitEntityView.Blueprint?.VisualSettings;
			if (unitVisualSettings != null)
			{
				unitVisualSettings.FootTypeSoundSwitch.Set(abstractUnitEntityView.gameObject);
				unitVisualSettings.FootSizeSoundSwitch.Set(abstractUnitEntityView.gameObject);
			}
			SetTerrainSwitch(abstractUnitEntityView);
			abstractUnitEntityView.PostSoundEvent(eventName, 1f, withPrefix: false);
		}
	}

	private static void SpawnDustFx(string eventName, MechanicEntityView view)
	{
		if (ConfigRoot.Instance.FxRoot.FallEventStrings.Any((string s) => s == eventName))
		{
			FxHelper.SpawnFxOnEntity(ConfigRoot.Instance.FxRoot.DustOnFallPrefab, view);
		}
	}

	private static void SetTerrainSwitch(MechanicEntityView view)
	{
		SurfaceType? surfaceSoundTypeSwitch = SoundSurfaceMap.GetSurfaceSoundTypeSwitch(view.transform.position);
		if (surfaceSoundTypeSwitch.HasValue)
		{
			string text = surfaceSoundTypeSwitch.ToString();
			if (!text.IsNullOrEmpty())
			{
				AkUnitySoundEngine.SetSwitch("Terrain", text, view.gameObject);
			}
		}
	}
}
