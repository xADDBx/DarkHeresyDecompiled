using System;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[Obsolete]
public class SpawnData
{
	[SerializeField]
	private BlueprintUnitsList.Reference m_UnitsList;

	[SerializeField]
	private BlueprintSpawnersList.Reference m_SpawnersList;

	[ValidatePositiveNumber]
	[Tooltip("Units will be spawned from this list starting FROM this round")]
	public int RoundFrom;

	[ValidatePositiveNumber]
	[Tooltip("Units will be spawned from this list until this round met")]
	public int RoundTo;

	[ValidatePositiveNumber]
	[Tooltip("Amount of spawn cycles from this data")]
	public int SpawnAttempts = 1;
}
