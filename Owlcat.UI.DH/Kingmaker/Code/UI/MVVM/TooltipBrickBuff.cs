using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuff : ITooltipBrick
{
	public readonly Buff Buff;

	public readonly BuffGroupType Group;

	public TooltipBrickBuff(Buff buff, BuffGroupType group)
	{
		Buff = buff;
		Group = group;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickBuffVM(Buff);
	}
}
