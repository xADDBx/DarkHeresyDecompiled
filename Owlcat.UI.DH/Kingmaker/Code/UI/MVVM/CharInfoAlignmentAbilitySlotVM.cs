using System;
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

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<SlotState> m_CurrentSlotState = new ReactiveProperty<SlotState>();

	private readonly ReactiveProperty<float> m_Progress = new ReactiveProperty<float>();

	private readonly AlignmentAxis m_Direction;

	private readonly int m_AlignmentTier;

	private readonly PartUnitAlignment m_PartUnitAlignment;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	public ReadOnlyReactiveProperty<SlotState> CurrentSlotState => m_CurrentSlotState;

	public ReadOnlyReactiveProperty<float> Progress => m_Progress;

	public CharInfoAlignmentAbilitySlotVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, AlignmentAxis direction, int mark, PartUnitAlignment partUnitAlignment)
		: base(unit)
	{
		m_Direction = direction;
		m_AlignmentTier = mark;
		m_PartUnitAlignment = partUnitAlignment;
		RefreshData();
	}

	protected override void RefreshData()
	{
		if (m_PartUnitAlignment != null && m_Direction != 0)
		{
			base.RefreshData();
			BlueprintAlignmentMarksRoot alignmentMarksRoot = ConfigRoot.Instance.AlignmentMarksRoot;
			ReasonCannotHaveMark reason;
			bool num = !m_PartUnitAlignment.CanHaveMarkInAxis(m_Direction, m_AlignmentTier, out reason);
			bool flag = m_AlignmentTier <= m_PartUnitAlignment.GetAlignmentMark(m_Direction);
			int alignmentRank = m_PartUnitAlignment.GetAlignmentRank(m_Direction);
			m_Tooltip.Value = new TooltipTemplateAlignmentFeature(Unit.CurrentValue, m_Direction, m_AlignmentTier);
			if (num)
			{
				m_CurrentSlotState.Value = SlotState.Locked;
				return;
			}
			m_CurrentSlotState.Value = ((!flag) ? SlotState.Inactive : SlotState.Active);
			int mark = Math.Min(m_AlignmentTier + 1, alignmentMarksRoot.GetMarksAmount(m_Direction));
			int mark2 = Math.Max(m_AlignmentTier, 0);
			int rankForMark = alignmentMarksRoot.GetRankForMark(m_Direction, mark);
			int rankForMark2 = alignmentMarksRoot.GetRankForMark(m_Direction, mark2);
			m_Progress.Value = 1f * (float)(alignmentRank - rankForMark2) / (float)(rankForMark - rankForMark2);
		}
	}
}
