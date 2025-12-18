using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[Obsolete]
[TypeId("585edc54e62446a58844f30e5d2f1492")]
public class BlueprintSpawnersList : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintSpawnersList>
	{
	}

	public List<EntityReference> SpawnersPool;
}
