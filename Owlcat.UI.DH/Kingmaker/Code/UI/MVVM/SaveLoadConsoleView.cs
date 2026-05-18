using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.UI.Common;
using Owlcat.UI;
using Owlcat.UI.Commands;
using Owlcat.UI.Navigation;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class SaveLoadConsoleView : SaveLoadBaseView
{
	[Header("TEMP")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[Header("Hints")]
	[SerializeField]
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[SerializeField]
	private WidgetList m_Hints;

	[SerializeField]
	private Kingmaker.UI.Common.HintView m_HintPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		this.AddNavigation().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
	}

	private bool IsBottomHint(Command command)
	{
		return true;
	}
}
