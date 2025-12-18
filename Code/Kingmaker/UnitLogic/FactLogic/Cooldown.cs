using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9e5d6fea90c0cb9418a322c839a11cf8")]
public class Cooldown : MechanicEntityFactComponentDelegate
{
	[HideIf("UntilEndOfCombat")]
	public int CooldownInRounds;

	public bool UntilEndOfCombat;

	public int GetCooldown()
	{
		if (!UntilEndOfCombat)
		{
			return CooldownInRounds;
		}
		return int.MaxValue;
	}
}
