namespace Kingmaker.Code.Middleware.Metrics;

public class DetectivePieceMetricsEvent : MetricsEvent
{
	public enum PieceType
	{
		Clue,
		Addendum,
		Conclusion
	}

	public enum PieceState
	{
		Added,
		Removed
	}

	protected override string Name => "detective_piece";

	public DetectivePieceMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public DetectivePieceMetricsEvent Type(PieceType type)
	{
		AddParam("type", type switch
		{
			PieceType.Clue => "clue", 
			PieceType.Addendum => "addendum", 
			PieceType.Conclusion => "conclusion", 
			_ => MetricsUtils.EnumToSnakeCase(type), 
		});
		return this;
	}

	public DetectivePieceMetricsEvent State(PieceState state)
	{
		AddParam("state", state switch
		{
			PieceState.Added => "added", 
			PieceState.Removed => "removed", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}
}
