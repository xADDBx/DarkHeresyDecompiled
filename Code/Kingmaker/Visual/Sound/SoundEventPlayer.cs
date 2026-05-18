using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Sound.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.FX;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public static class SoundEventPlayer
{
	public static void Play([NotNull] ISoundSettingsProvider soundProvider, [NotNull] MechanicEntity caster, UnitSoundAnimationEventType eventType)
	{
		IEnumerable<ISoundSettings> sounds = soundProvider.GetSounds(eventType);
		if (sounds == null)
		{
			return;
		}
		foreach (ISoundSettings item in sounds)
		{
			ISoundSettings effect = item;
			if (effect.Settings.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow(effect, caster);
			}
			IEnumerator PlaySoundDelayed()
			{
				yield return new WaitForSeconds(effect.Settings.Delay);
				PlaySoundNow(effect, caster);
			}
		}
	}

	public static void Play([NotNull] ISoundSettingsProvider soundProvider, [NotNull] IEvalContext context, AbilityEventType eventType)
	{
		IEnumerable<ISoundSettings> sounds = soundProvider.GetSounds(eventType);
		if (sounds == null || context.Caster == null)
		{
			return;
		}
		foreach (ISoundSettings item in sounds)
		{
			ISoundSettings effect = item;
			GameObject targetObject = GetSoundTarget(context.Caster, context.ClickedTarget, effect.Target);
			if (effect.Settings.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow(effect, context.Caster, targetObject);
			}
			IEnumerator PlaySoundDelayed()
			{
				yield return new WaitForSeconds(effect.Settings.Delay);
				PlaySoundNow(effect, context.Caster, targetObject);
			}
		}
	}

	public static void Play([NotNull] ISoundSettingsProvider soundProvider, [NotNull] MechanicEntity entity, [NotNull] TargetWrapper target, AbilityEventType eventType)
	{
		IEnumerable<ISoundSettings> sounds = soundProvider.GetSounds(eventType);
		if (sounds == null)
		{
			return;
		}
		foreach (ISoundSettings item in sounds)
		{
			ISoundSettings effect = item;
			GameObject targetObject = GetSoundTarget(entity, target, effect.Target);
			if (effect.Settings.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow(effect, entity, targetObject, target?.Entity);
			}
			IEnumerator PlaySoundDelayed()
			{
				yield return new WaitForSeconds(effect.Settings.Delay);
				PlaySoundNow(effect, entity, targetObject);
			}
		}
	}

	private static void PlaySoundNow(ISoundSettings effect, MechanicEntity entity)
	{
		uint id = SoundEventsManager.PostEvent(effect.Settings.Event, entity.View.gameObject);
		PlaySoundWithId(effect, entity, id);
	}

	private static void PlaySoundNow(ISoundSettings effect, MechanicEntity entity, GameObject gameObject, [CanBeNull] MechanicEntity target = null)
	{
		if (!(gameObject == null))
		{
			uint id = SoundEventsManager.PostEvent(effect.Settings.Event, gameObject);
			PlaySoundWithId(effect, entity, id, target);
		}
	}

	private static void PlaySoundWithId(ISoundSettings effect, MechanicEntity entity, uint id, [CanBeNull] MechanicEntity target = null)
	{
		if (id != 0)
		{
			AkUnitySoundEngine.SetRTPCValueByPlayingID("SpellGain", effect.Settings.Gain, id);
			AkUnitySoundEngine.SetRTPCValueByPlayingID("SpellPitch", effect.Settings.Pitch, id);
			GameSyncSettings[] gameSyncs = effect.Settings.GameSyncs;
			for (int i = 0; i < gameSyncs.Length; i++)
			{
				gameSyncs[i]?.Sync(entity, EvalContext.Current, id);
			}
		}
	}

	public static void PlaySound(SoundFXSettings sound, GameObject gameObject)
	{
		if (AkUnitySoundEngine.IsInitialized())
		{
			if (sound.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow();
			}
		}
		IEnumerator PlaySoundDelayed()
		{
			yield return new WaitForSeconds(sound.Delay);
			PlaySoundNow();
		}
		void PlaySoundNow()
		{
			uint num = SoundEventsManager.PostEvent(sound.Event, gameObject);
			if (num != 0)
			{
				AkUnitySoundEngine.SetRTPCValueByPlayingID("SpellGain", sound.Gain, num);
				AkUnitySoundEngine.SetRTPCValueByPlayingID("SpellPitch", sound.Pitch, num);
			}
		}
	}

	private static GameObject GetSoundTarget(MechanicEntity caster, [CanBeNull] TargetWrapper target, FXTarget targetType)
	{
		switch (targetType)
		{
		case FXTarget.Caster:
			if (caster is SimpleCaster simpleCaster2 && simpleCaster2.TrapParentObject != null)
			{
				return simpleCaster2.TrapParentObject;
			}
			return caster.View.AsMechanicEntityView()?.gameObject;
		case FXTarget.Target:
		{
			if ((object)target != null && target.IsPoint)
			{
				GameObject gameObject = new GameObject("SoundTarget_" + target.Point);
				gameObject.transform.SetPositionAndRotation(target.Point, Quaternion.identity);
				gameObject.AddComponent<AutoDestroy>().Lifetime = 10f;
				return gameObject;
			}
			GameObject gameObject2 = (target?.Entity?.View.AsMechanicEntityView()?.gameObject).Or(null);
			if ((object)gameObject2 == null)
			{
				MechanicEntityView mechanicEntityView = caster.View.AsMechanicEntityView();
				if ((object)mechanicEntityView == null)
				{
					return null;
				}
				gameObject2 = mechanicEntityView.gameObject;
			}
			return gameObject2;
		}
		case FXTarget.CasterWeapon:
			if (caster is SimpleCaster simpleCaster && simpleCaster.TrapParentObject != null)
			{
				return simpleCaster.TrapParentObject;
			}
			return caster.View.AsMechanicEntityView()?.gameObject;
		case FXTarget.CasterAllWeapon:
			return null;
		case FXTarget.CasterOffHandWeapon:
			return null;
		default:
			throw new ArgumentOutOfRangeException("targetType", targetType, null);
		}
	}
}
