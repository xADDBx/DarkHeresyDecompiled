using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWeaponDOTInitialDamage : ITooltipBrick
{
	public readonly BlueprintBuff Buff;

	public readonly int InitialDamage;

	public TooltipBrickWeaponDOTInitialDamage(BlueprintBuff buff, int initialDamage)
	{
		Buff = buff;
		InitialDamage = initialDamage;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickWeaponDOTInitialDamageVM(Buff, InitialDamage);
	}
}
