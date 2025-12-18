using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabDlcsDlcSelectorPCView : DlcManagerTabDlcsDlcSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerDlcEntityPCView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}

	public void UpdateDlcEntities()
	{
		List<DlcManagerDlcEntityPCView> list = m_WidgetList.Or(null)?.Entries?.OfType<DlcManagerDlcEntityPCView>().ToList();
		if (list != null && list.Any())
		{
			list.ForEach(delegate(DlcManagerDlcEntityPCView e)
			{
				e.UpdateGrayScale();
			});
		}
	}
}
