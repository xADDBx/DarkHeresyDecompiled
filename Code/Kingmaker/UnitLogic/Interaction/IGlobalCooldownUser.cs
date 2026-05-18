namespace Kingmaker.UnitLogic.Interaction;

public interface IGlobalCooldownUser
{
	bool UseGlobalCooldown { get; }

	bool CanCluster { get; }
}
