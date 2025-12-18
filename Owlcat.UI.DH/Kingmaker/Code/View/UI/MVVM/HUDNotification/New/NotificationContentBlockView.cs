using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationContentBlockView : View<NotificationContentBlockVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_CountLabel;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_HasDescriptionSelectable;

	protected override void OnBind()
	{
		m_Title.text = base.ViewModel.Title;
		bool flag = string.IsNullOrEmpty(base.ViewModel.Title);
		m_CountLabel.gameObject.SetActive(!flag);
		m_Title.gameObject.SetActive(!flag);
		m_Description.text = base.ViewModel.Description;
		m_StateSelectable.SetActiveLayer(base.ViewModel.State.ToString());
		m_HasDescriptionSelectable.SetActiveLayer(string.IsNullOrEmpty(base.ViewModel.Description) ? 1 : 0);
	}
}
