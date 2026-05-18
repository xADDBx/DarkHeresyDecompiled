using Kingmaker.Code.UI.MVVM.CombatNotifications.CombatObjectives;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.CombatNotifications;

public class CombatHUDNotificationsVM : ViewModel, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	public readonly ServoSkullBarkVM ServoSkullBarkVM;

	public readonly CombatObjectivesVM CombatObjectivesVM;

	public readonly ReadOnlyReactiveProperty<bool> IsHidden;

	public CombatHUDNotificationsVM(ReadOnlyReactiveProperty<bool> forceHidden)
	{
		IsHidden = forceHidden;
		ServoSkullBarkVM = new ServoSkullBarkVM().AddTo(this);
		ReadOnlyReactiveProperty<bool> isForceHidden = ServoSkullBarkVM.Bark.Select((string bark) => bark != null).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
		CombatObjectivesVM = new CombatObjectivesVM(ActiveEncounter.Current, isForceHidden).AddTo(this);
	}

	void IBarkHandler.HandleOnShowBark(string text)
	{
		ServoSkullBarkVM.HandleOnShowBark(text);
	}

	void IBarkHandler.HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
		ServoSkullBarkVM.HandleOnShowLinkedBark(text, encyclopediaLink);
	}

	void IBarkHandler.HandleOnHideBark()
	{
		ServoSkullBarkVM.HandleOnHideBark();
	}
}
