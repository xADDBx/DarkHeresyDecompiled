using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Root;

[Obsolete]
[TypeId("474c06b76471f274fb4ca7b1624c86ce")]
public class BlueprintProfitFactorRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintProfitFactorRoot>
	{
	}

	public float InitialProfitFactor = 30f;
}
