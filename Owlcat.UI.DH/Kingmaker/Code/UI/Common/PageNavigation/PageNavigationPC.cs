using System;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.PageNavigation;

public class PageNavigationPC : PageNavigationBase
{
	[Header("PC")]
	[SerializeField]
	private OwlcatMultiButton m_PreviousButton;

	[SerializeField]
	private OwlcatMultiButton m_NextButton;

	public override void Initialize(int pageCount, ReadOnlyReactiveProperty<int> pageIndex, Action<int> setPageIndex, Action prevCallback = null, Action nextCallback = null)
	{
		base.Initialize(pageCount, pageIndex, setPageIndex, prevCallback, nextCallback);
		Disposables.Add(ObservableSubscribeExtensions.Subscribe(m_PreviousButton.OnLeftClickAsObservable(), delegate
		{
			OnPreviousClick();
		}));
		Disposables.Add(ObservableSubscribeExtensions.Subscribe(m_NextButton.OnLeftClickAsObservable(), delegate
		{
			OnNextClick();
		}));
		OnCurrentIndexChanged(pageIndex.CurrentValue);
	}

	protected override void OnCurrentIndexChanged(int index)
	{
		base.OnCurrentIndexChanged(index);
		m_PreviousButton.Interactable = base.HasPrevious;
		m_NextButton.Interactable = base.HasNext;
	}
}
