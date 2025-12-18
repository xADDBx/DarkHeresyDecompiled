using System;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
public class BuffSpecialSettings
{
	[Tooltip("Для каких целей показывается в Special")]
	public BuffTargetType Targets;

	[Tooltip("При каких условиях показывается в Special")]
	public ConditionsChecker Conditions;
}
