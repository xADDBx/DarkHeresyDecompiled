using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ContextMenuEntityView : View<ContextMenuEntityVM>, IConfirmClickHandler, IConsoleEntity
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	protected Image m_Icon;

	[SerializeReference]
	protected ContextButtonFx m_ButtonFx;

	protected override void OnBind()
	{
		if (m_ButtonFx != null)
		{
			base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
			{
				if (value)
				{
					ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
					{
						m_ButtonFx.DoBlink();
					});
				}
			}).AddTo(this);
		}
		m_Button.SetActiveLayer(base.ViewModel.IsInteractable ? "Unlocked" : "Locked");
		UISounds.Instance.SetClickSound(m_Button, base.ViewModel.GetClickSoundType());
		UISounds.Instance.SetHoverSound(m_Button, base.ViewModel.GetHoverSoundType());
		base.ViewModel.IsEnabled.Subscribe(m_Button.SetInteractable).AddTo(this);
		base.ViewModel.Title.Subscribe(delegate(string t)
		{
			m_Label.text = t;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			Execute();
		}).AddTo(this);
		if (m_Icon != null)
		{
			if (base.ViewModel.Sprite != null)
			{
				m_Icon.gameObject.SetActive(value: true);
				m_Icon.sprite = base.ViewModel.Sprite;
			}
			else
			{
				m_Icon.gameObject.SetActive(value: false);
			}
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	private void Execute()
	{
		base.ViewModel.Execute();
		ContextMenuHelper.HideContextMenu();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.ViewModel != null)
		{
			return base.ViewModel.IsEnabled.CurrentValue;
		}
		return false;
	}

	public bool CanConfirmClick()
	{
		return m_Button.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		UISounds.Instance.PlayButtonClickSound((int)base.ViewModel.GetClickSoundType());
		base.ViewModel.Execute();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
