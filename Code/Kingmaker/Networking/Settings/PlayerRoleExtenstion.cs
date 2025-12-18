using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Networking.Settings;

public static class PlayerRoleExtenstion
{
	public static bool Can(this PlayerRole playerRole, Entity entity)
	{
		return playerRole.Can(entity, NetworkingManager.LocalNetPlayer);
	}

	public static bool Can(this PlayerRole playerRole, Entity entity, NetPlayer player)
	{
		return playerRole.Can(entity.UniqueId, player);
	}
}
