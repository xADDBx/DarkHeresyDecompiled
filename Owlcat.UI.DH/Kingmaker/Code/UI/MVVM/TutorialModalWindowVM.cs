using System;
using Kingmaker.Tutorial;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TutorialModalWindowVM : TutorialWindowVM
{
	private readonly ReactiveProperty<int> m_CurrentPageIndex = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TutorialData.Page> m_CurrentPage = new ReactiveProperty<TutorialData.Page>();

	public ReadOnlyReactiveProperty<int> CurrentPageIndex => m_CurrentPageIndex;

	public ReadOnlyReactiveProperty<TutorialData.Page> CurrentPage => m_CurrentPage;

	public int PageCount => base.Pages?.Count ?? 0;

	public bool MultiplePages => PageCount > 1;

	public TutorialModalWindowVM(TutorialData data, Action callbackHide)
		: base(data, callbackHide)
	{
		CurrentPageIndex.Subscribe(delegate(int i)
		{
			m_CurrentPage.Value = base.Pages?[i];
		}).AddTo(this);
	}

	public void SetCurrentPage(int pageIdx)
	{
		m_CurrentPageIndex.Value = pageIdx;
	}
}
