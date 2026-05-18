using System.Collections.Generic;
using DG.Tweening;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipSpecialBuffBlockView : View<OvertipSpecialBuffBlockVM>
{
	[SerializeField]
	private View<BuffVM> m_BuffView;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private RectTransform m_ImportantContainer;

	[SerializeField]
	private RectTransform m_CommonContainer;

	private Tweener m_FadeTween;

	private readonly List<View<BuffVM>> m_PooledViewsImportant = new List<View<BuffVM>>();

	private readonly List<View<BuffVM>> m_PooledViewsCommon = new List<View<BuffVM>>();

	private bool IsVisible
	{
		get
		{
			if (base.ViewModel.EntityUIState != null && base.ViewModel.EntityUIState.IsInCombat.CurrentValue)
			{
				if (base.ViewModel.EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
				{
					return base.ViewModel.EntityUIState.IsPlayer.CurrentValue;
				}
				return true;
			}
			return false;
		}
	}

	public void HideInstant()
	{
		base.gameObject.SetActive(value: false);
		m_FadeTween?.Kill();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		base.ViewModel.ImportantBuffs.Subscribe(HandleImportantBuffsChanged).AddTo(this);
		base.ViewModel.CommonBuffs.Subscribe(HandleCommonBuffsChanged).AddTo(this);
		base.ViewModel.EntityUIState.IsMouseOverUnit.CombineLatest(base.ViewModel.EntityUIState.ForceHotKeyPressed, base.ViewModel.EntityUIState.IsInCombat, base.ViewModel.EntityUIState.IsDeadOrUnconsciousIsDead, base.ViewModel.EntityUIState.IsPlayer, (bool _, bool _, bool _, bool _, bool _) => true).Subscribe(delegate
		{
			DoVisibility();
		}).AddTo(this);
		base.ViewModel.EntityUIState.IsMouseOverUnit.CombineLatest(base.ViewModel.EntityUIState.ForceHotKeyPressed, (bool isHovered, bool isForced) => isHovered || isForced).Subscribe(ShowCommonPart).AddTo(this);
	}

	protected override void OnUnbind()
	{
		ReleaseViews(m_PooledViewsImportant, 0);
		ReleaseViews(m_PooledViewsCommon, 0);
	}

	private void DoVisibility()
	{
		bool isVisible = IsVisible;
		float endValue = (isVisible ? 1f : 0f);
		if (isVisible)
		{
			base.gameObject.SetActive(value: true);
		}
		m_FadeTween?.Kill();
		m_FadeTween = m_CanvasGroup.DOFade(endValue, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true)
			.OnComplete(delegate
			{
				if (!isVisible)
				{
					base.gameObject.SetActive(value: false);
				}
			});
	}

	private void HandleImportantBuffsChanged(IReadOnlyList<BuffVM> buffs)
	{
		HandleBuffsChanged(buffs, m_ImportantContainer, m_PooledViewsImportant);
	}

	private void HandleCommonBuffsChanged(IReadOnlyList<BuffVM> buffs)
	{
		HandleBuffsChanged(buffs, m_CommonContainer, m_PooledViewsCommon);
	}

	private void HandleBuffsChanged(IReadOnlyList<BuffVM> buffs, Transform container, List<View<BuffVM>> list)
	{
		for (int i = 0; i < buffs.Count; i++)
		{
			View<BuffVM> buffView = GetBuffView(i, container, list);
			buffView.Bind(buffs[i]);
			buffView.gameObject.SetActive(value: true);
		}
		ReleaseViews(list, buffs.Count);
	}

	private void ShowCommonPart(bool show)
	{
		m_CommonContainer.gameObject.SetActive(show);
	}

	private View<BuffVM> GetBuffView(int index, Transform container, IList<View<BuffVM>> list)
	{
		if (index >= list.Count)
		{
			View<BuffVM> widget = WidgetFactory.GetWidget(m_BuffView, activate: false, strictMatching: true);
			widget.transform.SetParent(container, worldPositionStays: false);
			list.Add(widget);
		}
		return list[index];
	}

	private void ReleaseViews(List<View<BuffVM>> list, int fromIndex)
	{
		for (int i = fromIndex; i < list.Count; i++)
		{
			WidgetFactory.DisposeWidget(list[i]);
		}
		list.RemoveRange(fromIndex, list.Count - fromIndex);
	}
}
