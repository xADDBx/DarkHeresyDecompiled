using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsDlcSelectorBaseView : View<SelectionGroupRadioVM<DlcManagerSwitchOnDlcEntityVM>>
{
	[SerializeField]
	protected WidgetList m_WidgetList;
}
