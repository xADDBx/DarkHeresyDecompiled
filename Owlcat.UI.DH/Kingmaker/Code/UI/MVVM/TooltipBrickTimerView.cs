using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTimerView : TooltipBaseBrickView<TooltipBrickTimerVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private GameObject m_TimeIcon;

	protected override void OnBind()
	{
		base.ViewModel.Text.Subscribe(delegate(string t)
		{
			m_Text.text = t;
		}).AddTo(this);
		m_TimeIcon.SetActive(base.ViewModel.ShowTimeIcon);
	}
}
