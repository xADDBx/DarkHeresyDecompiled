using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTriggeredAuto : ITooltipBrick
{
	private readonly string m_TriggeredAutoText;

	private readonly IReadOnlyList<FeatureCountableFlag.FactsList.Element> m_ReasonItems;

	private readonly bool m_IsSuccess;

	public TooltipBrickTriggeredAuto(string triggeredAutoText, IReadOnlyList<FeatureCountableFlag.FactsList.Element> reasonItems, bool isSuccess)
	{
		m_TriggeredAutoText = triggeredAutoText;
		m_ReasonItems = reasonItems;
		m_IsSuccess = isSuccess;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTriggeredAutoVM(m_TriggeredAutoText, m_ReasonItems, m_IsSuccess);
	}
}
