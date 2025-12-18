using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("bb145d4e69427d641a4d60ab345f05da")]
public class WarhammerBoardingBuff : BlueprintComponent
{
	public int BoardingRating;

	public int MoraleDamagePerRound;
}
