using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsPCView : DlcManagerTabSwitchOnDlcsBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private DlcManagerTabSwitchOnDlcsDlcSelectorPCView m_DlcSelectorPCView;

	protected override void OnBind()
	{
		base.OnBind();
		m_DlcSelectorPCView.Bind(base.ViewModel.SelectionGroup);
	}
}
