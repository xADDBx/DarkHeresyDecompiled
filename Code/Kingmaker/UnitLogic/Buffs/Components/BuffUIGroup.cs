using System;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
public class BuffUIGroup
{
	[Tooltip("Для каких целей применяется")]
	public BuffTargetType Targets;

	[Tooltip("В какую группу оверрайдится")]
	public BuffGroupType Group;
}
