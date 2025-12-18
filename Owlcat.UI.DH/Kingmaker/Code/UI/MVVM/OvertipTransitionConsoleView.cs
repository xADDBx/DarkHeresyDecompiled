using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.Attributes;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipTransitionConsoleView : OvertipTransitionView
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	[SerializeField]
	private bool m_NeedHintPositionCorrection;

	[SerializeField]
	[ShowIf("m_NeedHintPositionCorrection")]
	private float m_UpperY;

	[SerializeField]
	[ShowIf("m_NeedHintPositionCorrection")]
	private float m_LowerY;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.MapObjectIsHighlighted.Subscribe(delegate(bool value)
		{
			if (value)
			{
				m_Button.OnPointerEnter();
			}
			else
			{
				m_Button.OnPointerExit();
			}
		}).AddTo(this);
		if (m_NeedHintPositionCorrection)
		{
			float confirmPosition = (string.IsNullOrEmpty(base.ViewModel.Title.CurrentValue) ? m_UpperY : m_LowerY);
			m_OvertipConsoleView.SetConfirmPosition(confirmPosition);
		}
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlighted, UIStrings.Instance.ActionTexts.ExitArea);
		base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		})
			.AddTo(this);
		m_OvertipConsoleView.AddTo(this);
	}
}
