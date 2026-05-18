using System;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[TypeId("9ed438c677a345df8145e30fef8ecccb")]
public sealed class DetectiveServoskullRoot : BlueprintScriptableObject
{
	[Header("Движение")]
	public float TeleportWhenDistanceToLeaderIsGreaterThan = 30f;

	public float FlyHeight = 0.3f;

	public float ScanFlyHeight = 0.6f;

	public float FlyHeightDeviationSpeed = 0.2f;

	public AnimationCurve FlyingHeightDeviation;

	[Header("Эффекты сканирования")]
	[Tooltip("Как близко к таргету встать для сканирования")]
	public float ScanToTargetDistance = 2.5f;

	[Tooltip("Эффект скана сервочерепа")]
	public PrefabLink ScanFxPrefab;

	[Tooltip("Время стабильного сканирования")]
	public float ScanFxDurationSeconds = 1f;

	[Tooltip("Время затухания сканирования")]
	public float ScanFxFadeSeconds = 0.5f;

	[Space]
	[Tooltip("Эффект, который ставится на скан следов")]
	public PrefabLink FxOnScanTracePrefab;

	[Header("Фонарик")]
	[AkEventReference]
	[Tooltip("Звук включения фонарика")]
	public string FlashlightOnSound;

	[AkEventReference]
	[Tooltip("Звук выключения фонарика")]
	public string FlashlightOffSound;

	[Space]
	[Tooltip("Механический радиус сканирования вокруг курсора")]
	public float FlashlightRadius = 3f;

	[Space]
	[Tooltip("Механический радиус видимости предметов вокруг персонажа с фонариком")]
	public float FlashlightHolderRadius = 2f;

	[Space]
	[Tooltip("Время прихода фонарика в точку")]
	[Range(0.05f, 0.5f)]
	public float FlashlightTargetingTime = 0.15f;

	[Tooltip("Максимальный угол поворота в пол от горизонта")]
	[Range(0f, -90f)]
	public float FlashlightMinVerticalAngle = -15f;

	[Tooltip("Максимальный угол задирания в потолок от горизонта")]
	[Range(0f, 90f)]
	public float FlashlightMaxVerticalAngle = 15f;

	[Range(0f, 1f)]
	public float FlashlightThreshold = 0.3f;

	public float ForcedWalkSpeed = 4.5f;
}
