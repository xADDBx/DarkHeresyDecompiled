using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
public class BuffSpecialSettings
{
	[UsedImplicitly]
	public string Comment;

	[Tooltip("Для каких целей происходит проверка условий")]
	public BuffTargetType Targets;

	[Tooltip("При каких условиях показывается в категории важных")]
	public ConditionsChecker Conditions;
}
