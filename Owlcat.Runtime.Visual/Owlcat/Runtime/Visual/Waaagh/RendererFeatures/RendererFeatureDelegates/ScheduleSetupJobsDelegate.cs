using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.RendererFeatureDelegates;

public delegate JobHandle ScheduleSetupJobsDelegate(in SetupContext context, JobHandle dependency);
