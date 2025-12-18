using System;
using Newtonsoft.Json;

namespace Kingmaker.Code.Globalmap.Colonization;

[Obsolete]
public class ColonyStatModifier : ColonyModifier
{
	[JsonProperty]
	public ColonyStatModifierType ModifierType;
}
