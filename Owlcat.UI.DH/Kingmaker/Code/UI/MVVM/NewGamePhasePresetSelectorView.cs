using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhasePresetSelectorView : View<SelectionGroupRadioVM<NewGamePhasePresetEntityVM>>
{
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private NewGamePhasePresetEntityView m_Prefab;

	protected override void OnBind()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_Prefab).AddTo(this);
	}
}
