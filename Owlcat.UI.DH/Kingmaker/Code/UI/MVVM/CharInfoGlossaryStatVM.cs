using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoGlossaryStatVM : CharInfoComponentVM
{
	public readonly StatType Stat;

	private readonly ReactiveProperty<int> m_StatValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ReadOnlyReactiveProperty<int> StatValue => m_StatValue;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	public CharInfoGlossaryStatVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, StatType stat)
		: base(unit)
	{
		Stat = stat;
		UpdateStat();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateStat();
	}

	private void UpdateStat()
	{
		if (Stat != 0)
		{
			ModifiableValue stat = Unit.CurrentValue.Stats.GetStat(Stat);
			if (stat != null)
			{
				m_StatValue.Value = stat.ModifiedValue;
				m_Tooltip.Value = new TooltipTemplateStat(new StatTooltipData(stat));
			}
		}
	}
}
