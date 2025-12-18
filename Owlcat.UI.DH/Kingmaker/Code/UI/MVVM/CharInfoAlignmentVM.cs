using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentVM : CharInfoComponentVM
{
	public readonly ReactiveProperty<CharInfoAlignmentMarksSectorVM> TorianSector = new ReactiveProperty<CharInfoAlignmentMarksSectorVM>();

	public readonly ReactiveProperty<CharInfoAlignmentMarksSectorVM> MonodominanceSector = new ReactiveProperty<CharInfoAlignmentMarksSectorVM>();

	public readonly ReactiveProperty<CharInfoAlignmentMarksSectorVM> XanthiteSector = new ReactiveProperty<CharInfoAlignmentMarksSectorVM>();

	public readonly ReactiveProperty<CharInfoAlignmentMarksSectorVM> XenophiliaSector = new ReactiveProperty<CharInfoAlignmentMarksSectorVM>();

	public const int MaxUnlockedTierId = 2;

	public ConvictionBarVM ConvictionBar { get; private set; }

	public CharInfoAlignmentVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		ConvictionBar = new ConvictionBarVM(Unit).AddTo(this);
		Unit.CurrentValue.GetAlignmentMarks();
		TorianSector.Value = new CharInfoAlignmentMarksSectorVM(Unit, AlignmentAxis.Torian).AddTo(this);
		MonodominanceSector.Value = new CharInfoAlignmentMarksSectorVM(Unit, AlignmentAxis.Monodominance).AddTo(this);
		XanthiteSector.Value = new CharInfoAlignmentMarksSectorVM(Unit, AlignmentAxis.Xanthite).AddTo(this);
		XenophiliaSector.Value = new CharInfoAlignmentMarksSectorVM(Unit, AlignmentAxis.Xenophilia).AddTo(this);
	}
}
