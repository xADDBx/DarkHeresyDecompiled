using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsDlcSelectorConsoleView : DlcManagerTabSwitchOnDlcsDlcSelectorBaseView
{
	[Header("Console Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerSwitchOnDlcEntityConsoleView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null || !m_WidgetList.Entries.Any())
		{
			return list;
		}
		list.AddRange(m_WidgetList.Entries.OfType<DlcManagerSwitchOnDlcEntityConsoleView>());
		return list;
	}
}
