using System;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[TypeId("fdd33cf2c5394802ae975acfd6de374d")]
public class BlueprintCameraFollowSettings : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintCameraFollowSettings>
	{
	}

	[Header("Combat Group Camera Settings")]
	public CombatCameraFocusSettings PlayerTurnCameraFocusSettings;

	public CombatCameraFocusSettings AllyTurnCameraFocusSettings;

	public CombatCameraFocusSettings EnemyTurnCameraFocusSettings;

	[Header("Combat Camera Focus Settings")]
	[Tooltip("Фокус камеры на начало хода юнита")]
	public CameraFollowTaskParams ToUnitOnStartTurn;

	[Tooltip("Фокус камеры на юните прямо перед исполнением его действия")]
	public CameraFollowTaskParams ToCasterBeforeAction;

	[Tooltip("Фокус камеры на цели")]
	public CameraFollowTaskParams ToTargetOnAttack;

	[Tooltip("Фокус камеры на цели при берст атаке")]
	public CameraFollowTaskParams ToTargetOnBurstAttack;

	[Tooltip("Фокус камеры на умирающем юните")]
	public CameraFollowTaskParams ToTargetOnDeath;

	[Tooltip("Фокус камеры на юните после завершения его действия")]
	public CameraFollowTaskParams ToCasterAfterAction;

	[Tooltip("Фокус камеры при сработавшей опорте")]
	public CameraFollowTaskParams ToCasterOnAttackOfOpportunity;

	[Tooltip("Фокус камеры при невозможности совершить/завершить свой ход (пример оглушении)")]
	public CameraFollowTaskParams ToCasterOnMissedTurn;

	[Tooltip("Фокус камеры при атаке ведущей к победе по морали")]
	public CameraFollowTaskParams ToTargetOnMoraleVictory;
}
