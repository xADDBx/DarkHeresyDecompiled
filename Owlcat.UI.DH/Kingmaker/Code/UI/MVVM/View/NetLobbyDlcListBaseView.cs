using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyDlcListBaseView : View<NetLobbyDlcListVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_HostDlcsLabel;

	[SerializeField]
	private TextMeshProUGUI m_HostHasNoDlc;

	[SerializeField]
	protected WidgetList m_DlcsWidgetList;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private List<TextMeshProUGUI> m_PlayersNames;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_HostDlcsLabel.text = UIStrings.Instance.NetLobbyTexts.HostsDlcList;
		m_HostHasNoDlc.text = UIStrings.Instance.NetLobbyTexts.HostHasNoDlc;
		ModalWindowsSounds.Instance.MessageBox.Show.Play();
		SetPlayers();
		ScrollToTop();
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.CloseWindow();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		ModalWindowsSounds.Instance.MessageBox.Hide.Play();
		base.gameObject.SetActive(value: false);
	}

	private void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	private void SetPlayers()
	{
		for (int i = 0; i < m_PlayersNames.Count; i++)
		{
			bool flag = i < base.ViewModel.PlayerNames.Count;
			m_PlayersNames[i].gameObject.SetActive(flag);
			if (flag)
			{
				m_PlayersNames[i].text = base.ViewModel.PlayerNames[i];
			}
		}
		DrawDlcs();
	}

	private void DrawDlcs()
	{
		m_DlcsWidgetList.Clear();
		m_HostHasNoDlc.transform.parent.gameObject.SetActive(!base.ViewModel.Dlcs.Any());
		DrawDlcsImpl();
	}

	protected virtual void DrawDlcsImpl()
	{
	}
}
