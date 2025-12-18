using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioSelectorBaseView : View<SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM>>
{
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;
}
