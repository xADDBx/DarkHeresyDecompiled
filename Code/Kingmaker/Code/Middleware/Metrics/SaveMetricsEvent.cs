using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Code.Middleware.Metrics;

public class SaveMetricsEvent : MetricsEvent
{
	protected override string Name => "save";

	public SaveMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public SaveMetricsEvent Type(SaveInfo.SaveType type)
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

	public SaveMetricsEvent Location(string location)
	{
		AddParam("location", location);
		return this;
	}

	public SaveMetricsEvent Encounter(string encounter)
	{
		AddParam("encounter", encounter);
		return this;
	}
}
