using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[TypeId("9c0a9fb4ae686c54c8e60a05c89de7c1")]
public class AbilityDeliverStarshipShot : BlueprintComponent
{
	[SerializeField]
	private bool isAE_Mode;

	[SerializeField]
	[Tooltip("AoE pattern used to create target list.\nOriginal target is ignored")]
	[ShowIf("isAE_Mode")]
	private AoEPattern m_Pattern;

	[SerializeField]
	private ActionList ActionsOnProjectileDeliver;

	[SerializeField]
	private ActionList ActionsOnStart;
}
