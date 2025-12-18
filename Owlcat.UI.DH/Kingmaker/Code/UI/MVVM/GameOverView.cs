using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class GameOverView : View<GameOverVM>
{
	[Header("Common Labels")]
	[SerializeField]
	private TextMeshProUGUI m_ResultText;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionText;

	[Header("Buttons")]
	[SerializeField]
	protected OwlcatMultiButton m_QuickLoadButton;

	[SerializeField]
	protected OwlcatMultiButton m_LoadButton;

	[SerializeField]
	protected OwlcatMultiButton m_MainMenuButton;

	[SerializeField]
	protected OwlcatMultiButton m_IronManDeleteSaveButton;

	[SerializeField]
	protected OwlcatMultiButton m_IronManContinueGameButton;

	[Header("Buttons Labels")]
	[SerializeField]
	private TextMeshProUGUI m_QuickLoadLabel;

	[SerializeField]
	private TextMeshProUGUI m_LoadLabel;

	[SerializeField]
	private TextMeshProUGUI m_MainMenuLabel;

	[SerializeField]
	private TextMeshProUGUI m_IronManDeleteSaveLabel;

	[SerializeField]
	private TextMeshProUGUI m_IronManContinueGameLabel;

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		Observable.Timer(TimeSpan.FromSeconds(3.0), UnityTimeProvider.UpdateIgnoreTimeScale).Subscribe(OnActivate);
		base.ViewModel.Reason.Subscribe(delegate(string value)
		{
			m_ResultText.text = value;
		}).AddTo(this);
		bool isIronMan = base.ViewModel.IsIronMan;
		m_DescriptionText.Or(null)?.gameObject.SetActive(isIronMan && base.ViewModel.HasDowngradedIronManSave);
		if (isIronMan && m_DescriptionText != null && base.ViewModel.HasDowngradedIronManSave)
		{
			m_DescriptionText.text = UIStrings.Instance.GameOverScreen.GameOverIronManDescription;
		}
		SetButtonsLabel();
		SetButtonVisible(isIronMan);
		ObservableSubscribeExtensions.Subscribe(m_QuickLoadButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnQuickLoad();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LoadButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonLoadGame();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MainMenuButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonMainMenu();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_IronManDeleteSaveButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnIronManDeleteSave();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_IronManContinueGameButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnIronManContinueGame();
		}).AddTo(this);
		m_QuickLoadButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnQuickLoad).AddTo(this);
		m_LoadButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnButtonLoadGame).AddTo(this);
		m_MainMenuButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnButtonMainMenu).AddTo(this);
		m_IronManDeleteSaveButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnIronManDeleteSave).AddTo(this);
		m_IronManContinueGameButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnIronManContinueGame).AddTo(this);
		base.ViewModel.CanQuickLoad.Subscribe(m_QuickLoadButton.SetInteractable).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.EscapeMenu);
		});
	}

	protected override void OnUnbind()
	{
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		base.gameObject.SetActive(value: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.EscapeMenu);
		});
	}

	protected virtual void OnActivate()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
	}

	private void SetButtonsLabel()
	{
		if (m_QuickLoadLabel != null)
		{
			m_QuickLoadLabel.text = UIStrings.Instance.GameOverScreen.QuickLoadLabel;
		}
		if (m_LoadLabel != null)
		{
			m_LoadLabel.text = UIStrings.Instance.GameOverScreen.LoadLabel;
		}
		if (m_MainMenuLabel != null)
		{
			m_MainMenuLabel.text = UIStrings.Instance.GameOverScreen.MainMenuLabel;
		}
		if (m_IronManDeleteSaveLabel != null)
		{
			m_IronManDeleteSaveLabel.text = UIStrings.Instance.GameOverScreen.IronManDeleteSaveLabel;
		}
		if (m_IronManContinueGameLabel != null)
		{
			m_IronManContinueGameLabel.text = UIStrings.Instance.GameOverScreen.IronManContinueGameLabel;
		}
	}

	private void SetButtonVisible(bool isIronMan)
	{
		m_QuickLoadButton.Or(null)?.gameObject.SetActive(!isIronMan);
		m_LoadButton.Or(null)?.gameObject.SetActive(!isIronMan);
		m_MainMenuButton.Or(null)?.gameObject.SetActive(!isIronMan || !base.ViewModel.HasDowngradedIronManSave);
		m_IronManDeleteSaveButton.Or(null)?.gameObject.SetActive(isIronMan && base.ViewModel.HasDowngradedIronManSave);
		m_IronManContinueGameButton.Or(null)?.gameObject.SetActive(isIronMan && base.ViewModel.HasDowngradedIronManSave);
	}
}
