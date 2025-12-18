using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[Obsolete]
[TypeId("8da007f57f974091916d05cbcc28116a")]
public class BlueprintUnitsList : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintUnitsList>
	{
	}

	public List<BlueprintUnitReference> Units;
}
