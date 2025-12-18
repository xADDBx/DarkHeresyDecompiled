using System;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class GamerTagAndNameBaseView : View<GamerTagAndNameVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_PLayerName;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		m_PLayerName.text = string.Empty;
		base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_PLayerName.text = value;
			PFLog.Net.Log("GamerTagAndNameBaseView SET NAME " + value);
		}).AddTo(this);
	}

	public void ShowOrHide(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void SetFocus(bool value)
	{
		m_IsFocused.Value = value;
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.gameObject.activeSelf)
		{
			return !string.IsNullOrWhiteSpace(base.ViewModel.Name.CurrentValue);
		}
		return false;
	}

	public void AddGamerTagInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action closeGamersTagModeAction, ReadOnlyReactiveProperty<bool> canConfirmLaunch = null)
	{
		if (canConfirmLaunch != null)
		{
			hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 9, m_IsFocused.And(canConfirmLaunch.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.Back).AddTo(this);
			inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 19, m_IsFocused.And(canConfirmLaunch.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased).AddTo(this);
			hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				base.ViewModel.ShowGamerCard();
			}, 8, m_IsFocused.And(canConfirmLaunch.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.ShowGamerCard).AddTo(this);
		}
		else
		{
			hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 9, m_IsFocused, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.Back).AddTo(this);
			inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 19, m_IsFocused, InputActionEventType.ButtonJustReleased).AddTo(this);
			hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				base.ViewModel.ShowGamerCard();
			}, 8, m_IsFocused, InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.ShowGamerCard).AddTo(this);
		}
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.CurrentValue;
	}
}
