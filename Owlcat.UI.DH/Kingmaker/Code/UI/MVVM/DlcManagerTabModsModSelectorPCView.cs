using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsModSelectorPCView : DlcManagerTabModsModSelectorBaseView
{
	[Header("PC Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerModEntityPCView m_ItemPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab).AddTo(this);
	}
}
