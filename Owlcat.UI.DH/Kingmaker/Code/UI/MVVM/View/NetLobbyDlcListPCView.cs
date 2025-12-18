using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyDlcListPCView : NetLobbyDlcListBaseView
{
	[SerializeField]
	private NetLobbyDlcListDlcEntityPCView m_DlcEntityPCViewPrefab;

	[SerializeField]
	private OwlcatButton m_DlcListButton;

	[SerializeField]
	private TextMeshProUGUI m_DlcListButtonButtonText;

	protected override void OnBind()
	{
		base.OnBind();
		m_DlcListButtonButtonText.text = UIStrings.Instance.CommonTexts.CloseWindow;
		ObservableSubscribeExtensions.Subscribe(m_DlcListButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.CloseWindow();
		}).AddTo(this);
	}

	protected override void DrawDlcsImpl()
	{
		base.DrawDlcsImpl();
		NetLobbyDlcListDlcEntityVM[] array = base.ViewModel.Dlcs.ToArray();
		if (array.Any())
		{
			m_DlcsWidgetList.DrawEntries(array, m_DlcEntityPCViewPrefab);
		}
	}
}
