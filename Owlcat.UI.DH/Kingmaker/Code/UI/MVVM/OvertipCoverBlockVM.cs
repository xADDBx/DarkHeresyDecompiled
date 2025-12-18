using Kingmaker.View.Covers;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCoverBlockVM : ViewModel
{
	private readonly MechanicEntityUIState m_MechanicEntityUIState;

	private readonly ReactiveProperty<bool> m_IsVisibleTrigger = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<LosCalculations.CoverType> CoverType => m_MechanicEntityUIState.CoverType;

	public ReadOnlyReactiveProperty<bool> IsVisibleTrigger => m_IsVisibleTrigger;

	public OvertipCoverBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		m_MechanicEntityUIState = mechanicEntityUIState;
		m_MechanicEntityUIState.IsInCombat.CombineLatest(m_MechanicEntityUIState.IsVisibleForPlayer, m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead, (bool isInCombat, bool isVisibleForPlayer, bool isDead) => isInCombat && isVisibleForPlayer && !isDead).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(bool value)
		{
			m_IsVisibleTrigger.Value = value;
		})
			.AddTo(this);
	}
}
