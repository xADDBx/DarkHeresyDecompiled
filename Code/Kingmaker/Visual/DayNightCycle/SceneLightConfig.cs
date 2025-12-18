using System;
using Kingmaker.ResourceLinks;
using Kingmaker.View.Scene.DetectiveServoskull;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.DayNightCycle;

[CreateAssetMenu(fileName = "SceneLightConfig", menuName = "Techart/Scene Light Config")]
public class SceneLightConfig : ScriptableObject
{
	[Serializable]
	public class Link : WeakResourceLink<SceneLightConfig>, IHashable
	{
		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[Header("Main Light")]
	public Vector3 MainLightRotation = new Vector3(-170.962f, -88.48801f, 80.463f);

	public Color MainLightColor = Color.white;

	public float MainLightIntensity = 1f;

	public float MainLightIndirectIntensity = 1f;

	[Range(0f, 1f)]
	public float MainLightShadowStrength = 0.7f;

	[Header("Contour Light")]
	[Tooltip("Состояние контурного лайта для персонажей. По дефолту должен быть всегда включен, это системная фича, которая должна работать на всех сценах, но могут быть исключения")]
	public bool CameraContourLightEnabled = true;

	[Tooltip("Настройки дирекшенал лайта, что висит на камере. Нужен для контурного освещения персонажей")]
	public float ContourLightIntensity = 4f;

	public Color ContourLightColor = new Color(0.495283f, 0.7442303f, 1f, 1f);

	public static bool CameraContourLightDefaultEnabled = true;

	public static float ContourLightDefaultIntensity = 4f;

	public static Color ContourLightDefaultColor = new Color(0.495283f, 0.7442303f, 1f, 1f);

	[Header("Ambient Colors")]
	public Color SkyAmbientColor = Color.blue;

	public Color EquatorAmbientColor = Color.gray;

	public Color GroundAmbientColor = Color.black;

	[Header("Skybox")]
	public Material SkyboxMaterial;

	public Color SkyboxColor = Color.gray;

	[HideInInspector]
	public Color SkyboxSkyTint = Color.blue;

	[HideInInspector]
	public Color SkyboxGround = Color.gray;

	public float SkyboxExposure = 1f;

	[Range(0f, 360f)]
	public float SkyboxRotation;

	[Header("Fog")]
	public Color FogColor = Color.gray;

	public float FogStartDistance = 25f;

	public float FogEndDistance = 65f;

	[Header("Post Processing")]
	public VolumeProfile PpProfile;

	[Space(10f)]
	[Header("AR Combat Grid Visual Overrides")]
	[Tooltip("Оверрайд материалы для комбатной сетки. Нужно, чтобы чинить ситуации когда сетку не видно из за особенностей арта конкретной зоны")]
	public Material[] ArCombatGridOverrideMaterials;

	[Space(10f)]
	[Header("Flashlight")]
	public UnitFlashlight.UnitFlashlightConfig FlashlightConfig = UnitFlashlight.UnitFlashlightConfig.Default;

	[field: SerializeField]
	[field: Header("Fake Character Light")]
	public Color characterFakeLightColor { get; private set; } = new Color(0f, 0f, 0f);


	[field: SerializeField]
	public float characterFakeLightIntensity { get; private set; } = 1f;

}
