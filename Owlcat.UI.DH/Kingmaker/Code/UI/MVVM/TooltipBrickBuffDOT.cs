using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuffDOT : ITooltipBrick
{
	public readonly Buff Buff;

	public TooltipBrickBuffDOT(Buff buff)
	{
		Buff = buff;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickBuffDOTVM(Buff);
	}
}
