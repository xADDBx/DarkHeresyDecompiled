using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintArea))]
[TypeId("22e6394dca698394997be7c45bbda818")]
public class TimeSurvival : BlueprintComponent
{
	[ValidateNotNull]
	[SerializeField]
	public List<SpawnData> spawnData;

	public bool UnlimitedTime;

	[HideIf("UnlimitedTime")]
	public int RoundsToSurvive = 5;

	public int RoundsPerSpawn = 2;

	public bool SpawnersShouldFollow = true;

	[SerializeField]
	private BlueprintBuffReference m_StartingBuff;

	public int m_BuffDuration = 1;

	public BlueprintBuff StartingBuff => m_StartingBuff?.Get();
}
