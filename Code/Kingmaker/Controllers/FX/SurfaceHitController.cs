using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Controllers.FX;

public class SurfaceHitController : IAreaHandler, ISubscriber
{
	private const int MAX_SPAWNED_EFFECTS_COUNT = 32;

	private static SurfaceHitController s_Instance;

	private RingBuffer<GameObject> m_SpawnedEffectsBuffer;

	public static SurfaceHitController Instance
	{
		get
		{
			if (s_Instance == null && Application.isPlaying)
			{
				s_Instance = new SurfaceHitController(32);
			}
			return s_Instance;
		}
	}

	private SurfaceHitController(int maxSpawnedEffectsCount)
	{
		m_SpawnedEffectsBuffer = new RingBuffer<GameObject>(maxSpawnedEffectsCount);
		EventBus.Subscribe(this);
	}

	public void ProcessProjectileHits(Projectile projectile, in SphereBounds hitFxCullingSphere)
	{
		SurfaceHitObject surfaceHitObject = null;
		RaycastHit hitPoint = default(RaycastHit);
		bool flag = false;
		foreach (RaycastHit item in projectile.Hits.OrderBy((RaycastHit hit) => Vector3.SqrMagnitude(hit.point - projectile.LaunchPosition)))
		{
			if (item.transform.TryGetComponent<SurfaceHitObject>(out var component))
			{
				flag = true;
				surfaceHitObject = component;
				hitPoint = item;
				break;
			}
		}
		if (flag)
		{
			StaticHitEffects staticHitEffects = ConfigRoot.Instance.HitSystemRoot.GetStaticHitEffects(surfaceHitObject.m_SoundSurfaceType);
			Vector3 point = hitPoint.point;
			if (!hitFxCullingSphere.Contains(in point))
			{
				SpawnProjectileHit(projectile, staticHitEffects, hitPoint);
			}
			SpawnSurfaceDecal(projectile, staticHitEffects, hitPoint);
		}
	}

	private void SpawnSurfaceDecal(Projectile projectile, [CanBeNull] StaticHitEffects staticHitEffects, RaycastHit hitPoint)
	{
		if (!(projectile?.Blueprint.ProjectileHit?.NoHitDecal).GetValueOrDefault())
		{
			GameObject gameObject = ((!(projectile?.Blueprint.ProjectileHit?.HitDecal?.Exists()).GetValueOrDefault()) ? ((!(staticHitEffects?.HitDecalLink?.Exists()).GetValueOrDefault()) ? null : staticHitEffects.HitDecalLink?.Load()) : projectile.Blueprint.ProjectileHit?.HitDecal?.Load());
			if (gameObject != null)
			{
				Quaternion rotation = Quaternion.FromToRotation(Vector3.down, -hitPoint.normal);
				SpawnInCircularBuffer(gameObject, hitPoint.point, rotation);
			}
		}
	}

	private static void SpawnProjectileHit(Projectile projectile, [CanBeNull] StaticHitEffects staticHitEffects, RaycastHit hitPoint)
	{
		GameObject gameObject = ((!(projectile.Blueprint.ProjectileHit?.HitFx?.Exists()).GetValueOrDefault()) ? ((!(staticHitEffects?.HitFXLink?.Exists()).GetValueOrDefault()) ? null : staticHitEffects.HitFXLink?.Load()) : projectile.Blueprint.ProjectileHit?.HitFx?.Load());
		if (!(gameObject != null))
		{
			return;
		}
		GameObject gameObject2 = FxHelper.SpawnFxOnPoint(gameObject, hitPoint.point, Quaternion.FromToRotation(Vector3.forward, -hitPoint.normal));
		if (staticHitEffects.Switch == null)
		{
			return;
		}
		AkUnitySoundEngine.SetSwitch(staticHitEffects.Switch.Group, staticHitEffects.Switch.Value, gameObject2);
		AkUnitySoundEngine.SetSwitch("HitMainType", "Normal", gameObject2);
		try
		{
			BlueprintAbilityFXSettings blueprintAbilityFXSettings = projectile.Ability?.FXSettings;
			AkSwitchReference akSwitchReference = blueprintAbilityFXSettings?.ProjectileTypeSwitch;
			if (akSwitchReference != null)
			{
				AkUnitySoundEngine.SetSwitch(akSwitchReference.Group, akSwitchReference.Value, gameObject2);
			}
			AkSwitchReference akSwitchReference2 = blueprintAbilityFXSettings?.MuffledTypeSwitch;
			if (akSwitchReference2 != null)
			{
				AkUnitySoundEngine.SetSwitch(akSwitchReference2.Group, akSwitchReference2.Value, gameObject2);
			}
			if (blueprintAbilityFXSettings != null && blueprintAbilityFXSettings.OverrideDamageType)
			{
				AkSwitchReference damageSoundSwitch = ConfigRoot.Instance.HitSystemRoot.GetDamageSoundSwitch(blueprintAbilityFXSettings.DamageType);
				if (damageSoundSwitch != null)
				{
					AkUnitySoundEngine.SetSwitch(damageSoundSwitch.Group, damageSoundSwitch.Value, gameObject2);
				}
			}
		}
		catch
		{
			PFLog.Default.Error($"{projectile.Ability?.Weapon} don't have sound type switch");
		}
		SoundEventPlayer.PlaySound(ConfigRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkSoundSettings, gameObject2);
	}

	private void SpawnInCircularBuffer(GameObject hitDecalFx, Vector3 position, Quaternion rotation)
	{
		if (m_SpawnedEffectsBuffer.Full)
		{
			Object.Destroy(m_SpawnedEffectsBuffer.PopFront());
		}
		m_SpawnedEffectsBuffer.PushBack(FxHelper.SpawnFxOnPoint(hitDecalFx, position, rotation));
	}

	public void OnAreaBeginUnloading()
	{
		while (!m_SpawnedEffectsBuffer.Empty)
		{
			Object.Destroy(m_SpawnedEffectsBuffer.PopBack());
		}
	}

	public void OnAreaDidLoad()
	{
	}
}
