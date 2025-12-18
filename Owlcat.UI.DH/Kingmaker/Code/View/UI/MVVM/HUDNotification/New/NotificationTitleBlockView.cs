using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationTitleBlockView : View<NotificationTitleBlockVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		m_Title.text = base.ViewModel.Title;
		m_StateSelectable.SetActiveLayer(base.ViewModel.State.ToString());
	}
}
