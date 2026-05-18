using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CharInfoAlignmentGroupVM : CharInfoComponentVM
{
	public readonly AlignmentAxis Direction;

	public readonly AutoDisposingList<CharInfoAlignmentAbilitySlotVM> AbilitySlotVms = new AutoDisposingList<CharInfoAlignmentAbilitySlotVM>();

	public TooltipBaseTemplate Tooltip { get; private set; }

	public int CurrentRank { get; private set; }

	public int CurrentLevel { get; private set; }

	public int MaxRank { get; private set; }

	public List<int> RankThresholds { get; private set; } = new List<int>();


	public CharInfoAlignmentGroupVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, AlignmentAxis direction)
		: base(unit)
	{
		PartUnitAlignment alignment = Unit.CurrentValue.Alignment;
		Direction = direction;
		int marksAmount = ConfigRoot.Instance.AlignmentMarksRoot.GetMarksAmount(direction);
		UpdateAlignmentInfo();
		for (int i = 0; i < marksAmount; i++)
		{
			AbilitySlotVms.Add(new CharInfoAlignmentAbilitySlotVM(Unit, direction, i + 1, alignment).AddTo(this));
		}
	}

	public void UpdateAlignmentInfo()
	{
		AlignmentTooltipExtensions.GetAlignmentInfo(Direction, Unit.CurrentValue, out var rankThresholds, out var maxValue, out var currentValue, out var currentTier);
		MaxRank = maxValue;
		CurrentRank = currentValue;
		CurrentLevel = currentTier;
		RankThresholds = rankThresholds;
		Tooltip = new TooltipTemplateAlignmentHeader(Unit.CurrentValue, Direction);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AbilitySlotVms.Clear();
		RankThresholds.Clear();
	}
}
