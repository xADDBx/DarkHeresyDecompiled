using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Gameplay.Features.Encounter;

public sealed class EncounterCompletionByMoraleConfirmation : EncounterCompletionConfirmation
{
	private bool _waitingConfirmationFromPlayer;

	public EncounterCompletionByMoraleConfirmation(EncounterCompletionType type)
		: base(type)
	{
		EventBus.Subscribe(this);
	}

	protected override void UpdateInternal()
	{
		if (!_waitingConfirmationFromPlayer)
		{
			_waitingConfirmationFromPlayer = true;
			EventBus.RaiseEvent(delegate(IMoraleVictoryConfirmationRequest h)
			{
				h.HandleMoraleVictoryConfirmationRequest(Callback);
			});
		}
	}

	private void Callback(bool confirmed)
	{
		_waitingConfirmationFromPlayer = false;
		if (confirmed)
		{
			Confirm();
		}
		else
		{
			Reject();
		}
		EventBus.RaiseEvent(delegate(IMoraleVictoryConfirmationHandler h)
		{
			h.HandleMoraleVictoryConfirmation(confirmed);
		});
	}

	public override void Dispose()
	{
		EventBus.Unsubscribe(this);
	}
}
