using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[Obsolete]
[TypeId("20257e9e386b4d73a2b6fe68f51bab0e")]
public class BlueprintRandomGroupOfUnits : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintRandomGroupOfUnits>
	{
	}

	public UnitInGroupSettings[] Units;

	public int MinCount;

	public int MaxCount;
}
