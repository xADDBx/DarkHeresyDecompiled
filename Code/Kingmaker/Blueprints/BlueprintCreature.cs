using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Obsolete]
[TypeId("b85796d0b71576a408d4e39fe25e46c4")]
public class BlueprintCreature : BlueprintUnit
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintCreature>
	{
	}
}
