using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Dialog;

public class DetectiveEpilogCaseItemsView : View<BlueprintCase>
{
	[Header("Views")]
	private CaseCardBaseView m_CaseCardBaseView;

	protected override void OnBind()
	{
		m_CaseCardBaseView.Bind(new CaseCardVM(base.ViewModel));
	}
}
