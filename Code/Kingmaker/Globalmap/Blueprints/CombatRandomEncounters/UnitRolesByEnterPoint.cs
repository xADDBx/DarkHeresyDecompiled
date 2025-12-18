using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[Serializable]
[Obsolete]
public class UnitRolesByEnterPoint
{
	public BlueprintAreaEnterPointReference EnterPoint;

	public UnitRole Roles;
}
