using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioSelectorPCView : NewGamePhaseStoryScenarioSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityPCView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}
}
