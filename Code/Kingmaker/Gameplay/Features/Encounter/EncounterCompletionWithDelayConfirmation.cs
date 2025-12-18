namespace Kingmaker.Gameplay.Features.Encounter;

public sealed class EncounterCompletionWithDelayConfirmation : EncounterCompletionConfirmation
{
	private float _timer;

	public EncounterCompletionWithDelayConfirmation(EncounterCompletionType type)
		: base(type)
	{
	}

	protected override void UpdateInternal()
	{
		Confirm();
	}

	public override void Dispose()
	{
	}
}
