using Kingmaker.Blueprints.Area;

namespace Kingmaker;

public static class GameScenes
{
	public const string Start = "Start";

	public const string BaseMechanics = "BaseMechanics";

	public const string EntityBounds = "EntityBounds";

	public const string CommonUI = "UI_Common_Scene";

	public const string LoadingScreen = "LoadingScreen";

	public const string IngameConsoleScene = "IngameConsole";

	public const string UnityPlaymodeTestScene = "InitTestScene";

	public static readonly SceneReference CommonUISceneRef = new SceneReference("UI_Common_Scene");

	public static readonly string[] ScenesToIncludeInBundles = new string[4] { MainMenu, "UI_Common_Scene", "IngameConsole", "LoadingScreen" };

	public static string MainMenu => "MainMenu";
}
