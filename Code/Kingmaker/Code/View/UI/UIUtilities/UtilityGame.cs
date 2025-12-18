namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityGame
{
	public static bool IsGlobalMap()
	{
		return Game.Instance.CurrentlyLoadedArea?.IsGlobalmapArea ?? false;
	}
}
