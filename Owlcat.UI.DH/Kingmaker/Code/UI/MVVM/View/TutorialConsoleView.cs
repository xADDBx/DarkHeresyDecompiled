using Kingmaker.ResourceLinks;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TutorialConsoleView : View<TutorialVM>
{
	[SerializeField]
	private UIViewLinkTemp<TutorialBigWindowConsoleView, TutorialModalWindowVM> m_BigWindowView;

	[SerializeField]
	private UIViewLinkTemp<TutorialSmallWindowConsoleView, TutorialHintWindowVM> m_SmallWindowView;

	public void Awake()
	{
		HideAll();
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.SetActive(value: true);
		base.ViewModel.BigWindowVM.Subscribe(m_BigWindowView.Bind).AddTo(this);
		base.ViewModel.SmallWindowVM.Subscribe(m_SmallWindowView.Bind).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		HideAll();
	}

	public void HideAll()
	{
		base.gameObject.SetActive(value: false);
	}
}
