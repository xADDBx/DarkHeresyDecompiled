using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBuffGroupsView : View<CharInfoBuffGroupsVM>
{
	[SerializeField]
	private BuffsGroupWidget m_CriticalEffects;

	[SerializeField]
	private BuffsGroupWidget m_StatusEffects;

	[SerializeField]
	private DOTGroupWidget m_DOTEffects;

	[Space]
	[SerializeField]
	private CharInfoBookmarkButton m_CriticalEffectsButton;

	[SerializeField]
	private CharInfoBookmarkButton m_StatusEffectsButton;

	[SerializeField]
	private CharInfoBookmarkButton m_DOTEffectsButton;

	[SerializeField]
	private CharInfoBookmarkButton m_NegativeEffectsButton;

	[SerializeField]
	private CharInfoBookmarkButton m_PositiveEffectsButton;

	protected override void OnBind()
	{
		base.ViewModel.BuffBlockVM.CriticalEffects.Subscribe(HandleCriticalEffectsChanged).AddTo(this);
		base.ViewModel.BuffBlockVM.StatusEffects.Subscribe(HandleStatusEffectsChanged).AddTo(this);
		base.ViewModel.BuffBlockVM.DOTEffects.Subscribe(HandleDOTEffectsChanged).AddTo(this);
		base.ViewModel.BuffGroupsVM.NegativeEffects.Subscribe(HandleNegativeEffectsChanged).AddTo(this);
		base.ViewModel.BuffGroupsVM.PositiveEffects.Subscribe(HandlePositiveEffectsChanged).AddTo(this);
		m_CriticalEffectsButton.Button.SetTooltip(base.ViewModel.CriticalEffectsTooltip).AddTo(this);
		m_StatusEffectsButton.Button.SetTooltip(base.ViewModel.StatusEffectsTooltip).AddTo(this);
		m_DOTEffectsButton.Button.SetTooltip(base.ViewModel.DotEffectsTooltip).AddTo(this);
		m_NegativeEffectsButton.Button.SetTooltip(base.ViewModel.NegativeEffectsTooltip).AddTo(this);
		m_PositiveEffectsButton.Button.SetTooltip(base.ViewModel.PositiveEffectsTooltip).AddTo(this);
	}

	private void HandleCriticalEffectsChanged(CriticalEffectsUIData criticalEffects)
	{
		if (SetButtonActive(criticalEffects.Count, m_CriticalEffectsButton))
		{
			string activeLayer = ((criticalEffects.Count > 1) ? $"Multiple_{criticalEffects.HighestRank}" : $"Single_{criticalEffects.HighestRank}");
			m_CriticalEffects.SetActiveLayer(activeLayer);
		}
	}

	private void HandleStatusEffectsChanged(StatusEffectsUIData statusEffects)
	{
		if (SetButtonActive(statusEffects.Count, m_StatusEffectsButton))
		{
			m_StatusEffects.SetCount(statusEffects.Count);
			string activeLayer = statusEffects.HighestSeverity.ToString();
			m_StatusEffects.SetActiveLayer(activeLayer);
		}
	}

	private void HandleDOTEffectsChanged(DOTEffectsUIData dotEffects)
	{
		int count = dotEffects.DotEffects.Count;
		m_DOTEffects.SetEffectsCount(count);
		if (!SetButtonActive(count, m_DOTEffectsButton))
		{
			m_DOTEffects.SetActiveLayerSingle("Disabled");
		}
		else if (count == 1)
		{
			DOT item = dotEffects.DotEffects.First().dotType;
			m_DOTEffects.SetActiveLayerSingle(item.ToString());
		}
		else
		{
			if (count <= 1)
			{
				return;
			}
			int num = 0;
			foreach (var dotEffect in dotEffects.DotEffects)
			{
				DOT item2 = dotEffect.dotType;
				if (num >= m_DOTEffects.MaxEffectsCount)
				{
					break;
				}
				m_DOTEffects.SetActiveLayerMultiple(item2.ToString(), num);
				num++;
			}
		}
	}

	private void HandleNegativeEffectsChanged(IReadOnlyList<Buff> effects)
	{
		HandleEffectsChanged(effects, m_NegativeEffectsButton);
	}

	private void HandlePositiveEffectsChanged(IReadOnlyList<Buff> effects)
	{
		HandleEffectsChanged(effects, m_PositiveEffectsButton);
	}

	private void HandleEffectsChanged(IReadOnlyList<Buff> effects, CharInfoBookmarkButton groupButton)
	{
		groupButton.SetActiveState(effects.Count > 0);
	}

	private bool SetButtonActive(int effectsCount, CharInfoBookmarkButton button)
	{
		bool flag = effectsCount > 0;
		button.SetActiveState(flag);
		return flag;
	}
}
