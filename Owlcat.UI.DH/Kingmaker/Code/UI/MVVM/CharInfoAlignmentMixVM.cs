using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentMixVM : CharInfoComponentVM
{
	public enum MixStatus
	{
		None,
		Unlocked,
		Locked
	}

	public readonly AlignmentMix AlignmentMix;

	public readonly MixStatus Status;

	public readonly TooltipBaseTemplate Tooltip;

	private Dictionary<AlignmentMix, (AlignmentAxis, AlignmentAxis)> m_AlignmentAxisPairs = new Dictionary<AlignmentMix, (AlignmentAxis, AlignmentAxis)>
	{
		{
			AlignmentMix.XanthiteMonodominance,
			(AlignmentAxis.Xanthite, AlignmentAxis.Monodominance)
		},
		{
			AlignmentMix.XanthiteTorian,
			(AlignmentAxis.Xanthite, AlignmentAxis.Torian)
		},
		{
			AlignmentMix.XenophiliaMonodominance,
			(AlignmentAxis.Xenophilia, AlignmentAxis.Monodominance)
		},
		{
			AlignmentMix.XenophiliaTorian,
			(AlignmentAxis.Xenophilia, AlignmentAxis.Torian)
		}
	};

	public CharInfoAlignmentMixVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, AlignmentMix alignmentMix)
		: base(unit)
	{
		AlignmentMix = alignmentMix;
		AlignmentMix alignmentMix2 = unit.CurrentValue.Alignment.GetAlignmentMix();
		if (alignmentMix2 == AlignmentMix.None)
		{
			Status = MixStatus.None;
		}
		else
		{
			Status = ((alignmentMix2 == alignmentMix) ? MixStatus.Unlocked : MixStatus.Locked);
		}
	}
}
