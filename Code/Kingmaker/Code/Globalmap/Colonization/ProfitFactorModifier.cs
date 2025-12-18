using System;
using Newtonsoft.Json;

namespace Kingmaker.Code.Globalmap.Colonization;

[Obsolete]
public class ProfitFactorModifier : ColonyModifier
{
	[JsonProperty]
	public ProfitFactorModifierType ModifierType;

	public bool IsNegative => Value < 0f;
}
