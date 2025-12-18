using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("cd154b06267b1c74380412abac2289e9")]
public class WarhammerStarshipVantagePoints : BlueprintComponent
{
	[SerializeField]
	[Range(1f, 100f)]
	private int PercentAmongReachableNodes;

	[SerializeField]
	private ActionList ActionsOnEnterVantagePoint;

	[SerializeField]
	private ActionList ActionsOnLeaveVantagePoint;
}
