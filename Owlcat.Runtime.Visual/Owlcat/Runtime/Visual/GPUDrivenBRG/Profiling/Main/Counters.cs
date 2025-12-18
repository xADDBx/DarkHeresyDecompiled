using Unity.Profiling;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main;

public static class Counters
{
	private const string kCullingSplits = "Culling Splits";

	private const string kTotalCameraDrawCommands = "Total Camera Draw Commands";

	private const string kTotalShadowDrawCommands = "Total Shadow Draw Commands";

	private const string kTotalHDBDrawCommands = "Total Camera HDB Draw Commands";

	private const string kTotalMotionVectorsDrawCommands = "Total Motion Vectors Draw Commands";

	private const string kSingleSplitDrawCommands = "Single Split Draw Commands";

	private const string kSingleSplitDrawRanges = "Single Split Draw Ranges";

	private const string kSingleSplitShadowDrawCommands = "Single Split Shadow Draw Commands";

	private const string kSingleSplitShadowDrawRanges = "Single Split Shadow Draw Ranges";

	private const string kSingleSplitShadowCullLightmappedDrawCommands = "Single Split Shadow (Cull Lightmapped) Draw Commands";

	private const ProfilerCounterOptions kDefaultCounterOptions = ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush;

	private static readonly ProfilerCategory s_Category = ProfilerCategory.Scripts;

	public static readonly ProfilerCounterValue<int> CullingSplits = new ProfilerCounterValue<int>(s_Category, "Culling Splits", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> TotalCameraDrawCommands = new ProfilerCounterValue<int>(s_Category, "Total Camera Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> TotalShadowDrawCommands = new ProfilerCounterValue<int>(s_Category, "Total Shadow Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> TotalHDBDrawCommands = new ProfilerCounterValue<int>(s_Category, "Total Camera HDB Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> TotalMotionVectorsDrawCommands = new ProfilerCounterValue<int>(s_Category, "Total Motion Vectors Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> SingleSplitDrawCommands = new ProfilerCounterValue<int>(s_Category, "Single Split Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> SingleSplitDrawRanges = new ProfilerCounterValue<int>(s_Category, "Single Split Draw Ranges", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> SingleSplitShadowDrawCommands = new ProfilerCounterValue<int>(s_Category, "Single Split Shadow Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> SingleSplitShadowDrawRanges = new ProfilerCounterValue<int>(s_Category, "Single Split Shadow Draw Ranges", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> SingleSplitShadowCullLightmappedDrawCommands = new ProfilerCounterValue<int>(s_Category, "Single Split Shadow (Cull Lightmapped) Draw Commands", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly string[] AutoEnabledCategoryNames = new string[1] { ProfilerCategory.Scripts.Name };
}
