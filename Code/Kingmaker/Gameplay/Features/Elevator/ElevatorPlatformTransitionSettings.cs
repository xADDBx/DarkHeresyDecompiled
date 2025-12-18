using System;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
public sealed class ElevatorPlatformTransitionSettings
{
	[InfoBox("Метры в секунду")]
	[Min(1f)]
	public float MovementSpeed = 3f;

	[InfoBox("Кривая изменения скорости движения: X - прогресс движения к следующему вейпоинту, Y - множитель для MovementSpeed")]
	public AnimationCurve MovementSpeedCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.15f, 1f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f));

	[InfoBox("WhileMoving - платформа поворачивается во время движения; BeforeMove/AfterMove - платформа поворачивается до/после перемещения за время RotationDuration")]
	public ElevatorPlatformRotationType Rotation;

	[InfoBox("Кривая изменения угла поворота: в зависимости от прогресса движения если Rotation=WhileMoving или в зависимости от прогресса по времени RotationDuration в противном случае")]
	public AnimationCurve RotationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	[HideIf("IsWhileMovingRotationType")]
	[InfoBox("Длительность поворота в секундах")]
	public float RotationDuration = 2.5f;

	private bool IsWhileMovingRotationType => Rotation == ElevatorPlatformRotationType.WhileMoving;
}
