using System;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.RandomEncounters.Settings;

[Obsolete]
[TypeId("07b3dc40a077b35439c5004db1e34d0f")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintSpawnableObject : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintSpawnableObject>
	{
	}

	[ValidateNotNull]
	public PrefabLink Prefab;

	public virtual void InitializeObjectView(MapObjectView view)
	{
	}
}
