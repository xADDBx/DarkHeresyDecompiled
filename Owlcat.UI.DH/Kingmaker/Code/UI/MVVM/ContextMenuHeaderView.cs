using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuHeaderView : View<ContextMenuEntityVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	protected TextMeshProUGUI m_SubText;

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel.Title.CurrentValue;
		m_SubText.text = base.ViewModel.SubTitle.CurrentValue;
		m_SubText.gameObject.SetActive(base.ViewModel.SubTitle.CurrentValue != null);
	}
}
