using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationClueBlockView : View<NotificationClueBlockVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		m_Title.text = base.ViewModel.Title;
		m_Description.text = base.ViewModel.Description;
		m_StateSelectable.SetActiveLayer(base.ViewModel.State.ToString());
	}
}
