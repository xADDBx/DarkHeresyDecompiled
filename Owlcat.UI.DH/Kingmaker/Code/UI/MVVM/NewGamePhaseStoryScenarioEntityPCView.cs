using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioEntityPCView : NewGamePhaseStoryScenarioEntityBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityIntegralDlcPCView m_ItemPrefab;

	protected override void DrawEntitiesImpl()
	{
		base.DrawEntitiesImpl();
		m_WidgetList.DrawEntries(base.ViewModel.IntegralDlcVms, m_ItemPrefab);
	}
}
