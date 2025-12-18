using Kingmaker.Localization;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.Common;

public class MenuEntityVM : SelectionGroupEntityVM
{
	public readonly LocalizedString Title;

	public readonly int EnumId;

	public MenuEntityVM(LocalizedString title, int enumId, bool allowSwitchOff = false)
		: base(allowSwitchOff)
	{
		Title = title;
		EnumId = enumId;
	}

	public void SetAvailable(bool value)
	{
		m_IsAvailable.Value = value;
	}

	protected override void DoSelectMe()
	{
	}
}
