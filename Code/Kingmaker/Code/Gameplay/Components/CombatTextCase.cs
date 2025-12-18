using System;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
public class CombatTextCase
{
	[Tooltip("Для каких целей применяется")]
	public CombatTextTargetType Targets = CombatTextTargetType.Enemy;

	[Tooltip("Для каких событий применяется")]
	public CombatTextEventType Events = CombatTextEventType.OnAttach;

	public CombatTextCaseSettings Settings = new CombatTextCaseSettings();
}
