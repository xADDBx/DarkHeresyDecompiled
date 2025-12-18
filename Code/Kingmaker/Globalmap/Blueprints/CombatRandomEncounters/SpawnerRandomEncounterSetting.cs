using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[Serializable]
[Obsolete]
public class SpawnerRandomEncounterSetting
{
	public EntityReference Entity;

	public List<UnitRolesByEnterPoint> RoleVariants = new List<UnitRolesByEnterPoint>();
}
