using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsModSelectorConsoleView : DlcManagerTabModsModSelectorBaseView
{
	[Header("Console Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerModEntityConsoleView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (m_WidgetList.Entries == null || !m_WidgetList.Entries.Any())
		{
			return;
		}
		foreach (DlcManagerModEntityConsoleView item in m_WidgetList.Entries.OfType<DlcManagerModEntityConsoleView>())
		{
			item.CreateInputImpl(inputLayer, hintsWidget);
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null || !m_WidgetList.Entries.Any())
		{
			return list;
		}
		list.AddRange(m_WidgetList.Entries.OfType<DlcManagerModEntityConsoleView>());
		return list;
	}
}
