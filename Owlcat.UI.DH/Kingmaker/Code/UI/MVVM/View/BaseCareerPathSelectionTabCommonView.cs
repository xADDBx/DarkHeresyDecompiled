using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BaseCareerPathSelectionTabCommonView<TViewModel> : View<TViewModel>, ICareerPathSelectionTabView where TViewModel : ViewModel
{
	[Header("Header")]
	[SerializeField]
	protected GameObject m_HeaderBlock;

	[SerializeField]
	protected TextMeshProUGUI m_HeaderLabel;

	protected readonly ReactiveProperty<string> NextButtonLabel = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<string> BackButtonLabel = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<string> FinishButtonLabel = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<bool> IsTabActiveProp = new ReactiveProperty<bool>();

	protected RectTransform RectTransform;

	protected AccessibilityTextHelper TextHelper;

	public virtual void Initialize()
	{
		Hide();
		RectTransform = GetComponent<RectTransform>();
	}

	protected override void OnBind()
	{
		TextHelper = new AccessibilityTextHelper(m_HeaderLabel).AddTo(this);
		TextHelper.UpdateTextSize();
		Show();
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	protected void Show()
	{
		base.gameObject.SetActive(value: true);
		IsTabActiveProp.Value = true;
	}

	protected void Hide()
	{
		base.gameObject.SetActive(value: false);
		IsTabActiveProp.Value = false;
	}

	public bool IsTabActive()
	{
		return IsTabActiveProp.Value;
	}

	protected void SetHeader(string text)
	{
		if (m_HeaderBlock != null)
		{
			m_HeaderBlock.gameObject.SetActive(!string.IsNullOrEmpty(text));
		}
		if (m_HeaderLabel != null)
		{
			m_HeaderLabel.text = text;
		}
	}

	protected void SetNextButtonLabel(string text)
	{
		NextButtonLabel.Value = text;
	}

	protected void SetBackButtonLabel(string text)
	{
		BackButtonLabel.Value = text;
	}

	protected void SetFinishButtonLabel(string text)
	{
		FinishButtonLabel.Value = text;
	}

	public virtual void UpdateState()
	{
	}

	protected virtual void HandleClickNext()
	{
	}

	protected virtual void HandleClickBack()
	{
	}

	protected virtual void HandleClickFinish()
	{
	}
}
