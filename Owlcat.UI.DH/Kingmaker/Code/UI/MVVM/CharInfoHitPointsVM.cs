using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoHitPointsVM : CharInfoComponentVM
{
	private readonly ReactiveProperty<int> m_CurrentHp = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_MaxHP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentArmor = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_MaxArmor = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ArmorTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_WoundsTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_DefenceTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveCommand<Unit> m_Refresh = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<int> CurrentHp => m_CurrentHp;

	public ReadOnlyReactiveProperty<int> MaxHP => m_MaxHP;

	public ReadOnlyReactiveProperty<int> CurrentArmor => m_CurrentArmor;

	public ReadOnlyReactiveProperty<int> MaxArmor => m_MaxArmor;

	public Observable<Unit> Refresh => m_Refresh;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> WoundsTooltip => m_WoundsTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> ArmorTooltip => m_ArmorTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> DefenceTooltip => m_DefenceTooltip;

	private PartHealth Health => UnitUIWrapper.Health;

	private PartArmor Armor => UnitUIWrapper.Armor;

	public CharInfoHitPointsVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			UpdateValues();
		});
		UpdateTooltip();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateTooltip();
		UpdateValues();
		m_Refresh?.Execute();
	}

	private void UpdateTooltip()
	{
		if (Unit.CurrentValue != null)
		{
			ModifiableValue modifiableValue = UnitUIWrapper.Stats?.GetStat(StatType.HitPoints);
			if (modifiableValue != null)
			{
				m_WoundsTooltip.Value = new TooltipTemplateStat(new StatTooltipData(modifiableValue));
			}
			ModifiableValue modifiableValue2 = UnitUIWrapper.Stats?.GetStat(StatType.ArmorDurability);
			if (modifiableValue2 != null)
			{
				m_ArmorTooltip.Value = new TooltipTemplateStat(new StatTooltipData(modifiableValue2));
			}
			ModifiableValue modifiableValue3 = UnitUIWrapper.Stats?.GetStat(StatType.Defence);
			if (modifiableValue3 != null)
			{
				m_DefenceTooltip.Value = new TooltipTemplateStat(new StatTooltipData(modifiableValue3));
			}
		}
	}

	protected virtual void UpdateValues()
	{
		if (Unit.CurrentValue == null || Unit.CurrentValue.IsDisposed)
		{
			return;
		}
		PartLifeState lifeState = UnitUIWrapper.LifeState;
		if (Health != null && lifeState != null)
		{
			if (Unit.CurrentValue.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
			{
				m_CurrentHp.Value = 1;
				m_MaxHP.Value = 1;
				m_CurrentArmor.Value = 1;
				m_MaxArmor.Value = 1;
			}
			else
			{
				m_CurrentHp.Value = Health.HitPointsLeft;
				m_MaxHP.Value = Health.MaxHitPoints;
				m_CurrentArmor.Value = Armor?.DurabilityLeft ?? 0;
				ReactiveProperty<int> maxArmor = m_MaxArmor;
				ModifiableValue modifiableValue = Armor?.Durability;
				maxArmor.Value = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
			}
		}
	}
}
