using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSequentialSelectorView : BaseCharGenAppearancePageComponentView<TextureSequentialSelectorVM>, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, IConsoleEntity, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler
{
	[SerializeField]
	private Image m_CurrentValue;

	[SerializeField]
	private OwlcatMultiButton m_ButtonNext;

	[SerializeField]
	private OwlcatMultiButton m_ButtonPrevious;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

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
		base.gameObject.SetActive(value: true);
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_ButtonNext.OnLeftClickAsObservable(), delegate
		{
			OnNextHandler();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_ButtonPrevious.OnLeftClickAsObservable(), delegate
		{
			OnPreviousHandler();
		}));
		AddDisposable(base.ViewModel.Title.Subscribe(SetTitleText));
		AddDisposable(base.ViewModel.Value.Subscribe(SetTexture));
		AddDisposable(base.ViewModel.CurrentIndex.Subscribe(SetCounter));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(base.gameObject.SetActive));
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

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	private void SetTexture(Sprite texture)
	{
		m_CurrentValue.sprite = texture;
	}

	private void SetCounter(int currentIndex)
	{
		if (!(m_Counter == null))
		{
			m_Counter.text = $"{currentIndex + 1} / {base.ViewModel.TotalCount}";
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
	}
}
