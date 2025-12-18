using Kingmaker.Code.View.Bridge.Root;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenPCRootView : View<LoadingScreenRootVM>, IInitializable
{
	[SerializeField]
	private LoadingScreenPCView m_LoadingScreenPCView;

	public void Initialize()
	{
		m_LoadingScreenPCView.Initialize();
	}

	protected override void OnBind()
	{
		base.ViewModel.LoadingScreenVM.Subscribe(m_LoadingScreenPCView.Bind).AddTo(this);
	}
}
