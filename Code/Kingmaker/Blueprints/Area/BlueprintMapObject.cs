using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Area;

[TypeId("f49127b137bc20547b1ea99ecc628529")]
public abstract class BlueprintMapObject : BlueprintLogicConnector
{
	[CanBeNull]
	[FormerlySerializedAs("PlacedPrefab")]
	public GameObject Prefab;
}
