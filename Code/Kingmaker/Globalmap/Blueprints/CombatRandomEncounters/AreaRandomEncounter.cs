using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[Obsolete]
[AllowedOn(typeof(BlueprintArea))]
[TypeId("d8555cc51bdf418ba08237442f22fc7d")]
public class AreaRandomEncounter : BlueprintComponent
{
	public List<SpawnerRandomEncounterSetting> RandomSpawners = new List<SpawnerRandomEncounterSetting>();

	public List<EntityReference> CoverGroupVariations;

	public List<EntityReference> TrapGroupVariations;

	public List<EntityReference> AreaEffectGroupVariations;

	public List<EntityReference> OtherMapObjectGroupVariations;
}
