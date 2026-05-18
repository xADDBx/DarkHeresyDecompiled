using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMapObjectInteractionConsoleView : OvertipMapObjectInteractionView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
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
		base.ViewModel.MapObjectIsHighlighted.Or(base.ViewModel.IsMouseOverUI).Subscribe(delegate(bool value)
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
			bool flag = string.IsNullOrEmpty(base.ViewModel.Name.CurrentValue);
			m_OvertipConsoleView.SetConfirmPosition(flag ? m_ConfirmUpperY : m_ConfirmLowerY);
			bool flag2 = string.IsNullOrEmpty(base.ViewModel.ObjectDescription.CurrentValue) && string.IsNullOrEmpty(base.ViewModel.ObjectSkillCheckText.CurrentValue);
			m_OvertipConsoleView.SetPaginatorPosition(flag2 ? m_PaginatorLowerY : m_PaginatorUpperY);
		}
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlighted, GetHintLabel(base.ViewModel.Type));
		base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		})
			.AddTo(this);
		m_OvertipConsoleView.AddTo(this);
	}

	private string GetHintLabel(UIInteractionType type)
	{
		UIActionText actionTexts = UIStrings.Instance.ActionTexts;
		switch (type)
		{
		case UIInteractionType.None:
			return string.Empty;
		case UIInteractionType.Action:
		case UIInteractionType.Credits:
			return actionTexts.Interact;
		case UIInteractionType.Move:
			return actionTexts.Move;
		case UIInteractionType.Info:
		case UIInteractionType.DetectiveTrace:
			return actionTexts.Inspect;
		default:
			return string.Empty;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.ViewModel != null)
		{
			base.ViewModel.MapObjectEntity.View.ForceHighlightExternal(value: true);
			base.ViewModel.SetMouseOverUI(isOverUI: true);
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View.AsMapObjectView(), isHighlighted: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.ViewModel != null)
		{
			base.ViewModel.MapObjectEntity.View.ForceHighlightExternal(value: false);
			base.ViewModel.SetMouseOverUI(isOverUI: false);
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View.AsMapObjectView(), isHighlighted: false);
		}
	}
}
