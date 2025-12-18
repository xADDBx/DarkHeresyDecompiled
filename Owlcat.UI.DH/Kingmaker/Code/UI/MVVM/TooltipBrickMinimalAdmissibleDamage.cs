using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickMinimalAdmissibleDamage : ITooltipBrick
{
	private readonly int m_MinimalAdmissibleDamage;

	private readonly string m_ReasonValue;

	public TooltipBrickMinimalAdmissibleDamage(int minimalAdmissibleDamage, string reasonValue)
	{
		m_MinimalAdmissibleDamage = minimalAdmissibleDamage;
		m_ReasonValue = reasonValue;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickMinimalAdmissibleDamageVM(m_MinimalAdmissibleDamage, m_ReasonValue);
	}
}
