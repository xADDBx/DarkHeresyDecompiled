using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentHistoryVM : CharInfoComponentVM
{
	public List<CharInfoAlignmentShiftRecordVM> SoulMarkShiftsHistory;

	public string Biography { get; private set; }

	public bool IsMainPlayer { get; private set; }

	public CharInfoAlignmentHistoryVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		IsMainPlayer = Game.Instance.Player.MainCharacterOriginalEntity == Unit.CurrentValue;
		if (IsMainPlayer)
		{
			SoulMarkShiftsHistory = (from s in AlignmentShiftExtension.AppliedShifts()
				select new CharInfoAlignmentShiftRecordVM(s)).ToList();
			return;
		}
		IEnumerable<BlueprintCompanionStory> source = Game.Instance.Player.CompanionStories.Get(Unit.CurrentValue);
		Biography = string.Join("\n ", source.Select((BlueprintCompanionStory s) => s.Description.Text));
	}
}
