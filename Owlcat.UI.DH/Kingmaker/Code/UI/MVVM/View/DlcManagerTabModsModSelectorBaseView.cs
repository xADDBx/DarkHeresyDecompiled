using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class DlcManagerTabModsModSelectorBaseView : View<SelectionGroupRadioVM<DlcManagerModEntityVM>>
{
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;
}
