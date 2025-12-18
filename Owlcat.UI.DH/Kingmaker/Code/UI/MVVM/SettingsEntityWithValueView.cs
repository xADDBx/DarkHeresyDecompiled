using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SettingsEntityWithValueView<TSettingsEntityVM> : SettingsEntityView<TSettingsEntityVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IConsoleNavigationEntity, IConsoleEntity, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler where TSettingsEntityVM : SettingsEntityWithValueVM
{
	[SerializeField]
	private Image m_HighlightedImage;

	[SerializeField]
	private Color NormalColor = Color.clear;

	[SerializeField]
	private Color OddColor = new Color(0.77f, 0.75f, 0.69f, 0.29f);

	[SerializeField]
	private Color HighlightedColor = new Color(0.52f, 0.52f, 0.52f, 0.29f);

	[SerializeField]
	private Image m_PointImage;

	[SerializeField]
	private Image m_MarkImage;

	private string m_ModificationNotAllowedReason;

	private IDisposable m_Disposable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.ModificationAllowed.Subscribe(delegate(bool value)
		{
			OnModificationChanged(base.ViewModel.ModificationAllowedReason.CurrentValue, value);
		}));
		AddDisposable(base.ViewModel.IsChanged.Subscribe(UpdatePoints));
		OnModificationChanged(base.ViewModel.ModificationAllowedReason.CurrentValue, base.ViewModel.ModificationAllowed.CurrentValue);
		SetupColor(isHighlighted: false);
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
		base.DestroyViewImplementation();
	}

	private void UpdatePoints(bool isChanged)
	{
		if (m_PointImage != null)
		{
			m_PointImage.gameObject.SetActive((!isChanged && !base.ViewModel.IsSet) || base.ViewModel.HideMarkImage);
		}
		m_MarkImage.Or(null)?.gameObject.SetActive(isChanged && !base.ViewModel.HideMarkImage);
	}

	public void SetupColor(bool isHighlighted)
	{
		Color color = (base.ViewModel.IsOdd ? OddColor : NormalColor);
		if (m_HighlightedImage != null)
		{
			m_HighlightedImage.color = (isHighlighted ? HighlightedColor : color);
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
		{
			h.HandleShowSettingsDescription(base.ViewModel.UISettingsEntity);
		});
		SetupColor(isHighlighted: true);
	}

	protected void SubscribeNotAllowedSelectable(MonoBehaviour selectable)
	{
		AddDisposable(selectable.OnPointerClickAsObservable().Subscribe(delegate
		{
			CallNotAllowedNotification(m_ModificationNotAllowedReason, WarningNotificationFormat.Attention);
		}));
	}

	private void CallNotAllowedNotification(string reason, WarningNotificationFormat format)
	{
		if (!base.ViewModel.ModificationAllowed.CurrentValue && !string.IsNullOrWhiteSpace(reason))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(reason, addToLog: true, format);
			});
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetupColor(isHighlighted: false);
	}

	public virtual void OnModificationChanged(string reason, bool allowed = true)
	{
		m_ModificationNotAllowedReason = reason;
	}

	protected void SetNotAllowedModificationHint(MonoBehaviour selectable)
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
		string modificationNotAllowedReason = m_ModificationNotAllowedReason;
		bool currentValue = base.ViewModel.ModificationAllowed.CurrentValue;
		string text = modificationNotAllowedReason ?? string.Empty;
		bool shouldShow = !currentValue && !string.IsNullOrWhiteSpace(modificationNotAllowedReason);
		m_Disposable = selectable.SetHint(text, null, default(Color), shouldShow);
	}

	public virtual void SetFocus(bool value)
	{
		SetupColor(value);
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(base.ViewModel.UISettingsEntity);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleHideSettingsDescription();
			});
		}
	}

	public bool IsValid()
	{
		return true;
	}

	public abstract bool HandleLeft();

	public abstract bool HandleRight();
}
