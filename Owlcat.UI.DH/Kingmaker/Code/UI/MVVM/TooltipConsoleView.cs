using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipConsoleView : TooltipBaseView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_InteractionHint;

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
					UISounds.Instance.Sounds.Tooltip.TooltipShow.Play();
					m_IsShowed = true;
				}).SetUpdate(isIndependentUpdate: true);
			}
		}, 1).AddTo(this);
	}

	private void SetupInteractionHint()
	{
		m_InteractionHint.SetActive(base.ViewModel.IsPrimitive);
		if (base.ViewModel.IsPrimitive)
		{
			return;
		}
		InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
		if (base.ViewModel.HasScroll)
		{
			currentInputLayer.AddAxis(base.Scroll, 3).AddTo(this);
		}
		switch (base.ViewModel.InfoCallConsoleMethod)
		{
		case InfoCallConsoleMethod.LongShortRightStickButton:
			m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19)).AddTo(this);
			currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19, InputActionEventType.ButtonJustLongPressed).AddTo(this);
			break;
		case InfoCallConsoleMethod.ShortRightStickButton:
			m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19)).AddTo(this);
			break;
		case InfoCallConsoleMethod.LongRightStickButton:
			m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19, InputActionEventType.ButtonJustLongPressed)).AddTo(this);
			break;
		case InfoCallConsoleMethod.FunkAdditionalButton:
			m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 17)).AddTo(this);
			break;
		default:
			m_InteractionHint.SetActive(state: false);
			break;
		}
		m_InteractionHint.SetLabel(UIStrings.Instance.CommonTexts.Expand);
		m_HintContainer.gameObject.SetActive(value: true);
	}

	private void ShowInfo()
	{
		if (base.ViewModel.IsGlossary && base.ViewModel.MainTemplate is TooltipTemplateGlossary { GlossaryEntry: not null } tooltipTemplateGlossary)
		{
			TooltipHelper.ShowGlossaryInfo(tooltipTemplateGlossary, base.ViewModel.OwnerNavigationBehaviour);
		}
		else if (base.ViewModel.Templates != null)
		{
			TooltipHelper.ShowInfo(base.ViewModel.Templates, base.ViewModel.OwnerNavigationBehaviour);
		}
		else
		{
			TooltipHelper.ShowInfo(base.ViewModel.MainTemplate, base.ViewModel.OwnerNavigationBehaviour, base.ViewModel.ShouldNotHideLittleTooltip);
		}
	}

	protected override void OnUnbind()
	{
		if (m_IsShowed)
		{
			UISounds.Instance.Sounds.Tooltip.TooltipHide.Play();
		}
		m_IsShowed = false;
		base.OnUnbind();
	}
}
