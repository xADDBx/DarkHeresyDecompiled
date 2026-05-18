using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotPCView : SaveSlotBaseView
{
	[Header("PC")]
	[SerializeField]
	private Image m_ZoomIcon;

	[Header("Buttons")]
	[SerializeField]
	private bool m_HasButtons = true;

	[SerializeField]
	[ShowIf("m_HasButtons")]
	private TextMeshProUGUI m_SaveLoadLabel;

	[SerializeField]
	[ShowIf("m_HasButtons")]
	private OwlcatMultiButton m_SaveLoadButton;

	[SerializeField]
	[ShowIf("m_HasButtons")]
	private TextMeshProUGUI m_DeleteLabel;

	[SerializeField]
	[ShowIf("m_HasButtons")]
	private OwlcatMultiButton m_DeleteButton;

	private IDisposable m_SaveLoadButtonHintDisposable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_SaveLoadButton != null)
		{
			if (m_HasButtons)
			{
				AddDisposable(ObservableSubscribeExtensions.Subscribe(m_SaveLoadButton.Or(null)?.OnLeftClickAsObservable(), delegate
				{
					base.ViewModel.SaveOrLoad();
				}));
				AddDisposable(ObservableSubscribeExtensions.Subscribe(m_DeleteButton.Or(null)?.OnLeftClickAsObservable(), delegate
				{
					base.ViewModel.Delete();
				}));
			}
			AddDisposable(base.ViewModel.Mode.Subscribe(SetSaveLoadButton));
			if (m_DeleteLabel != null)
			{
				m_DeleteLabel.text = base.Texts?.DeleteLabel;
			}
			SetSaveLoadButton(base.ViewModel.Mode.CurrentValue);
		}
		AddDisposable(base.ViewModel.IsCurrentIronManSave.Subscribe(delegate
		{
			SetInteractableSaveLoadButton();
		}));
		SetInteractableSaveLoadButton();
		if (!m_IsDetailedView && !(this is NewSaveSlotPCView))
		{
			AddDisposable(ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftDoubleClickAsObservable(), delegate
			{
				if (base.ViewModel.ShowSaveLoadButton)
				{
					base.ViewModel.SaveOrLoad();
				}
			}));
		}
		AddDisposable(m_ZoomIcon.OnPointerEnterAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowScreenshot();
		}));
		AddDisposable(m_ZoomIcon.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel.HideScreenshot();
		}));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(OnAvailableChanged));
		OnAvailableChanged(base.ViewModel.IsAvailable.CurrentValue);
	}

	protected override void DestroyViewImplementation()
	{
		m_SaveLoadButtonHintDisposable?.Dispose();
		m_SaveLoadButtonHintDisposable = null;
		m_SaveLoadButton.Or(null)?.SetInteractable(state: false);
		m_DeleteButton.Or(null)?.SetInteractable(state: false);
		if (m_SaveLoadLabel != null)
		{
			m_SaveLoadLabel.text = string.Empty;
		}
		if (m_DeleteLabel != null)
		{
			m_DeleteLabel.text = string.Empty;
		}
		base.DestroyViewImplementation();
	}

	private void OnAvailableChanged(bool isAvailable)
	{
		SetSaveLoadButton(base.ViewModel.Mode.CurrentValue);
		m_DeleteButton.Or(null)?.gameObject.SetActive(isAvailable);
		m_DeleteButton.Or(null)?.SetInteractable(!(base.ViewModel is NewSaveSlotVM));
	}

	private void SetSaveLoadButton(SaveLoadMode mode)
	{
		if (m_SaveLoadLabel != null)
		{
			m_SaveLoadLabel.text = ((mode == SaveLoadMode.Load) ? base.Texts.LoadLabel : base.Texts.SaveLabel);
		}
		m_SaveLoadButton.Or(null)?.gameObject.SetActive(base.ViewModel.ShowSaveLoadButton && base.ViewModel.IsAvailable.CurrentValue);
	}

	protected override void UpdateDLCState(bool b)
	{
		base.UpdateDLCState(b);
		SetInteractableSaveLoadButton();
	}

	private void SetInteractableSaveLoadButton()
	{
		bool interactable = !base.ViewModel.IsCurrentIronManSave.CurrentValue && (!base.ViewModel.ShowDlcRequiredLabel.CurrentValue || base.ViewModel.Mode.CurrentValue == SaveLoadMode.Save);
		m_SaveLoadButton.Or(null)?.SetInteractable(interactable);
		m_SaveLoadButtonHintDisposable?.Dispose();
		if (base.ViewModel.IsCurrentIronManSave.CurrentValue)
		{
			m_SaveLoadButtonHintDisposable = m_SaveLoadButton?.SetHint(UIStrings.Instance.SaveLoadTexts.CannotLoadCurrentIronManSave);
		}
	}
}
