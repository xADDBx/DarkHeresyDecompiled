using Kingmaker.Code.UI.MVVM.View;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBarkPartView : BarkBlockView<UnitBarkPartVM>
{
	[SerializeField]
	private GameObject m_BarkActiveIcon;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsBarkActive.CombineLatest(base.ViewModel.IsUnitOnScreen, (bool barkActive, bool unitOnScreen) => barkActive && !unitOnScreen).Subscribe(delegate(bool value)
		{
			FadeAnimator.PlayAnimation(value);
		}).AddTo(this);
		base.ViewModel.IsBarkActive.Subscribe(delegate(bool value)
		{
			m_BarkActiveIcon.Or(null)?.SetActive(value);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		FadeAnimator.DisappearInstant();
		m_BarkActiveIcon.Or(null)?.SetActive(value: false);
	}

	[ContextMenu("Show Bark")]
	private void EditorShowBark()
	{
		base.ViewModel.ShowBark("This is a test bark!\nThis character has a lot to say indeed, because I need this text to have multiple lines");
	}

	[ContextMenu("Hide Bark")]
	private void EditorHideBark()
	{
		base.ViewModel.HideBark();
	}
}
