using Kingmaker.Code.View.Bridge.Root;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenConsoleRootView : View<LoadingScreenRootVM>, IInitializable
{
	[SerializeField]
	private LoadingScreenConsoleView m_LoadingScreenConsoleView;

	public void Initialize()
	{
		m_LoadingScreenConsoleView.Initialize();
	}

	protected override void OnBind()
	{
		base.ViewModel.LoadingScreenVM.Subscribe(m_LoadingScreenConsoleView.Bind).AddTo(this);
	}
}
