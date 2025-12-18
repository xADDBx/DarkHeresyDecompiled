using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class IngameMenuSettingsButtonPCView : IngameMenuBasePCView<IngameMenuSettingsButtonVM>
{
	[Header("Buttons Part")]
	[SerializeField]
	private OwlcatMultiButton m_Settings;

	[SerializeField]
	private OwlcatMultiButton m_Pause;

	[SerializeField]
	private List<Image> m_PauseMarks;

	[SerializeField]
	private Sprite m_PauseSprite;

	[SerializeField]
	private Sprite m_UnpauseSprite;

	[SerializeField]
	private OwlcatMultiButton m_NetRoles;

	[SerializeField]
	private Image m_NetRolesAttentionMark;

	private IDisposable m_PauseHintDisposable;

	protected override void OnBind()
	{
		base.OnBind();
		SetButtonsSounds();
		ObservableSubscribeExtensions.Subscribe(m_Settings.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenEscMenu();
		}).AddTo(this);
		m_Settings.SetHint(UIStrings.Instance.MainMenu.Settings, "EscPressed").AddTo(this);
		base.ViewModel.ShowPauseButton.Subscribe(m_Pause.transform.parent.gameObject.SetActive).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Pause.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Pause();
		}).AddTo(this);
		SetButtonPauseSettings(base.ViewModel.IsPause.CurrentValue);
		base.ViewModel.IsPause.Subscribe(SetButtonPauseSettings).AddTo(this);
		base.ViewModel.PlayerHaveRoles.CombineLatest(base.ViewModel.NetFirstLoadState, (bool haveRoles, bool netFirstLoadState) => new { haveRoles, netFirstLoadState }).Subscribe(value =>
		{
			m_NetRoles.transform.parent.gameObject.SetActive(value.netFirstLoadState);
			m_NetRolesAttentionMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NetRoles.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenNetRoles();
		}).AddTo(this);
		m_NetRoles.SetHint(UIStrings.Instance.EscapeMenu.EscMenuRoles).AddTo(this);
		m_NetRolesAttentionMark.SetHint(UIStrings.Instance.NetRolesTexts.YouHaveNoRole).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_PauseHintDisposable?.Dispose();
		m_PauseHintDisposable = null;
	}

	private void SetButtonPauseSettings(bool state)
	{
		m_PauseHintDisposable?.Dispose();
		m_PauseHintDisposable = null;
		m_Pause.SetActiveLayer(state ? "Active" : "Normal");
		m_PauseHintDisposable = m_Pause.SetHint(state ? UIStrings.Instance.CommonTexts.Unpause : UIStrings.Instance.CommonTexts.Pause);
		m_PauseMarks.ForEach(delegate(Image m)
		{
			m.sprite = (state ? m_UnpauseSprite : m_PauseSprite);
		});
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_Settings, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Pause, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_NetRoles, ButtonSoundsEnum.PlastickSound);
	}
}
