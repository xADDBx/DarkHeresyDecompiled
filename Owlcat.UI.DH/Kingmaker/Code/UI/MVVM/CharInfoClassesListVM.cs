using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CharInfoClassesListVM : CharInfoComponentVM
{
	public AutoDisposingList<CharInfoClassEntryVM> ClassVMs { get; } = new AutoDisposingList<CharInfoClassEntryVM>();


	public CharInfoClassesListVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		RefreshData();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		PartUnitProgression progression = Unit?.CurrentValue?.Progression;
		ExtractProgression(progression);
		RefreshClassesList();
	}

	private void ExtractProgression(PartUnitProgression progression)
	{
	}

	private void RefreshClassesList()
	{
		ClassVMs.Clear();
	}
}
