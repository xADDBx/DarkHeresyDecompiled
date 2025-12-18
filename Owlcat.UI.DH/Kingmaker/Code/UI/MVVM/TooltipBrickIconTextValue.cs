using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconTextValue : TooltipBrickCombatLogBase
{
	private readonly string m_Value;

	public TooltipBrickIconTextValue(string name, string value, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false, TooltipBaseTemplate tooltip = null)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground, tooltip)
	{
		m_Value = value;
	}

	public override TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconTextValueVM(m_Name, m_Value, m_NestedLevel, m_IsResultValue, m_ResultValue, m_IsProtectionIcon, m_IsTargetHitIcon, m_IsBorderChanceIcon, m_IsGrayBackground, m_IsBeigeBackground, m_IsRedBackground, m_Tooltip);
	}
}
