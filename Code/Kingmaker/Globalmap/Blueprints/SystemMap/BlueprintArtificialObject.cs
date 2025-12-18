using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[Obsolete]
[TypeId("56c15ee9d5074f45935dfa2df604fdc7")]
public class BlueprintArtificialObject : BlueprintStarSystemObject
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintArtificialObject>
	{
	}

	public override bool ShouldBeHighlighted => true;
}
