using Unity.Profiling;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;

public static class Counters
{
	public class CounterCollection
	{
		public const string kInstanceDataCPU = "Instance Data CPU";

		public const string kInstanceDataGPU = "Instance Data GPU";

		public const string kResourceDataCPU = "Resource Data CPU";

		public const string kCullingBatchingDataCPU = "Culling & Batching Data CPU";

		public const string kCullingBatchingDataGPU = "Culling & Batching Data GPU";

		public const string kMiscCPU = "Misc CPU";

		public const string kMiscGPU = "Misc GPU";

		public readonly ProfilerCounterValue<int> CullingBatchingCPU = new ProfilerCounterValue<int>(Category, "Culling & Batching Data CPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public readonly ProfilerCounterValue<int> CullingBatchingGPU = new ProfilerCounterValue<int>(Category, "Culling & Batching Data GPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public readonly ProfilerCounterValue<int> InstanceDataCPU = new ProfilerCounterValue<int>(Category, "Instance Data CPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public readonly ProfilerCounterValue<int> InstanceDataGPU = new ProfilerCounterValue<int>(Category, "Instance Data GPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public readonly ProfilerCounterValue<int> MiscCPU = new ProfilerCounterValue<int>(Category, "Misc CPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public readonly ProfilerCounterValue<int> MiscGPU = new ProfilerCounterValue<int>(Category, "Misc GPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public readonly ProfilerCounterValue<int> ResourceDataCPU = new ProfilerCounterValue<int>(Category, "Resource Data CPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public void ComputeTotal()
		{
			TotalCPU.Value = InstanceDataCPU.Value + MiscCPU.Value + CullingBatchingCPU.Value + ResourceDataCPU.Value;
			TotalGPU.Value = InstanceDataGPU.Value + MiscGPU.Value + CullingBatchingGPU.Value;
		}
	}

	private const string kTotalCPU = "Total CPU";

	private const string kTotalGPU = "Total GPU";

	private const ProfilerCounterOptions kDefaultCounterOptions = ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush;

	private static readonly ProfilerCategory Category = ProfilerCategory.Scripts;

	public static readonly CounterCollection Collection = new CounterCollection();

	public static readonly ProfilerCounterValue<int> TotalCPU = new ProfilerCounterValue<int>(Category, "Total CPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly ProfilerCounterValue<int> TotalGPU = new ProfilerCounterValue<int>(Category, "Total GPU", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	public static readonly string[] AutoEnabledCategoryNames = new string[1] { ProfilerCategory.Scripts.Name };
}
