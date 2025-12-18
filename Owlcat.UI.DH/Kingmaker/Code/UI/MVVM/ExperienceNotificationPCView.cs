using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ExperienceNotificationPCView : NotificationPCView<ExperienceNotificationVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.ShowExperienceAmount.Subscribe(delegate
		{
			m_Text.text = $"+{base.ViewModel.ShowExperienceAmount} xp";
		}).AddTo(this);
	}
}
