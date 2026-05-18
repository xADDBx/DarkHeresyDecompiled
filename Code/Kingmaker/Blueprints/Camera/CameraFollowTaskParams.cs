using System;
using Cinemachine;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[Serializable]
public class CameraFollowTaskParams
{
	[Tooltip("Время, за которое камера будет висеть над целью")]
	public float CameraObserveTime = 1f;

	[Tooltip("Таймскейл")]
	public float TimeScale = 1f;

	[Tooltip("Параметры полёта камеры")]
	public CinemachineBlendDefinition BlendSettings = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.5f);

	[Tooltip("Если точка интереса находится в безопасном прямоугольнике на экране, то камера не будет перемещаться на эту точку")]
	public bool SkipIfOnScreen;

	[ShowIf("SkipIfOnScreen")]
	[Tooltip("Подлёта камеры не будет, но изменение таймскейла отработает")]
	public bool ForceTimescale;
}
