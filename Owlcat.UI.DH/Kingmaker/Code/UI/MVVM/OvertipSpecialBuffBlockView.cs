using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipSpecialBuffBlockView : View<UnitBuffBlockVM>
{
	[SerializeField]
	private BuffPCView m_BuffView;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private int m_MaxCount = 2;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private RectTransform m_RectTransform;

	private Tweener m_FadeTween;

	private Tweener m_SizeTween;

	private readonly List<BuffPCView> m_BuffList = new List<BuffPCView>();

	private bool IsVisible
	{
		get
		{
			if (base.ViewModel.MechanicEntityUIState != null && base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue)
			{
				if (base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
				{
					return base.ViewModel.MechanicEntityUIState.IsPlayer.CurrentValue;
				}
				return true;
			}
			return false;
		}
	}

	public void HideInstant()
	{
		m_FadeTween?.Kill();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		DrawBuffs();
		base.ViewModel.Buffs.ObserveAdd().Subscribe(delegate
		{
			DrawBuffs();
		}).AddTo(this);
		base.ViewModel.Buffs.ObserveRemove().Subscribe(delegate
		{
			DrawBuffs();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Buffs.ObserveReset(), delegate
		{
			Clear();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.CheckSpecialComplete, delegate
		{
			DrawBuffs();
		}).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CombineLatest(base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, base.ViewModel.MechanicEntityUIState.IsInCombat, base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead, base.ViewModel.MechanicEntityUIState.IsPlayer, (bool _, bool _, bool _, bool _, bool _) => true).Subscribe(delegate
		{
			DoVisibility();
		}).AddTo(this);
	}

	private void DoVisibility()
	{
		float endValue = (IsVisible ? 1f : 0f);
		m_FadeTween?.Kill();
		m_FadeTween = m_CanvasGroup.DOFade(endValue, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
	}

	private void DrawBuffs()
	{
		base.ViewModel.SortBuffs();
		DrawBuffsInternal(base.ViewModel.Buffs.Where((BuffVM b) => b.IsSpecial).Take(m_MaxCount).ToList(), m_BuffList);
	}

	private void DrawBuffsInternal(List<BuffVM> buffs, List<BuffPCView> views)
	{
		for (int i = 0; i < views.Count; i++)
		{
			BuffVM viewModel = views[i].ViewModel;
			bool flag = false;
			foreach (BuffVM buff in buffs)
			{
				if (buff == viewModel)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				WidgetFactory.DisposeWidget(views[i]);
				views.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < buffs.Count; j++)
		{
			BuffVM buffVM = buffs[j];
			bool flag2 = false;
			foreach (BuffPCView view in views)
			{
				if (view.ViewModel == buffVM)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
				widget.Bind(buffVM);
				widget.transform.SetParent(m_Container, worldPositionStays: false);
				views.Add(widget);
			}
		}
	}

	private void Clear()
	{
		m_BuffList.ForEach(WidgetFactory.DisposeWidget);
		m_BuffList.Clear();
	}
}
