namespace Kingmaker.View;

public interface IMovementSettingsProvider
{
	const float DefaultWalkingSpeed = 1.5f;

	const float DefaultAcceleration = 10f;

	const bool DefaultDecelerateBeforeStop = true;

	const float DefaultStoppingDistance = 1.35f;

	const float DefaultMinSpeed = 0.2f;

	const float DefaultAngularSpeedInCombat = 360f;

	const float DefaultAngularSpeedInNonCombat = 180f;

	const float DefaultAngularSpeedWhenMove = 220f;

	const float DefaultSlowDownCoefficient = 0.7f;

	float Acceleration { get; }

	bool DecelerateBeforeStop { get; }

	float StoppingDistance { get; }

	float MinSpeed { get; }

	float AngularSpeedInCombat { get; }

	float AngularSpeedInNonCombat { get; }

	float AngularSpeedWhenMove { get; }

	float SlowDownCoefficient { get; }
}
