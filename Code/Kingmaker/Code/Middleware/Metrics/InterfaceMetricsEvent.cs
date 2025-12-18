namespace Kingmaker.Code.Middleware.Metrics;

public class InterfaceMetricsEvent : MetricsEvent
{
	public enum InterfaceStates
	{
		Open,
		Close
	}

	public enum InterfaceTypes
	{
		Dialogue,
		CharScreen,
		CharGen,
		LevelUp,
		Respec,
		Inventory,
		BugReport,
		Detective,
		LoadingScreen,
		Tutorial,
		Encyclopedia
	}

	protected override string Name => "interface";

	public InterfaceMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	protected override void AddSpecificEventDefaultParameters()
	{
		base.AddSpecificEventDefaultParameters();
		if (Game.Instance?.LoadedArea?.Blueprint.AssetGuid != null)
		{
			AddParam("location_id", Game.Instance?.LoadedArea?.Blueprint.AssetGuid);
		}
	}

	public InterfaceMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public InterfaceMetricsEvent InterfaceState(InterfaceStates state)
	{
		AddParam("state", state switch
		{
			InterfaceStates.Open => "open", 
			InterfaceStates.Close => "close", 
			_ => MetricsEvent.EnumToSnakeCase(state), 
		});
		return this;
	}

	public InterfaceMetricsEvent InterfaceType(InterfaceTypes type)
	{
		AddParam("type", type switch
		{
			InterfaceTypes.Dialogue => "dialogue", 
			InterfaceTypes.CharScreen => "char_screen", 
			InterfaceTypes.CharGen => "char_gen", 
			InterfaceTypes.LevelUp => "level_up", 
			InterfaceTypes.Respec => "respec", 
			InterfaceTypes.Inventory => "inventory", 
			InterfaceTypes.BugReport => "bug_report", 
			InterfaceTypes.Detective => "detective", 
			InterfaceTypes.LoadingScreen => "loading_screen", 
			InterfaceTypes.Tutorial => "tutorial", 
			InterfaceTypes.Encyclopedia => "encyclopedia", 
			_ => MetricsEvent.EnumToSnakeCase(type), 
		});
		return this;
	}
}
