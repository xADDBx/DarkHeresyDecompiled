using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.GlobalEffectSystem;

[Serializable]
[TypeId("5dfd91841cd3454598a2df8d2bf404b5")]
public sealed class BlueprintGlobalEffect : BlueprintScriptableObject
{
	public GlobalEffectLink Prefab = new GlobalEffectLink();
}
