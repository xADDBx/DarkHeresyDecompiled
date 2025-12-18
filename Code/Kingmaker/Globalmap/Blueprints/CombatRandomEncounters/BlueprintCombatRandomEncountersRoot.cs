using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.CombatRandomEncounters;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[Obsolete]
[TypeId("91a515db04514950af7f1ea65ea76ea9")]
public class BlueprintCombatRandomEncountersRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintCombatRandomEncountersRoot>
	{
	}

	public List<CombatRandomEncounterSettings> Settings = new List<CombatRandomEncounterSettings>();
}
