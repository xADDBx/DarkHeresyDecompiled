using DG.Tweening;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipPCView : TooltipBaseView
{
	protected override void OnBind()
	{
		if (!base.ViewModel.IsComparative)
		{
			DelayedInvoker.InvokeInTime(delegate
			{
				base.OnBind();
			}, 0.2f).AddTo(this);
		}
		else
		{
			base.OnBind();
		}
	}

	protected override void Show()
	{
		SetWidth();
		DelayedInvoker.InvokeInTime(delegate
		{
			SetHeight();
			if (!base.ViewModel.IsComparative)
			{
				UIUtilityRect.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.OwnerTransform, Vector2.zero, base.ViewModel.PriorityPivots);
				m_ShowTween = base.CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					ModalWindowsSounds.Instance.Tooltip.Show.Play();
					m_IsShowed = true;
				}).SetUpdate(isIndependentUpdate: true);
				base.ViewModel.SetLastWorldPosition(UIUtilityRect.GetWorldCenter((RectTransform)base.transform));
			}
		}, 0.1f).AddTo(this);
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
