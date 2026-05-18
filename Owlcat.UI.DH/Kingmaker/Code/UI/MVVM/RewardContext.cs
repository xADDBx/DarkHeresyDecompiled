using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RewardContext : ViewModel, IAlignmentRankShiftHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private readonly ReactiveProperty<AlignmentMarkRewardVM> m_AlignmentRewardVM = new ReactiveProperty<AlignmentMarkRewardVM>();

	public ReadOnlyReactiveProperty<AlignmentMarkRewardVM> AlignmentRewardVM => m_AlignmentRewardVM;

	public RewardContext(ReactiveProperty<AlignmentMarkRewardVM> alignmentRewardVm)
	{
		m_AlignmentRewardVM = alignmentRewardVm;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeSoulMarkReward();
	}

	public void HandleAlignmentRankShift(AlignmentShift shift)
	{
		AlignmentAxis axis = shift.Axis;
		int mainCharacterAlignmentMark = AlignmentShiftExtension.GetMainCharacterAlignmentMark(axis);
		int currentRank = mainCharacterAlignmentMark - shift.Value;
		int alignmentRankIndex = AlignmentShiftExtension.GetAlignmentRankIndex(axis, mainCharacterAlignmentMark);
		int alignmentRankIndex2 = AlignmentShiftExtension.GetAlignmentRankIndex(axis, currentRank);
		if (alignmentRankIndex > alignmentRankIndex2)
		{
			m_AlignmentRewardVM.Value = new AlignmentMarkRewardVM(axis, alignmentRankIndex, DisposeSoulMarkReward);
		}
	}

	private void DisposeSoulMarkReward()
	{
		AlignmentRewardVM.CurrentValue?.Dispose();
		m_AlignmentRewardVM.Value = null;
	}
}
