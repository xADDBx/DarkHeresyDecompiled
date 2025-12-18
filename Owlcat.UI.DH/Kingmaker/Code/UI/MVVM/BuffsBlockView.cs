using System.Linq;
using Code.Enums;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BuffsBlockView : View<UnitBuffBlockVM>
{
	[SerializeField]
	private BuffsGroupWidget m_StatusEffect;

	[SerializeField]
	private BuffsGroupWidget m_CriticalEffect;

	[SerializeField]
	private DOTGroupWidget m_DotEffect;

	protected override void OnBind()
	{
		base.ViewModel.StatusEffects.Subscribe(HandleStatusEffectsChanged).AddTo(this);
		base.ViewModel.CriticalEffects.Subscribe(HandleCriticalEffectsChanged).AddTo(this);
		base.ViewModel.DOTEffects.Subscribe(HandleDOTEffectsChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		SetActive(isActive: false);
	}

	protected virtual void HandleCriticalEffectsChanged(CriticalEffectsUIData data)
	{
		UpdateCriticalEffects(data);
		UpdateVisibility();
	}

	protected virtual void HandleStatusEffectsChanged(StatusEffectsUIData data)
	{
		UpdateStatusEffects(data);
		UpdateVisibility();
	}

	protected virtual void HandleDOTEffectsChanged(DOTEffectsUIData data)
	{
		UpdateDOTEffects(data);
		UpdateVisibility();
	}

	private void UpdateCriticalEffects(CriticalEffectsUIData data)
	{
		bool flag = data.HighestRank > 0;
		m_CriticalEffect.SetActive(flag);
		if (flag)
		{
			m_CriticalEffect.SetCount(data.Count);
			string activeLayer = ((data.Count > 1) ? $"Multiple_{data.HighestRank}" : $"Single_{data.HighestRank}");
			m_CriticalEffect.SetActiveLayer(activeLayer);
		}
	}

	private void UpdateStatusEffects(StatusEffectsUIData data)
	{
		bool flag = data.Count > 0;
		m_StatusEffect.SetActive(flag);
		if (flag)
		{
			string activeLayer = data.HighestSeverity.ToString();
			m_StatusEffect.SetCount(data.Count);
			m_StatusEffect.SetActiveLayer(activeLayer);
		}
	}

	private void UpdateDOTEffects(DOTEffectsUIData data)
	{
		int count = data.DotEffects.Count;
		bool flag = count > 0;
		m_DotEffect.SetActive(flag);
		if (!flag)
		{
			return;
		}
		bool num = count == 1;
		bool flag2 = count > 1;
		m_DotEffect.SetEffectsCount(count);
		if (num)
		{
			DOT item = data.DotEffects.First().dotType;
			m_DotEffect.SetActiveLayerSingle(item.ToString());
		}
		if (!flag2)
		{
			return;
		}
		int num2 = 0;
		foreach (var dotEffect in data.DotEffects)
		{
			DOT item2 = dotEffect.dotType;
			if (num2 >= m_DotEffect.MaxEffectsCount)
			{
				break;
			}
			m_DotEffect.SetActiveLayerMultiple(item2.ToString(), num2);
			num2++;
		}
	}

	private void UpdateVisibility()
	{
		bool flag = base.ViewModel.CriticalEffects.CurrentValue.Count > 0;
		bool flag2 = base.ViewModel.DOTEffects.CurrentValue.DotEffects.Count > 0;
		bool flag3 = base.ViewModel.StatusEffects.CurrentValue.Count > 0;
		SetActive(flag || flag2 || flag3);
	}

	private void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}
}
