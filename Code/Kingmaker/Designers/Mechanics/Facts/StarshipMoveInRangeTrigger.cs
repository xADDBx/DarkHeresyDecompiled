using System;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4ffdad852e91e7b44b39a84c90cd9978")]
public class StarshipMoveInRangeTrigger : BlueprintComponent
{
	public bool TriggerOnSelf;

	[HideIf("TriggerOnSelf")]
	public int minDistance;

	[HideIf("TriggerOnSelf")]
	public int maxDistance;

	[HideIf("TriggerOnSelf")]
	public bool CheckFaction;

	[SerializeField]
	[ShowIf("CheckFaction")]
	private BlueprintFactionReference m_Faction;

	public ActionList ActionOnSelf;

	public ActionList ActionsOnUnit;

	public BlueprintFaction Faction => m_Faction?.Get();
}
