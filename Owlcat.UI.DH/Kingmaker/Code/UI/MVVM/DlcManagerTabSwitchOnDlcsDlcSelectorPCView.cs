using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsDlcSelectorPCView : DlcManagerTabSwitchOnDlcsDlcSelectorBaseView
{
	[Header("PC Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerSwitchOnDlcEntityPCView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}
}
