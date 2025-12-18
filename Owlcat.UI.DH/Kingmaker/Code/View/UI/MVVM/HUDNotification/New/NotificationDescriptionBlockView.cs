using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationDescriptionBlockView : View<NotificationDescriptionBlockVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Description;

	protected override void OnBind()
	{
		m_Description.text = base.ViewModel.Description;
	}
}
