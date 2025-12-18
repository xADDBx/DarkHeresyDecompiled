using System;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyPlayerBaseView : View<NetLobbyPlayerVM>
{
	[Space]
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	protected GamerTagAndNameBaseView m_GamerTagAndName;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[SerializeField]
	protected Image m_Plus;

	[SerializeField]
	private Image m_MeLayer;

	[SerializeField]
	protected OwlcatButton m_MainButton;

	[SerializeField]
	protected Image m_ProblemsWithPlayerAndHostDlcsMarker;

	private readonly ReactiveProperty<bool> m_InviteButtonInteractable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_KickButtonInteractable = new ReactiveProperty<bool>();

	private IDisposable m_Disposable;

	public ReadOnlyReactiveProperty<bool> InviteButtonInteractable => m_InviteButtonInteractable;

	public ReadOnlyReactiveProperty<bool> KickButtonInteractable => m_KickButtonInteractable;

	public GamerTagAndNameBaseView GamerTagAndName => m_GamerTagAndName;

	protected override void OnBind()
	{
		m_GamerTagAndName.Bind(base.ViewModel.GamerTagAndNameVM);
		base.ViewModel.IsEmpty.Subscribe(delegate(bool value)
		{
			m_Portrait.gameObject.SetActive(!value);
			if (value)
			{
				m_ProblemsWithPlayerAndHostDlcsMarker.gameObject.SetActive(value: false);
			}
		}).AddTo(this);
		base.ViewModel.IsMeHost.CombineLatest(base.ViewModel.IsEmpty, base.ViewModel.IsMe, (bool host, bool empty, bool me) => new { host, empty, me }).Subscribe(value =>
		{
			m_InviteButtonInteractable.Value = value.empty;
			m_KickButtonInteractable.Value = value.host && !value.empty && !value.me;
			m_Plus.gameObject.SetActive(value.empty);
		}).AddTo(this);
		base.ViewModel.IsActive.Subscribe(delegate(bool value)
		{
			if (!(m_GrayScale == null))
			{
				m_GrayScale.EffectAmount = (value ? 0f : 0.8f);
				m_GrayScale.Alpha = (value ? 1f : 0.5f);
			}
		}).AddTo(this);
		base.ViewModel.IsMe.Subscribe(m_MeLayer.gameObject.SetActive).AddTo(this);
		base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_Portrait.sprite = value;
		}).AddTo(this);
		base.ViewModel.Name.Subscribe(delegate(string value)
		{
			bool flag = !string.IsNullOrWhiteSpace(value);
			m_GamerTagAndName.ShowOrHide(flag);
			m_Disposable?.Dispose();
			m_Disposable = null;
			if (flag)
			{
				m_Disposable = m_Portrait.SetHint(value);
			}
		}).AddTo(this);
		base.ViewModel.PlayersDifferentDlcs.Subscribe(CheckProblemsWithPlayerAndHostDlcs).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.CurrentValue;
	}

	private void CheckProblemsWithPlayerAndHostDlcs(string dlcList)
	{
		if (!(m_ProblemsWithPlayerAndHostDlcsMarker == null))
		{
			bool active = !string.IsNullOrWhiteSpace(dlcList);
			m_ProblemsWithPlayerAndHostDlcsMarker.gameObject.SetActive(active);
			CheckProblemsWithPlayerAndHostDlcsImpl(dlcList);
		}
	}

	protected virtual void CheckProblemsWithPlayerAndHostDlcsImpl(string dlcList)
	{
	}
}
