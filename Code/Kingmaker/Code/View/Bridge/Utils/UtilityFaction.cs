using Kingmaker.Gameplay.Features.Reputation;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityFaction
{
	public static string GetSpriteLabel(ReputationType reputation)
	{
		return reputation switch
		{
			ReputationType.Respect => "<color=#FFFFFF><sprite name=\"Respect\"></color>", 
			ReputationType.Fear => "<color=#FFFFFF><sprite name=\"Fear\"></color>", 
			_ => string.Empty, 
		};
	}
}
