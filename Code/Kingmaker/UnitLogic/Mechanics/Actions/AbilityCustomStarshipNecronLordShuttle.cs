using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("f1745a9dceb1b0e40bec6d4e68eecaf0")]
public class AbilityCustomStarshipNecronLordShuttle : BlueprintComponent
{
	private enum ActionMode
	{
		AimingAbility,
		ShuttleAbility
	}

	[SerializeField]
	private BlueprintFeatureReference m_NecronLordFeature;

	[SerializeField]
	private ActionMode m_ActionMode;

	[SerializeField]
	[ShowIf("IsShuttleMode")]
	private BlueprintProjectileReference m_EscapeProjectile;

	[SerializeField]
	private ActionList ActionsOnLaunch;

	[SerializeField]
	[ShowIf("IsShuttleMode")]
	private ActionList ActionsOnArrival;

	public BlueprintFeature NecronLordFeature => m_NecronLordFeature?.Get();

	public BlueprintProjectile EscapeProjectile => m_EscapeProjectile?.Get();

	public bool IsShuttleMode => m_ActionMode == ActionMode.ShuttleAbility;
}
