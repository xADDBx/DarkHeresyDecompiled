namespace Kingmaker.Code.Gameplay.Parts;

public interface IUIUnitMoraleData
{
	int Morale { get; }

	int MinValue { get; }

	int MaxValue { get; }

	MoralePhaseType MoralePhase { get; }
}
