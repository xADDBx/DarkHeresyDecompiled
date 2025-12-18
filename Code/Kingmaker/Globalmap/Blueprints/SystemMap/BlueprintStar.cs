using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[Obsolete]
[TypeId("51eab84063e74c40b129fcfbcf2a58c0")]
public class BlueprintStar : BlueprintStarSystemObject
{
	public enum StarType
	{
		Dwarf,
		Giant,
		Unique
	}

	[Serializable]
	public new class Reference : BlueprintReference<BlueprintStar>
	{
	}

	public StarType Type;
}
