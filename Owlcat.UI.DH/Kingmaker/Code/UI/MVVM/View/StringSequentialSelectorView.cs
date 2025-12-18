using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class StringSequentialSelectorView : BaseCharGenAppearancePageComponentView<StringSequentialSelectorVM>, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, IConsoleEntity, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler
{
	[SerializeField]
	private TextMeshProUGUI m_CurrentValue;

	[SerializeField]
	private GameObject m_DescriptionObject;

	[SerializeField]
	private TextMeshProUGUI m_CurrentDescription;

	[SerializeField]
	protected OwlcatMultiButton ButtonNext;

	[SerializeField]
	protected OwlcatMultiButton ButtonPrevious;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public bool HandleUp()
	{
		return false;
	}

	public bool HandleDown()
	{
		return false;
	}

	public bool HandleLeft()
	{
		return OnPreviousHandler();
	}

	public bool HandleRight()
	{
		return OnNextHandler();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableSubscribeExtensions.Subscribe(ButtonNext.OnLeftClickAsObservable(), delegate
		{
			OnNextHandler();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(ButtonPrevious.OnLeftClickAsObservable(), delegate
		{
			OnPreviousHandler();
		}));
		AddDisposable(base.ViewModel.Value.Select((string x) => x ?? string.Empty).Subscribe(delegate(string value)
		{
			m_CurrentValue.text = value;
		}));
		AddDisposable(base.ViewModel.SecondaryValue.Subscribe(SetDescriptionText));
		AddDisposable(base.ViewModel.CurrentIndex.Subscribe(SetCounter));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	private void SetDescriptionText(string description)
	{
		if (!(m_CurrentDescription == null))
		{
			m_DescriptionObject.SetActive(!string.IsNullOrEmpty(description));
			m_CurrentDescription.text = description;
		}
	}

	private void SetCounter(int currentIndex)
	{
		if (!(m_Counter == null))
		{
			m_Counter.text = $"{currentIndex + 1} / {base.ViewModel.TotalCount}";
		}
	}

	private bool OnPreviousHandler()
	{
		base.ViewModel.OnLeft();
		return true;
	}

	private bool OnNextHandler()
	{
		base.ViewModel.OnRight();
		return true;
	}
}
