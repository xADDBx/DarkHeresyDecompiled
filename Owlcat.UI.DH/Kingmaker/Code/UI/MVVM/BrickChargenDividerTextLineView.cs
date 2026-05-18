using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenDividerTextLineView : BrickBaseView<BrickChargenDividerTextLineVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private OwlcatMultiSelectable m_TitleTypeSelectable;

	protected override void OnBind()
	{
		m_TitleTypeSelectable.SetActiveLayer(base.ViewModel.DividerType.ToString());
		m_Text.text = base.ViewModel.Text;
	}
}
