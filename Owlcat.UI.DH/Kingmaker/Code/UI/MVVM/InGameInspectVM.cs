using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Inspect;
using Kingmaker.Mechanics.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InGameInspectVM : InspectVM
{
	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>();

	private readonly InspectReactiveData m_InspectReactiveData = new InspectReactiveData();

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	private UnitInspectInfoByPart m_InspectInfo;

	private IDisposable m_Disposable;

	protected override void HideInspect()
	{
		base.HideInspect();
		m_Disposable?.Dispose();
	}

	protected override void OnUnitInvoke(AbstractUnitEntity entity)
	{
		BaseUnitEntity baseUnitEntity = entity as BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			m_Unit.Value = baseUnitEntity;
			Game.Instance.Player.UISettings.ShowInspect = true;
			m_Disposable?.Dispose();
			m_Disposable = ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
			{
				UpdateInspect(baseUnitEntity);
			});
			UpdateInspect(baseUnitEntity);
			m_Tooltip.Value = new TooltipTemplateUnitInspect(m_Unit, m_InspectReactiveData);
		}
	}

	private void UpdateInspect(BaseUnitEntity unit)
	{
		if (m_UnitUIWrapper.MechanicEntity != unit)
		{
			m_UnitUIWrapper = new MechanicEntityUIWrapper(unit);
			m_InspectInfo = InspectUnitsHelper.GetInfo(unit.BlueprintForInspection, force: true);
		}
		UpdateWounds();
		UpdateDurability();
		UpdateDefence(unit);
		UpdateDamageReduction(unit);
		UpdateMovementPoints(unit);
		UpdateBuffs(unit);
	}

	protected virtual void UpdateWounds()
	{
		if (m_InspectInfo.DefencePart != null && InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue))
		{
			m_InspectReactiveData.WoundsValue.Value = woundsValue;
		}
	}

	protected virtual void UpdateDurability()
	{
		if (m_InspectInfo.DefencePart != null && InspectExtensions.TryGetDurabilityText(m_UnitUIWrapper, out var durabilityValue))
		{
			m_InspectReactiveData.DurabilityValue.Value = durabilityValue;
		}
	}

	protected virtual void UpdateDefence(BaseUnitEntity unit)
	{
		m_InspectReactiveData.DefenceValue.Value = InspectExtensions.GetDefence(unit);
	}

	protected virtual void UpdateDamageReduction(BaseUnitEntity unit)
	{
		m_InspectReactiveData.DamageReductionValue.Value = InspectExtensions.GetDamageReduction(unit);
	}

	protected virtual void UpdateMovementPoints(BaseUnitEntity unit)
	{
		m_InspectReactiveData.MovementPointsValue.Value = InspectExtensions.GetMovementPoints(unit);
	}

	private void UpdateBuffs(BaseUnitEntity unit)
	{
		List<TooltipBrickBuff> buffs = InspectExtensions.GetBuffs(unit);
		bool flag = false;
		if (m_InspectReactiveData.TooltipBrickBuffs.Count != buffs.Count)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < m_InspectReactiveData.TooltipBrickBuffs.Count; i++)
			{
				TooltipBrickBuff obj = m_InspectReactiveData.TooltipBrickBuffs[i] as TooltipBrickBuff;
				TooltipBrickBuff tooltipBrickBuff = buffs[i];
				if (obj?.Buff != tooltipBrickBuff.Buff)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			m_InspectReactiveData.TooltipBrickBuffs.Clear();
			buffs.ForEach(delegate(TooltipBrickBuff buff)
			{
				m_InspectReactiveData.TooltipBrickBuffs.Add(buff);
			});
		}
	}
}
