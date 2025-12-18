using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CharInfoAlignmentAbilitySlotVM : CharInfoComponentVM
{
	public enum SlotState
	{
		Active,
		Inactive,
		Locked
	}

	private readonly AlignmentAxis m_Direction;

	private readonly int m_ConvictionMark;

	private readonly PartUnitAlignment m_PartUnitAlignment;

	public TooltipBaseTemplate Tooltip { get; private set; }

	public SlotState CurrentSlotState { get; private set; }

	public float Progress { get; private set; }

	public CharInfoAlignmentAbilitySlotVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, AlignmentAxis direction, int mark, PartUnitAlignment partUnitAlignment)
		: base(unit)
	{
		m_Direction = direction;
		m_ConvictionMark = mark;
		m_PartUnitAlignment = partUnitAlignment;
		RefreshData();
	}

	protected override void RefreshData()
	{
		if (m_PartUnitAlignment != null && m_Direction != 0)
		{
			base.RefreshData();
			bool num = !m_PartUnitAlignment.CanHaveMarkInAxis(m_Direction, m_ConvictionMark);
			bool flag = m_ConvictionMark <= m_PartUnitAlignment.GetAlignmentMark(m_Direction);
			int alignmentRank = m_PartUnitAlignment.GetAlignmentRank(m_Direction);
			Tooltip = new TooltipTemplateSoulMarkFeature(Unit.CurrentValue, m_Direction, m_ConvictionMark, null);
			if (num)
			{
				CurrentSlotState = SlotState.Locked;
				return;
			}
			CurrentSlotState = ((!flag) ? SlotState.Inactive : SlotState.Active);
			int rankForMark = ConfigRoot.Instance.AlignmentMarksRoot.GetRankForMark(m_Direction, m_ConvictionMark);
			int num2 = ((m_ConvictionMark != 0) ? ConfigRoot.Instance.AlignmentMarksRoot.GetRankForMark(m_Direction, m_ConvictionMark - 1) : 0);
			Progress = ((m_ConvictionMark == ConfigRoot.Instance.AlignmentMarksRoot.GetMarksAmount(m_Direction) - 1) ? 1f : (1f * (float)(alignmentRank - num2) / (float)(rankForMark - num2)));
		}
	}
}
