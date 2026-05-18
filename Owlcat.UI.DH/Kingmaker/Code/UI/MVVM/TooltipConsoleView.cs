using DG.Tweening;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipConsoleView : TooltipBaseView
{
	[Header("Console")]
	[SerializeField]
	private HintView m_InteractionHint;

	protected override void OnBind()
	{
		base.OnBind();
		SetupInteractionHint();
	}

	protected override void Show()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			SetHeight();
			if (!base.ViewModel.IsComparative)
			{
				UIUtilityRect.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.OwnerTransform, (base.ViewModel.PriorityPivots == null || base.ViewModel.PriorityPivots.Empty()) ? new Vector2(0f, 25f) : Vector2.zero, base.ViewModel.PriorityPivots);
				m_ShowTween = base.CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					ModalWindowsSounds.Instance.Tooltip.Show.Play();
					m_IsShowed = true;
				}).SetUpdate(isIndependentUpdate: true);
			}
		}, 1).AddTo(this);
	}

	private void SetupInteractionHint()
	{
	}

	private void ShowInfo()
	{
		if (base.ViewModel.IsGlossary && base.ViewModel.MainTemplate is TooltipTemplateGlossary { GlossaryEntry: not null } tooltipTemplateGlossary)
		{
			TooltipHelper.ShowGlossaryInfo(tooltipTemplateGlossary);
		}
		else if (base.ViewModel.Templates != null)
		{
			TooltipHelper.ShowInfo(base.ViewModel.Templates);
		}
		else
		{
			TooltipHelper.ShowInfo(base.ViewModel.MainTemplate, base.ViewModel.ShouldNotHideLittleTooltip);
		}
	}

	protected override void OnUnbind()
	{
		if (m_IsShowed)
		{
			ModalWindowsSounds.Instance.Tooltip.Hide.Play();
		}
		m_IsShowed = false;
		base.OnUnbind();
	}
}
