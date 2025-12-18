using Kingmaker.ResourceLinks;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RootConsoleView : ViewBase<RootVM>
{
	[SerializeField]
	private UIViewLinkTemp<EpilogBaseView, EpilogVM> m_EpilogConsoleView;

	[SerializeField]
	private UIViewLinkTemp<DialogPCView, DialogVM> m_DialogConsoleView;

	[SerializeField]
	private UIViewLinkTemp<InterchapterBaseView, InterchapterVM> m_InterchapterView;

	[SerializeField]
	private UIViewLinkTemp<SoulMarkRewardBaseView, AlignmentMarkRewardVM> m_SoulMarkReward;

	protected override void BindViewImplementation()
	{
		if (m_DialogConsoleView != null)
		{
			AddDisposable(base.ViewModel.DialogVM.Subscribe(m_DialogConsoleView.Bind));
		}
		if (m_EpilogConsoleView != null)
		{
			AddDisposable(base.ViewModel.EpilogVM.Subscribe(m_EpilogConsoleView.Bind));
		}
		if (m_InterchapterView != null)
		{
			AddDisposable(base.ViewModel.InterchapterVM.Subscribe(m_InterchapterView.Bind));
		}
		if (m_SoulMarkReward != null)
		{
			AddDisposable(base.ViewModel.SoulMarkRewardVM.Subscribe(m_SoulMarkReward.Bind));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
