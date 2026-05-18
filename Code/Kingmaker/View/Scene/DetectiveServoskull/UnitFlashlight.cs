using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.CharacterSystem;
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

	[SerializeField]
	private GameObject m_OnState;

	[SerializeField]
	private GameObject m_OffState;

	[Space]
	[SerializeField]
	private Transform m_RotatingBone;

	[Space]
	[SerializeField]
	private Light m_MainSpotlight;

	[SerializeField]
	private Light m_LongSpotlight;

	[SerializeField]
	private Light m_PointLight;

	private DetectiveServoskullFlashlightController m_FlashlightController;

	private UnitPartFlashlight m_Part;

	private BaseUnitEntity m_Owner;

	private bool m_IsOn;

	private Vector3 m_LastFlashlightTarget;

	private Tweener? m_RotatingTween;

	private Tweener? m_ResettingTween;

	private FlashlightState m_ProcessedState;

	private DetectiveServoskullRoot Settings => ConfigRoot.Instance.DetectiveServoskull;

	private void Awake()
	{
		Character componentInParent = GetComponentInParent<Character>();
		if ((object)componentInParent == null || !componentInParent.DisplayOptions.IsInDollRoom)
		{
			m_FlashlightController = Game.Instance.Controllers.ServoskullFlashlightController;
			m_Owner = base.transform.GetComponentInParent<UnitEntityView>()?.EntityData;
			UnitPartFlashlight unitPartFlashlight = m_Owner?.GetOptional<UnitPartFlashlight>();
			if (unitPartFlashlight != null)
			{
				m_Part = unitPartFlashlight;
				bool isFlashlightEnabled = m_Part.IsFlashlightEnabled;
				m_OnState.SetActive(isFlashlightEnabled);
				m_OffState.SetActive(!isFlashlightEnabled);
				m_FlashlightController.Setup(m_Owner, m_Part, GetForwardPoint);
			}
			else
			{
				m_OnState.SetActive(value: false);
				m_OffState.SetActive(value: true);
				m_IsOn = false;
			}
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
				string flashlightOnSound = Settings.FlashlightOnSound;
				if (!string.IsNullOrEmpty(flashlightOnSound))
				{
					SoundEventsManager.PostEvent(flashlightOnSound, base.gameObject);
				}
			}
			else
			{
				string flashlightOffSound = Settings.FlashlightOffSound;
				if (!string.IsNullOrEmpty(flashlightOffSound))
				{
					SoundEventsManager.PostEvent(flashlightOffSound, base.gameObject);
				}
			}
		}
		if (!m_IsOn)
		{
			m_ProcessedState = FlashlightState.NotActive;
			return;
		}
		switch (m_FlashlightController.State)
		{
		case FlashlightState.LookingTowards:
			if (m_ProcessedState != FlashlightState.LookingTowards)
			{
				PointTowards();
				m_ProcessedState = FlashlightState.LookingTowards;
			}
			break;
		case FlashlightState.FollowingCursor:
			PointToTarget(m_FlashlightController.TargetPoint);
			m_ProcessedState = FlashlightState.FollowingCursor;
			break;
		case FlashlightState.ForcedLookAtPosition:
			PointToTarget(m_FlashlightController.TargetPoint);
			m_ProcessedState = FlashlightState.ForcedLookAtPosition;
			break;
		}
	}

	private Vector3 GetForwardPoint()
	{
		return m_RotatingBone.position + m_RotatingBone.forward * Settings.FlashlightRadius;
	}

	private void PointTowards()
	{
		m_RotatingTween.Kill();
		m_ResettingTween = m_RotatingBone.DOLocalRotate(Vector3.zero, Settings.FlashlightTargetingTime);
	}

	private void PointToTarget(Vector3 target)
	{
		if (m_Part.IsFlashlightEnabled && !(Vector3.Distance(m_LastFlashlightTarget, target) < Settings.FlashlightThreshold))
		{
			m_LastFlashlightTarget = target;
			m_ResettingTween.Kill();
			m_RotatingTween = m_RotatingBone.DOLookAt(target, Settings.FlashlightTargetingTime, AxisConstraint.Y);
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

	private void OnDrawGizmosSelected()
	{
		if (m_Owner != null && m_FlashlightController != null)
		{
			Vector3 targetPoint = m_FlashlightController.TargetPoint;
			Vector3 eyePosition = m_Owner.EyePosition;
			if (m_ProcessedState == FlashlightState.FollowingCursor || m_ProcessedState == FlashlightState.LookingTowards || m_ProcessedState == FlashlightState.ForcedLookAtPosition)
			{
				Gizmos.DrawLine(m_RotatingBone.position, targetPoint);
				Gizmos.DrawWireSphere(targetPoint, Settings.FlashlightRadius);
				DrawWireTriangularPrism(eyePosition, targetPoint, Settings.FlashlightRadius);
			}
		}
	}

	private static void DrawWireTriangularPrism(Vector3 eye, Vector3 target, float radius)
	{
		Vector2 vector = new Vector2(target.x - eye.x, target.z - eye.z);
		float magnitude = vector.magnitude;
		if (!(magnitude < float.Epsilon))
		{
			Vector2 vector2 = vector / magnitude;
			Vector3 vector3 = new Vector3(0f - vector2.y, 0f, vector2.x);
			float y = eye.y - radius;
			float y2 = eye.y + radius;
			Vector3 vector4 = new Vector3(eye.x, y, eye.z);
			Vector3 vector5 = new Vector3(eye.x, y2, eye.z);
			Vector3 vector6 = new Vector3(target.x, y, target.z) + vector3 * radius;
			Vector3 vector7 = new Vector3(target.x, y2, target.z) + vector3 * radius;
			Vector3 vector8 = new Vector3(target.x, y, target.z) - vector3 * radius;
			Vector3 vector9 = new Vector3(target.x, y2, target.z) - vector3 * radius;
			Gizmos.DrawLine(vector4, vector6);
			Gizmos.DrawLine(vector6, vector8);
			Gizmos.DrawLine(vector8, vector4);
			Gizmos.DrawLine(vector5, vector7);
			Gizmos.DrawLine(vector7, vector9);
			Gizmos.DrawLine(vector9, vector5);
			Gizmos.DrawLine(vector4, vector5);
			Gizmos.DrawLine(vector6, vector7);
			Gizmos.DrawLine(vector8, vector9);
		}
	}
}
