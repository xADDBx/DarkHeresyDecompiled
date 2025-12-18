using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Code.Middleware.Metrics;

public class LoadMetricsEvent : MetricsEvent
{
	protected override string Name => "load";

	public LoadMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public LoadMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public LoadMetricsEvent Type(SaveInfo.SaveType type)
	{
		AddParam("type", type switch
		{
			SaveInfo.SaveType.Auto => "auto", 
			SaveInfo.SaveType.Manual => "manual", 
			SaveInfo.SaveType.Quick => "quick", 
			SaveInfo.SaveType.Bugreport => "bugreport", 
			SaveInfo.SaveType.Coop => "coop", 
			SaveInfo.SaveType.Remote => "remote", 
			SaveInfo.SaveType.ForImport => "for_import", 
			SaveInfo.SaveType.IronMan => "iron_man", 
			_ => MetricsEvent.EnumToSnakeCase(type), 
		});
		return this;
	}

	public LoadMetricsEvent GameId(string game_Id)
	{
		AddParam("game_Id", game_Id);
		return this;
	}
}
