using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.Code.UI.MVVM;

public class BrickBuffDOTVM : TooltipBrickVM
{
	public readonly Buff Buff;

	public readonly string Damage;

	public BrickBuffDOTVM(Buff buff)
	{
		Buff = buff;
		Damage = ((Buff != null) ? Buff.Rank.ToString() : string.Empty);
	}
}
