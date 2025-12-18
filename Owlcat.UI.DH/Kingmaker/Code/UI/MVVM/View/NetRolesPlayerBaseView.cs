using System;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetRolesPlayerBaseView : View<NetRolesPlayerVM>
{
	[SerializeField]
	private Image m_PlayerAvatar;

	[SerializeField]
	private GameObject m_MeLayer;

	[SerializeField]
	protected GamerTagAndNameBaseView m_GamerTagAndName;

	private IDisposable m_Disposable;

	public GamerTagAndNameBaseView GamerTagAndName => m_GamerTagAndName;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_GamerTagAndName.Bind(base.ViewModel.GamerTagAndNameVM);
		base.ViewModel.IsMe.Subscribe(delegate(bool value)
		{
			m_MeLayer.Or(null)?.SetActive(value);
		}).AddTo(this);
		base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_PlayerAvatar.sprite = value;
		}).AddTo(this);
		base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_Disposable?.Dispose();
			m_Disposable = null;
			m_Disposable = m_PlayerAvatar.SetHint(value);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
		base.gameObject.SetActive(value: false);
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.CurrentValue;
	}
}
