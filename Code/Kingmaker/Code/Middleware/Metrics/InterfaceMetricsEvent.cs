using Kingmaker.Code.View.Bridge.Enums;

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
		Respec,
		BugReport,
		LoadingScreen,
		Tutorial,
		CombatLog,
		Formation
	}

	protected override string Name => "interface";

	public InterfaceMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public InterfaceMetricsEvent State(InterfaceStates state)
	{
		AddParam("state", state switch
		{
			InterfaceStates.Open => "open", 
			InterfaceStates.Close => "close", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}

	public InterfaceMetricsEvent FullScreenType(FullScreenUIType type)
	{
		AddParam("type", type switch
		{
			FullScreenUIType.LevelUp => "level_up", 
			FullScreenUIType.EscapeMenu => "escape_menu", 
			FullScreenUIType.SaveLoad => "save_load", 
			FullScreenUIType.Inventory => "inventory", 
			FullScreenUIType.CharacterScreen => "character_screen", 
			FullScreenUIType.Journal => "journal", 
			FullScreenUIType.Reputation => "reputation", 
			FullScreenUIType.LocalMap => "local_map", 
			FullScreenUIType.DetectiveJournal => "detective_journal", 
			FullScreenUIType.Vendor => "vendor", 
			FullScreenUIType.Settings => "settings", 
			FullScreenUIType.Credits => "credits", 
			FullScreenUIType.Encyclopedia => "encyclopedia", 
			FullScreenUIType.Chargen => "chargen", 
			FullScreenUIType.TransitionMap => "transition_map", 
			FullScreenUIType.NewGame => "new_game", 
			FullScreenUIType.FirstLaunchSettings => "first_launch_settings", 
			FullScreenUIType.GroupChanger => "group_changer", 
			FullScreenUIType.Loot => "loot", 
			FullScreenUIType.OneSlotLoot => "one_slot_loot", 
			FullScreenUIType.PlayerChest => "player_chest", 
			_ => MetricsUtils.EnumToSnakeCase(type), 
		});
		return this;
	}

	public InterfaceMetricsEvent Type(InterfaceTypes type)
	{
		AddParam("type", type switch
		{
			InterfaceTypes.Respec => "respec", 
			InterfaceTypes.BugReport => "bug_report", 
			InterfaceTypes.LoadingScreen => "loading_screen", 
			InterfaceTypes.Tutorial => "tutorial", 
			InterfaceTypes.CombatLog => "combat_log", 
			InterfaceTypes.Formation => "formation", 
			_ => MetricsUtils.EnumToSnakeCase(type), 
		});
		return this;
	}
}
