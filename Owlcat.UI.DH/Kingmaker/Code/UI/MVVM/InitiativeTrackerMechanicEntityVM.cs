using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerMechanicEntityVM : CombatMechanicEntityVM
{
	private readonly ReactiveProperty<int> m_Round = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_OrderIndex = new ReactiveProperty<int>();

	public ReadOnlyReactiveProperty<int> Round => m_Round;

	public ReadOnlyReactiveProperty<int> OrderIndex => m_OrderIndex;

	public InitiativeTrackerMechanicEntityVM(int round)
	{
		m_Round = new ReactiveProperty<int>(round).AddTo(this);
	}

	public InitiativeTrackerMechanicEntityVM(MechanicEntity mechanicMechanicEntity, int index, bool isCurrent)
		: base(mechanicMechanicEntity, null, isCurrent)
	{
		m_OrderIndex = new ReactiveProperty<int>(index).AddTo(this);
	}

	public void UpdateData(int index, bool isCurrent)
	{
		if (base.MechanicEntity != null)
		{
			m_OrderIndex.Value = index;
			m_IsCurrent.Value = isCurrent;
			UpdateData();
		}
	}

	public void SetRound(int round)
	{
		m_Round.Value = round;
	}
}
