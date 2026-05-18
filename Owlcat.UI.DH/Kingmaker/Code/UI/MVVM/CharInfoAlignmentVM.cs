using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Progression.Features;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentVM : CharInfoComponentVM
{
	private readonly ReactiveProperty<bool> m_UnitIsXenos = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<CharInfoAlignmentGroupVM> m_TorianSector = new ReactiveProperty<CharInfoAlignmentGroupVM>();

	private readonly ReactiveProperty<CharInfoAlignmentGroupVM> m_MonodominanceSector = new ReactiveProperty<CharInfoAlignmentGroupVM>();

	private readonly ReactiveProperty<CharInfoAlignmentGroupVM> m_XanthiteSector = new ReactiveProperty<CharInfoAlignmentGroupVM>();

	private readonly ReactiveProperty<CharInfoAlignmentGroupVM> m_XenophiliaSector = new ReactiveProperty<CharInfoAlignmentGroupVM>();

	private readonly ReactiveProperty<CharInfoAlignmentMixVM> m_XantMonoMix = new ReactiveProperty<CharInfoAlignmentMixVM>();

	private readonly ReactiveProperty<CharInfoAlignmentMixVM> m_XantTorMix = new ReactiveProperty<CharInfoAlignmentMixVM>();

	private readonly ReactiveProperty<CharInfoAlignmentMixVM> m_XenoMonoMix = new ReactiveProperty<CharInfoAlignmentMixVM>();

	private readonly ReactiveProperty<CharInfoAlignmentMixVM> m_XenoTorMix = new ReactiveProperty<CharInfoAlignmentMixVM>();

	public ReadOnlyReactiveProperty<bool> UnitIsXenos => m_UnitIsXenos;

	public ReadOnlyReactiveProperty<CharInfoAlignmentGroupVM> TorianSector => m_TorianSector;

	public ReadOnlyReactiveProperty<CharInfoAlignmentGroupVM> MonodominanceSector => m_MonodominanceSector;

	public ReadOnlyReactiveProperty<CharInfoAlignmentGroupVM> XanthiteSector => m_XanthiteSector;

	public ReadOnlyReactiveProperty<CharInfoAlignmentGroupVM> XenophiliaSector => m_XenophiliaSector;

	public ReadOnlyReactiveProperty<CharInfoAlignmentMixVM> XantMonoMix => m_XantMonoMix;

	public ReadOnlyReactiveProperty<CharInfoAlignmentMixVM> XantTorMix => m_XantTorMix;

	public ReadOnlyReactiveProperty<CharInfoAlignmentMixVM> XenoMonoMix => m_XenoMonoMix;

	public ReadOnlyReactiveProperty<CharInfoAlignmentMixVM> XenoTorMix => m_XenoTorMix;

	public CharInfoAlignmentVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		BlueprintRace race = Unit.CurrentValue.Blueprint.Race;
		ReactiveProperty<bool> unitIsXenos = m_UnitIsXenos;
		bool value;
		if (race != null)
		{
			Race raceId = race.RaceId;
			if ((uint)(raceId - 2) <= 1u)
			{
				value = true;
				goto IL_0039;
			}
		}
		value = false;
		goto IL_0039;
		IL_0039:
		unitIsXenos.Value = value;
		m_TorianSector.Value = new CharInfoAlignmentGroupVM(Unit, AlignmentAxis.Torian).AddTo(this);
		m_MonodominanceSector.Value = new CharInfoAlignmentGroupVM(Unit, AlignmentAxis.Monodominance).AddTo(this);
		m_XanthiteSector.Value = new CharInfoAlignmentGroupVM(Unit, AlignmentAxis.Xanthite).AddTo(this);
		m_XenophiliaSector.Value = new CharInfoAlignmentGroupVM(Unit, AlignmentAxis.Xenophilia).AddTo(this);
		m_XantMonoMix.Value = new CharInfoAlignmentMixVM(Unit, AlignmentMix.XanthiteMonodominance).AddTo(this);
		m_XantTorMix.Value = new CharInfoAlignmentMixVM(Unit, AlignmentMix.XanthiteTorian).AddTo(this);
		m_XenoMonoMix.Value = new CharInfoAlignmentMixVM(Unit, AlignmentMix.XenophiliaMonodominance).AddTo(this);
		m_XenoTorMix.Value = new CharInfoAlignmentMixVM(Unit, AlignmentMix.XenophiliaTorian).AddTo(this);
	}
}
