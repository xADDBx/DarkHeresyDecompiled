using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenStatTitleView : BrickBaseView<BrickChargenStatTitleVM>
{
	[SerializeField]
	private TMP_Text m_DisplayName;

	[SerializeField]
	private TMP_Text m_Acronym;

	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	private TMP_Text m_Subname;

	protected override void OnBind()
	{
		m_DisplayName.text = base.ViewModel.DisplayName;
		m_Subname.text = base.ViewModel.Subname;
		m_Acronym.text = base.ViewModel.Acronym;
		m_Value.text = base.ViewModel.Value;
	}
}
