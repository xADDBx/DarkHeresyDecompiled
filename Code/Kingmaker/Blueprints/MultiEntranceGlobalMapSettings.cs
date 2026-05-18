using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintMultiEntrance))]
[TypeId("ecdea7bed2d90fb48b1baf2893b922aa")]
public class MultiEntranceGlobalMapSettings : BlueprintComponent
{
	public BlueprintMultiEntranceMap CurrentMap;

	[SerializeField]
	public ConditionsChecker CanExitToGunCutter;
}
