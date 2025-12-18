using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardBaseView : View<CaseCardVM>
{
	[Header("Views")]
	[SerializeField]
	private CaseCardHeaderView m_Header;

	[SerializeField]
	private CaseCardIconView m_Icon;

	[SerializeField]
	private CaseCardDescriptionView m_Description;

	protected override void OnBind()
	{
		m_Header.Bind(base.ViewModel);
		m_Icon.Bind(base.ViewModel);
		m_Description.Bind(base.ViewModel);
	}
}
