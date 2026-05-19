using Kingmaker.Settings;

namespace Kingmaker.Code.Middleware.Metrics;

public class PerformanceMetricsEvent : MetricsEvent
{
	protected override string Name => "performance";

	public PerformanceMetricsEvent PerformanceSession(string perf_session_name)
	{
		AddParam("perf_session_name", perf_session_name);
		return this;
	}

	public PerformanceMetricsEvent Location(string map_name)
	{
		AddParam("map_name", map_name);
		return this;
	}

	public PerformanceMetricsEvent FsrMode(FsrMode upscaler_mode)
	{
		AddParam("upscaler_mode", MetricsUtils.EnumToSnakeCase(upscaler_mode));
		return this;
	}

	public PerformanceMetricsEvent VSync(VSyncModeOptions vsync)
	{
		AddParam("vsync", MetricsUtils.EnumToSnakeCase(vsync));
		return this;
	}

	public PerformanceMetricsEvent ShadowQuality(QualityOptionDisactivatable shadow_quality)
	{
		AddParam("shadow_quality", MetricsUtils.EnumToSnakeCase(shadow_quality));
		return this;
	}

	public PerformanceMetricsEvent ScreenResolution(string resolution)
	{
		AddParam("resolution", resolution);
		return this;
	}

	public PerformanceMetricsEvent CPU(string cpu)
	{
		AddParam("cpu", cpu);
		return this;
	}

	public PerformanceMetricsEvent GPU(string gpu)
	{
		AddParam("gpu", gpu);
		return this;
	}

	public PerformanceMetricsEvent Duration(int duration)
	{
		AddParam("duration", duration.ToString());
		return this;
	}

	public PerformanceMetricsEvent Ram(int ram)
	{
		AddParam("ram", ram.ToString());
		return this;
	}

	public PerformanceMetricsEvent AddFpsStats(int from, int to, int stat)
	{
		AddParam($"fps_{from}_{to}", stat.ToString());
		return this;
	}
}
