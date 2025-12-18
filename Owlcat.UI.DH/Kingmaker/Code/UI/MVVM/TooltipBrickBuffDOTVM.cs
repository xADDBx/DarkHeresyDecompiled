using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuffDOTVM : TooltipBaseBrickVM
{
	public string Damage;

	private readonly Buff m_Buff;

	public TooltipBrickBuffDOTVM(Buff buff)
	{
		m_Buff = buff;
		Damage = ((m_Buff != null) ? m_Buff.Rank.ToString() : string.Empty);
	}
}
