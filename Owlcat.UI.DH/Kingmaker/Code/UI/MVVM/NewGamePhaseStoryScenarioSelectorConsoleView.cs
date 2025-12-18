using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioSelectorConsoleView : NewGamePhaseStoryScenarioSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityConsoleView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null)
		{
			return list;
		}
		foreach (NewGamePhaseStoryScenarioEntityConsoleView item in m_WidgetList.Entries.OfType<NewGamePhaseStoryScenarioEntityConsoleView>())
		{
			list.Add(item);
			list.AddRange(item.GetNavigationEntities());
		}
		return list;
	}
}
