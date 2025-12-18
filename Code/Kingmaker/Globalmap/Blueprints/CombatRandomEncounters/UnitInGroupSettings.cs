using System;
using Kingmaker.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[Serializable]
[Obsolete]
public class UnitInGroupSettings
{
	public BlueprintUnitReference Unit;

	[FormerlySerializedAs("Count")]
	public int UnitsCount;

	[SerializeField]
	public bool IsMandatoryInGroup;
}
