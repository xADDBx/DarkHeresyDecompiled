using Unity.Profiling;

namespace Owlcat.Runtime.Visual.VirtualTexture.Profiling;

public static class Counters
{
	private static readonly ProfilerCategory Category = ProfilerCategory.Scripts;

	private const string kVirtualAtlasOccupancyName = "Virtual Atlas Occupancy";

	private const string kTilesLoadedPerFrameName = "Tiles Loaded Per Frame";

	private const string kTilesLoadedTotalName = "Tiles Loaded Total";

	private const string kTilesLoadingLag = "Tiles Loading Lag (Frames)";

	private const string kFeedbackConsumptionName = "Feedback Consumption";

	public static readonly ProfilerCounterValue<float> VirtualAtlasOccupancy = new ProfilerCounterValue<float>(Category, "Virtual Atlas Occupancy", ProfilerMarkerDataUnit.Count);

	public static readonly ProfilerCounterValue<int> TilesLoadedPerFrame = new ProfilerCounterValue<int>(Category, "Tiles Loaded Per Frame", ProfilerMarkerDataUnit.Count);

	public static readonly ProfilerCounterValue<int> TilesLoadedTotal = new ProfilerCounterValue<int>(Category, "Tiles Loaded Total", ProfilerMarkerDataUnit.Count);

	public static readonly ProfilerCounterValue<int> TilesLoadingLag = new ProfilerCounterValue<int>(Category, "Tiles Loading Lag (Frames)", ProfilerMarkerDataUnit.Count);

	public static readonly ProfilerCounterValue<float> FeedbackConsumption = new ProfilerCounterValue<float>(Category, "Feedback Consumption", ProfilerMarkerDataUnit.Count);

	public static readonly string[] kAutoEnabledCategoryNames = new string[1] { ProfilerCategory.Scripts.Name };
}
