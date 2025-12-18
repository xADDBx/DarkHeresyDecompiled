using System;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[Obsolete]
[TypeId("b6e5b459369e4d53834ea75b97160603")]
public class BlueprintPlanetPrefab : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintPlanetPrefab>
	{
	}

	[SerializeField]
	private PrefabLink m_Prefab;

	public PrefabLink PrefabLink => m_Prefab;
}
