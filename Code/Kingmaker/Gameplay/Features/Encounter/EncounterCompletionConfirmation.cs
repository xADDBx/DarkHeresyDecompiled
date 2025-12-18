using System;

namespace Kingmaker.Gameplay.Features.Encounter;

public abstract class EncounterCompletionConfirmation : IDisposable
{
	public readonly EncounterCompletionType Type;

	public bool IsConfirmed { get; private set; }

	public bool IsRejected { get; private set; }

	protected EncounterCompletionConfirmation(EncounterCompletionType type)
	{
		Type = type;
	}

	protected void Confirm()
	{
		if (IsRejected)
		{
			throw new InvalidOperationException();
		}
		IsConfirmed = true;
	}

	protected void Reject()
	{
		if (IsConfirmed)
		{
			throw new InvalidOperationException();
		}
		IsRejected = true;
	}

	protected abstract void UpdateInternal();

	public void Update()
	{
		if (!IsConfirmed && !IsRejected)
		{
			UpdateInternal();
		}
	}

	public abstract void Dispose();
}
