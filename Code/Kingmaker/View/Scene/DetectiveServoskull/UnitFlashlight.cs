using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.DayNightCycle;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine;

namespace Kingmaker.View.Scene.DetectiveServoskull;

public class UnitFlashlight : MonoBehaviour
{
	[Serializable]
	public class UnitFlashlightConfig
	{
		public bool OverrideEnabled;

		public bool MainLightEnabled = true;

		public Color MainColor = new Color(1f, 0.9637558f, 0.8537736f, 0.3960784f);

		public float MainIntensity = 20f;

		public float MainRange = 15f;

		public bool MainVolumetricEnabled = true;

		public float MainVolumetricIntensity = 15f;

		[Space]
		public bool LongLightEnabled = true;

		public Color LongColor = new Color(1f, 0.9637558f, 0.8537736f, 0.3960784f);

		public float LongIntensity = 0.2f;

		public float LongRange = 65f;

		[Space]
		public bool PointLightEnabled;

		public Color PointColor = new Color(0.8490566f, 0.7098949f, 0.6367924f, 1f);

		public float PointIntensity = 2f;

		public float PointRange = 8.93f;

		public bool PointVolumetricEnabled = true;

		public float PointVolumetricIntensity = 15f;

		public static UnitFlashlightConfig Default = new UnitFlashlightConfig
		{
			OverrideEnabled = false,
			MainLightEnabled = true,
			MainColor = new Color(1f, 0.9637558f, 0.8537736f, 0.3960784f),
			MainIntensity = 20f,
			MainRange = 15f,
			MainVolumetricEnabled = true,
			MainVolumetricIntensity = 15f,
			LongLightEnabled = true,
			LongColor = new Color(1f, 0.9637558f, 0.8537736f, 0.3960784f),
			LongIntensity = 0.2f,
			LongRange = 65f,
			PointLightEnabled = false,
			PointColor = new Color(0.8490566f, 0.7098949f, 0.6367924f, 1f),
			PointIntensity = 2f,
			PointRange = 8.93f,
			PointVolumetricEnabled = true,
			PointVolumetricIntensity = 15f
		};
	}

	private UnitPartFlashlight m_Part;

	private BaseUnitEntity m_Owner;

	[SerializeField]
	private GameObject m_OnState;

	[SerializeField]
	private GameObject m_OffState;

	[Space]
	[SerializeField]
	private Transform m_FlashlightBone;

	[Space]
	[SerializeField]
	private Light m_MainSpotlight;

	[SerializeField]
	private Light m_LongSpotlight;

	[SerializeField]
	private Light m_PointLight;

	private Vector3 m_LastFlashlightTarget;

	private bool m_IsOn;

	private PartyAwarenessController m_AwarenessController;

	private FlashlightState m_State;

	private Tweener? m_RotatingTween;

	private Tweener? m_ResettingTween;

	private DetectiveServoskullRoot? Settings => ConfigRoot.Instance.DetectiveServoskull;

	private void Awake()
	{
		m_AwarenessController = Game.Instance.GetController<PartyAwarenessController>();
		m_Owner = base.transform.GetComponentInParent<UnitEntityView>()?.EntityData;
		UnitPartFlashlight unitPartFlashlight = m_Owner?.GetOptional<UnitPartFlashlight>();
		if (unitPartFlashlight != null)
		{
			m_Part = unitPartFlashlight;
			bool isFlashlightEnabled = m_Part.IsFlashlightEnabled;
			m_OnState.SetActive(isFlashlightEnabled);
			m_OffState.SetActive(!isFlashlightEnabled);
		}
		else
		{
			m_OnState.SetActive(value: false);
			m_OffState.SetActive(value: true);
			m_IsOn = false;
		}
	}

	private void Update()
	{
		if (m_Part == null)
		{
			return;
		}
		bool isFlashlightEnabled = m_Part.IsFlashlightEnabled;
		if (isFlashlightEnabled != m_IsOn)
		{
			m_OnState.SetActive(isFlashlightEnabled);
			m_OffState.SetActive(!isFlashlightEnabled);
			m_IsOn = isFlashlightEnabled;
			if (m_IsOn)
			{
				LightController.Active?.ApplyFlashlightConfig(this);
			}
		}
		if (!m_IsOn)
		{
			m_State = FlashlightState.NotActive;
			return;
		}
		switch ((FlashlightState)(m_Part.ForcedLookAtPosition.HasValue ? 3 : ((!m_Owner.MovementAgent.IsReallyMoving) ? 1 : 2)))
		{
		case FlashlightState.LookingTowards:
			if (m_State != FlashlightState.LookingTowards)
			{
				PointTowards();
				m_State = FlashlightState.LookingTowards;
			}
			UpdateObjectsAtTargetAndNearOwner(m_FlashlightBone.position + m_FlashlightBone.forward * Settings.FlashlightRadius);
			break;
		case FlashlightState.FollowingCursor:
			PointToTarget(PointerController.WorldPositionForSimulation, lockByVertical: true);
			UpdateObjectsAtTargetAndNearOwner(PointerController.WorldPositionForSimulation);
			m_State = FlashlightState.FollowingCursor;
			break;
		case FlashlightState.ForcedLookAtPosition:
			PointToTarget(m_Part.ForcedLookAtPosition.Value);
			UpdateObjectsAtTargetAndNearOwner(m_Part.ForcedLookAtPosition.Value);
			m_State = FlashlightState.ForcedLookAtPosition;
			break;
		}
	}

	private void UpdateHighlightNearObjects()
	{
		HashSet<MapObjectEntity> hashSet = new HashSet<MapObjectEntity>();
		foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
		{
			if (mapObject.FlashlightOwnerNear)
			{
				mapObject.FlashlightOwnerNear = false;
				hashSet.Add(mapObject);
			}
		}
		foreach (Entity item in EntityBoundsHelper.FindEntitiesInRange(m_Part.Owner.Position, Settings.FlashlightHolderRadius))
		{
			if (item is MapObjectEntity mapObjectEntity && !item.IsInFogOfWar && m_Owner.HasLOS(mapObjectEntity) && !mapObjectEntity.FlashlightOwnerNear)
			{
				mapObjectEntity.FlashlightOwnerNear = true;
				hashSet.Add(mapObjectEntity);
			}
		}
		foreach (MapObjectEntity item2 in hashSet)
		{
			if (item2.FlashlightOwnerNear)
			{
				TryTriggerAwareness(item2);
			}
			item2.View.UpdateHighlight();
		}
	}

	private void PointTowards()
	{
		m_RotatingTween.Kill();
		m_ResettingTween = m_FlashlightBone.DOLocalRotate(Vector3.zero, Settings.FlashlightTargetingTime);
	}

	private void PointToTarget(Vector3 target, bool lockByVertical = false)
	{
		if (m_Part.IsFlashlightEnabled && !(Vector3.Distance(m_LastFlashlightTarget, target) < Settings.FlashlightThreshold))
		{
			m_LastFlashlightTarget = target;
			m_ResettingTween.Kill();
			m_RotatingTween = m_FlashlightBone.DOLookAt(target, Settings.FlashlightTargetingTime, AxisConstraint.Y);
			UpdateObjectsAtTargetAndNearOwner(target);
		}
	}

	private void UpdateObjectsAtTargetAndNearOwner(Vector3 target)
	{
		List<MapObjectEntity> list = new List<MapObjectEntity>();
		foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
		{
			PartAwarenessCheck awarenessCheck = mapObject.AwarenessCheck;
			if (awarenessCheck != null)
			{
				awarenessCheck.IsRevealedByFlashlight = false;
			}
			if (!mapObject.SuppressedByFlashlight)
			{
				mapObject.SuppressedByFlashlight = true;
				list.Add(mapObject);
			}
			if (mapObject.FlashlightOwnerNear)
			{
				mapObject.FlashlightOwnerNear = false;
				list.Add(mapObject);
			}
		}
		List<Entity> list2 = EntityBoundsHelper.FindEntitiesInRange(target, Settings.FlashlightRadius);
		foreach (Entity item in EntityBoundsHelper.FindEntitiesInRange(m_Part.Owner.Position, Settings.FlashlightHolderRadius))
		{
			if (item is MapObjectEntity mapObjectEntity && !item.IsInFogOfWar && m_Owner.HasLOS(mapObjectEntity) && !mapObjectEntity.FlashlightOwnerNear)
			{
				mapObjectEntity.FlashlightOwnerNear = true;
				list.Add(mapObjectEntity);
			}
		}
		foreach (Entity item2 in list2)
		{
			if (item2 is MapObjectEntity mapObjectEntity2 && !item2.IsInFogOfWar && m_Owner.HasLOS(mapObjectEntity2))
			{
				if (mapObjectEntity2.SuppressedByFlashlight)
				{
					mapObjectEntity2.SuppressedByFlashlight = false;
					list.Add(mapObjectEntity2);
				}
				TryTriggerAwareness(mapObjectEntity2);
			}
		}
		foreach (MapObjectEntity item3 in list)
		{
			item3.View.UpdateHighlight();
		}
	}

	private void TryTriggerAwareness(MapObjectEntity mapObject)
	{
		PartAwarenessCheck optional = mapObject.GetOptional<PartAwarenessCheck>();
		if (optional == null || !optional.Settings.HiddenInDarkness)
		{
			return;
		}
		optional.IsRevealedByFlashlight = true;
		AbstractInteractionPart abstractInteractionPart = mapObject.Interactions.FirstOrDefault((AbstractInteractionPart i) => i.Type == InteractionType.Flashlight);
		if (!optional.IsPassed)
		{
			m_AwarenessController.ForceUpdateMapObject(mapObject);
			if (optional.IsPassed && abstractInteractionPart != null)
			{
				UnitCommandsRunner.DirectInteract(m_Owner, abstractInteractionPart);
			}
		}
		else if (optional.IsPassed() && abstractInteractionPart != null && !abstractInteractionPart.AlreadyVisited)
		{
			UnitCommandsRunner.DirectInteract(m_Owner, abstractInteractionPart);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (m_State == FlashlightState.FollowingCursor)
		{
			Gizmos.DrawLine(m_FlashlightBone.position, PointerController.WorldPositionForSimulation);
			Gizmos.DrawWireSphere(PointerController.WorldPositionForSimulation, Settings.FlashlightRadius);
		}
		else if (m_State == FlashlightState.LookingTowards)
		{
			Gizmos.DrawLine(m_FlashlightBone.position, m_FlashlightBone.position + m_FlashlightBone.forward * Settings.FlashlightRadius);
			Gizmos.DrawWireSphere(m_FlashlightBone.position + m_FlashlightBone.forward * Settings.FlashlightRadius, Settings.FlashlightRadius);
		}
	}

	public void ApplyFlashLightConfig(UnitFlashlightConfig flashlightConfig)
	{
		if (!flashlightConfig.OverrideEnabled)
		{
			return;
		}
		m_MainSpotlight.enabled = flashlightConfig.MainLightEnabled;
		if (m_MainSpotlight.enabled)
		{
			m_MainSpotlight.color = flashlightConfig.MainColor;
			m_MainSpotlight.intensity = flashlightConfig.MainIntensity;
			m_MainSpotlight.range = flashlightConfig.MainRange;
			OwlcatAdditionalLightData owlcatAdditionalLightData = m_MainSpotlight.GetOwlcatAdditionalLightData();
			if (owlcatAdditionalLightData != null)
			{
				owlcatAdditionalLightData.VolumetricLighting = flashlightConfig.MainVolumetricEnabled;
				owlcatAdditionalLightData.VolumetricIntensity = flashlightConfig.MainVolumetricIntensity;
			}
		}
		m_LongSpotlight.enabled = flashlightConfig.LongLightEnabled;
		if (m_LongSpotlight.enabled)
		{
			m_LongSpotlight.color = flashlightConfig.LongColor;
			m_LongSpotlight.intensity = flashlightConfig.LongIntensity;
			m_LongSpotlight.range = flashlightConfig.LongRange;
		}
		m_PointLight.enabled = flashlightConfig.PointLightEnabled;
		if (m_PointLight.enabled)
		{
			m_PointLight.color = flashlightConfig.PointColor;
			m_PointLight.intensity = flashlightConfig.PointIntensity;
			m_PointLight.range = flashlightConfig.PointRange;
			OwlcatAdditionalLightData owlcatAdditionalLightData2 = m_PointLight.GetOwlcatAdditionalLightData();
			if (owlcatAdditionalLightData2 != null)
			{
				owlcatAdditionalLightData2.VolumetricLighting = flashlightConfig.PointVolumetricEnabled;
				owlcatAdditionalLightData2.VolumetricIntensity = flashlightConfig.PointVolumetricIntensity;
			}
		}
	}
}
