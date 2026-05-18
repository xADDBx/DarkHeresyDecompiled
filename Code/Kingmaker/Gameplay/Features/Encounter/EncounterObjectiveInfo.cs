using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.Localization;

namespace Kingmaker.Gameplay.Features.Encounter;

public readonly struct EncounterObjectiveInfo
{
	public readonly LocalizedString Description;

	public readonly LocalizedString Hint;

	public readonly EncounterObjectiveType Type;

	public readonly EncounterObjectiveState State;

	public readonly EncounterActiveObjectiveResolution ActiveStateResolution;

	public readonly int CurrentValue;

	public readonly int? TargetValue;

	public readonly LocalizedString? CounterFormat;

	public bool IsCounter => Type == EncounterObjectiveType.Counter;

	public EncounterObjectiveInfo(LocalizedString description, LocalizedString hint, EncounterObjectiveType type, EncounterObjectiveState state, EncounterActiveObjectiveResolution activeStateResolution, int currentValue, int? targetValue, LocalizedString? counterFormat)
	{
		Description = description;
		Hint = hint;
		Type = type;
		State = state;
		ActiveStateResolution = activeStateResolution;
		CurrentValue = currentValue;
		TargetValue = targetValue;
		CounterFormat = counterFormat;
	}
}
