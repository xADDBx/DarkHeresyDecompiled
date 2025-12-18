using System;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[Obsolete]
[TypeId("bd36704e28236724fb4887a0e7f3142a")]
public class BlueprintPet : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintPet>
	{
	}
}
