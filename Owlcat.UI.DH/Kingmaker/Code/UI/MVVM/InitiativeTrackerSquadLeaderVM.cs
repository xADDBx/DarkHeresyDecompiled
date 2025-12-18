using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerSquadLeaderVM : InitiativeTrackerMechanicEntityVM
{
	private List<InitiativeTrackerMechanicEntityVM> m_SquadUnits = new List<InitiativeTrackerMechanicEntityVM>();

	private InitiativeTrackerMechanicEntityVM m_Leader;

	public InitiativeTrackerSquadLeaderVM(int round)
		: base(round)
	{
	}

	public InitiativeTrackerSquadLeaderVM(MechanicEntity mechanicMechanicEntity, int index, bool isCurrent)
		: base(mechanicMechanicEntity, index, isCurrent)
	{
	}

	public void SetSquadLeader(InitiativeTrackerMechanicEntityVM leader)
	{
		m_Leader = leader;
		m_Leader.NeedToShow.Skip(1).Subscribe(delegate(bool x)
		{
			HandleShowingSquadChange(x);
		}).AddTo(this);
	}

	public void AddToSquad(InitiativeTrackerMechanicEntityVM mechanicEntity)
	{
		m_SquadUnits.Add(mechanicEntity);
	}

	private void HandleShowingSquadChange(bool val)
	{
		foreach (InitiativeTrackerMechanicEntityVM squadUnit in m_SquadUnits)
		{
			_ = squadUnit;
			m_NeedToShow.Value = val;
		}
		EventBus.RaiseEvent(delegate(IInitiativeTrackerShowGroup h)
		{
			h.HandleShowChange();
		});
	}
}
