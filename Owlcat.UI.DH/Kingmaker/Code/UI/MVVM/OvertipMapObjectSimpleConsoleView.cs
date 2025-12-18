using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.Attributes;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMapObjectSimpleConsoleView : OvertipMapObjectSimpleView
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	[SerializeField]
	private bool m_NeedHintPositionCorrection;

	[SerializeField]
	[ShowIf("m_NeedHintPositionCorrection")]
	private float m_ConfirmUpperY;

	[SerializeField]
	[ShowIf("m_NeedHintPositionCorrection")]
	private float m_ConfirmLowerY;

	[SerializeField]
	[ShowIf("m_NeedHintPositionCorrection")]
	private float m_PaginatorUpperY;

	[SerializeField]
	[ShowIf("m_NeedHintPositionCorrection")]
	private float m_PaginatorLowerY;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_NeedHintPositionCorrection)
		{
			bool flag = string.IsNullOrEmpty(base.ViewModel.Name.CurrentValue);
			m_OvertipConsoleView.SetConfirmPosition(flag ? m_ConfirmUpperY : m_ConfirmLowerY);
			bool flag2 = string.IsNullOrEmpty(base.ViewModel.ObjectDescription.CurrentValue) || string.IsNullOrEmpty(base.ViewModel.ObjectSkillCheckText.CurrentValue);
			m_OvertipConsoleView.SetPaginatorPosition(flag2 ? m_PaginatorLowerY : m_PaginatorUpperY);
		}
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlighted, UIStrings.Instance.ActionTexts.Interact);
		base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		})
			.AddTo(this);
		m_OvertipConsoleView.AddTo(this);
	}
}
