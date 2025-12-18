namespace Kingmaker.Code.Middleware.Metrics;

public class EquipmentMetricsEvent : MetricsEvent
{
	public enum EquipmentStates
	{
		Equip,
		Remove
	}

	protected override string Name => "equipment";

	public EquipmentMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public EquipmentMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public EquipmentMetricsEvent OwnerId(string owner_id)
	{
		AddParam("owner_id", owner_id);
		return this;
	}

	public EquipmentMetricsEvent State(EquipmentStates state)
	{
		AddParam("state", state switch
		{
			EquipmentStates.Equip => "equip", 
			EquipmentStates.Remove => "remove", 
			_ => MetricsEvent.EnumToSnakeCase(state), 
		});
		return this;
	}
}
