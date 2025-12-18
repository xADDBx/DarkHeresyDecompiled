using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FeedbackPopupPCView : View<FeedbackPopupVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[Header("Items")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private FeedbackPopupItemPCView m_ItemPCView;

	protected override void OnBind()
	{
		m_Title.text = UIStrings.Instance.MainMenu.Feedback;
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		DrawEntities();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Items.ToArray(), m_ItemPCView);
	}
}
