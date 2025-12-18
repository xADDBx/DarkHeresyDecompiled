using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabDlcsDlcSelectorBaseView : View<SelectionGroupRadioVM<DlcManagerDlcEntityVM>>
{
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;
}
