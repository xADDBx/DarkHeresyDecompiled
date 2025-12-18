namespace Kingmaker.Gameplay.Features.Experience;

public interface IExperienceSettings
{
	ExperienceType Type { get; }

	int? OverrideCR { get; }

	int? OverrideValue { get; }
}
