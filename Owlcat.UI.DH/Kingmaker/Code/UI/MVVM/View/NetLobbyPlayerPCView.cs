using System;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyPlayerPCView : NetLobbyPlayerBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_KickButton;

	[SerializeField]
	private Image m_InfoPlayerDlcList;

	private IDisposable m_CurrentPlayerDlcsDisposable;

	private IDisposable m_ProblemsWithPlayerAndHostDlcsDisposable;

	protected override void OnBind()
	{
		base.OnBind();
		base.InviteButtonInteractable.Subscribe(m_MainButton.SetInteractable).AddTo(this);
		base.KickButtonInteractable.Subscribe(m_KickButton.gameObject.SetActive).AddTo(this);
		m_MainButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Invite).AddTo(this);
		m_KickButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Kick).AddTo(this);
		base.ViewModel.PlayerDLcStringList.Subscribe(CheckCurrentPlayerDlcsNamesList).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_CurrentPlayerDlcsDisposable?.Dispose();
		m_CurrentPlayerDlcsDisposable = null;
		m_ProblemsWithPlayerAndHostDlcsDisposable?.Dispose();
		m_ProblemsWithPlayerAndHostDlcsDisposable = null;
		base.OnUnbind();
	}

	private void CheckCurrentPlayerDlcsNamesList(string dlcList)
	{
		if (!(m_InfoPlayerDlcList == null))
		{
			bool flag = !string.IsNullOrWhiteSpace(dlcList);
			m_InfoPlayerDlcList.gameObject.SetActive(flag);
			m_CurrentPlayerDlcsDisposable?.Dispose();
			m_CurrentPlayerDlcsDisposable = null;
			if (flag)
			{
				m_CurrentPlayerDlcsDisposable = m_InfoPlayerDlcList.SetHint(dlcList);
			}
		}
	}

	protected override void CheckProblemsWithPlayerAndHostDlcsImpl(string dlcList)
	{
		base.CheckProblemsWithPlayerAndHostDlcsImpl(dlcList);
		m_ProblemsWithPlayerAndHostDlcsDisposable?.Dispose();
		m_ProblemsWithPlayerAndHostDlcsDisposable = null;
		if (!string.IsNullOrWhiteSpace(dlcList))
		{
			m_ProblemsWithPlayerAndHostDlcsDisposable = m_ProblemsWithPlayerAndHostDlcsMarker.SetHint(UIStrings.Instance.NetLobbyTexts.PlayerHasNoDlcs.Text + ":" + Environment.NewLine + Environment.NewLine + dlcList);
		}
	}
}
