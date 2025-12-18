using System;
using Kingmaker.Blueprints;
using Newtonsoft.Json;

namespace Kingmaker.Code.Globalmap.Colonization;

[Obsolete]
public class ColonyModifier
{
	[JsonProperty]
	public float Value;

	[JsonProperty]
	public BlueprintScriptableObject Modifier;
}
