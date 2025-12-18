using System;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.Blueprints.Facts;

[HashRoot]
[TypeId("bdddf6ca1cd54d888a214ffce2286e39")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public abstract class BlueprintFact : BlueprintScriptableObject
{
	public bool? IsGameStateCache;

	protected abstract Type GetFactType();
}
