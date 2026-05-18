using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickIconTextView : BrickBaseView<BrickIconTextVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private Image m_IconImage;

	protected override void OnBind()
	{
		m_IconImage.gameObject.SetActive(base.ViewModel.IsShowIcon);
		m_Text.text = base.ViewModel.Text;
	}
}
