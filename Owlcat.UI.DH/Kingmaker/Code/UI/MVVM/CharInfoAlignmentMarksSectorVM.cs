using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CharInfoAlignmentMarksSectorVM : CharInfoComponentVM
{
	public readonly AlignmentAxis Direction;

	public readonly AutoDisposingList<CharInfoAlignmentAbilitySlotVM> AbilitySlotVms = new AutoDisposingList<CharInfoAlignmentAbilitySlotVM>();

	public TooltipBaseTemplate Tooltip;

	private List<int> m_RankThresholds = new List<int>();

	private int m_MaxRank;

	private int m_CurrentRank;

	private int m_CurrentLevel;

	public int CurrentRank => m_CurrentRank;

	public int CurrentLevel => m_CurrentLevel;

	public int MaxRank => m_MaxRank;

	public List<int> RankThresholds => m_RankThresholds;

	public CharInfoAlignmentMarksSectorVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, AlignmentAxis direction)
		: base(unit)
	{
		PartUnitAlignment alignment = Unit.CurrentValue.Alignment;
		Direction = direction;
		int marksAmount = ConfigRoot.Instance.AlignmentMarksRoot.GetMarksAmount(direction);
		UpdateSoulMarkInfo();
		for (int i = 0; i < marksAmount; i++)
		{
			AbilitySlotVms.Add(new CharInfoAlignmentAbilitySlotVM(Unit, direction, i + 1, alignment).AddTo(this));
		}
	}

	public void UpdateSoulMarkInfo()
	{
		AlignmentTooltipExtensions.GetAlignmentInfo(Direction, Unit.CurrentValue, out m_RankThresholds, out m_MaxRank, out m_CurrentRank, out m_CurrentLevel);
		Tooltip = new TooltipTemplateSoulMarkHeader(Unit.CurrentValue, Direction);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AbilitySlotVms.Clear();
		RankThresholds.Clear();
	}
}
