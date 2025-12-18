using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenCustomPortraitCreatorVM : ViewModel
{
	private readonly Action m_OnClose;

	private readonly Action m_OnOpenFolder;

	private readonly Action m_OnRefreshPortrait;

	public readonly ReadOnlyReactiveProperty<PortraitVM> Portrait;

	public CharGenCustomPortraitCreatorVM(ReadOnlyReactiveProperty<PortraitVM> portraitVM, Action onOpenFolder, Action onRefreshPortrait, Action onClose)
	{
		Portrait = portraitVM.Where((PortraitVM p) => (p?.PortraitData?.IsCustom).GetValueOrDefault() && p.PortraitData.EnsureImages()).ToReadOnlyReactiveProperty();
		m_OnOpenFolder = onOpenFolder;
		m_OnRefreshPortrait = onRefreshPortrait;
		m_OnClose = onClose;
	}

	public void OnOpenFolderClick()
	{
		m_OnOpenFolder?.Invoke();
	}

	public void OnRefreshPortraitClick()
	{
		m_OnRefreshPortrait?.Invoke();
	}

	public void OnClose()
	{
		m_OnClose?.Invoke();
	}
}
