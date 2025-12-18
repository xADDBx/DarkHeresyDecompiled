using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging.DisplayStats;

internal abstract class ExtendedDebugDisplayStats
{
	protected struct CategoryInfo<TEnum> where TEnum : Enum
	{
		public TEnum Category;

		public string Name;

		public string Tooltip;
	}

	protected enum DetailedStatsViewMode
	{
		All,
		Categorized
	}

	private class AccumulatedTiming
	{
		public float AccumulatedValue;

		public float LastAverage;

		internal void UpdateLastAverage(int frameCount)
		{
			LastAverage = AccumulatedValue / (float)frameCount;
			AccumulatedValue = 0f;
		}
	}

	private enum DebugProfilingType
	{
		CPU,
		InlineCPU,
		GPU
	}

	protected enum SummaryStatsViewMode
	{
		CPU,
		GPU
	}

	private const float kAccumulationTimeInSeconds = 1f;

	private const float kValueRefreshRate = 0.2f;

	private static readonly string[] s_DetailedStatsColumnLabels = new string[3] { "CPU", "CPUInline", "GPU" };

	private readonly Dictionary<ExtendedDebugDisplayStatMarker, AccumulatedTiming>[] m_AccumulatedTiming = new Dictionary<ExtendedDebugDisplayStatMarker, AccumulatedTiming>[3]
	{
		new Dictionary<ExtendedDebugDisplayStatMarker, AccumulatedTiming>(),
		new Dictionary<ExtendedDebugDisplayStatMarker, AccumulatedTiming>(),
		new Dictionary<ExtendedDebugDisplayStatMarker, AccumulatedTiming>()
	};

	private readonly HashSet<ExtendedDebugDisplayStatMarker> m_HiddenSamplers = new HashSet<ExtendedDebugDisplayStatMarker>();

	protected bool AverageProfilerTimingsOverASecond = true;

	protected bool HideEmptyScopes = true;

	private int m_AccumulatedFrames;

	private float m_TimeSinceLastAvgValue;

	protected SummaryStatsViewMode SummaryViewMode;

	protected DetailedStatsViewMode ViewMode = DetailedStatsViewMode.Categorized;

	public abstract void EnableProfilingRecorders();

	public abstract void DisableProfilingRecorders();

	public abstract void RegisterDebugUI(List<DebugUI.Widget> list);

	public abstract void Update();

	protected void UpdateDetailedStats(List<ExtendedDebugDisplayStatMarker> samplers)
	{
		m_HiddenSamplers.Clear();
		m_TimeSinceLastAvgValue += Time.unscaledDeltaTime;
		m_AccumulatedFrames++;
		bool flag = m_TimeSinceLastAvgValue >= 1f;
		UpdateListOfAveragedProfilerTimings(flag, samplers);
		if (flag)
		{
			m_TimeSinceLastAvgValue = 0f;
			m_AccumulatedFrames = 0;
		}
	}

	protected DebugUI.Widget BuildDetailedStatsList(string title, List<ExtendedDebugDisplayStatMarker> samplers, WaaaghProfileCategory? profileCategory = null)
	{
		return new DebugUI.Foldout(title, BuildProfilingSamplerWidgetList(samplers, profileCategory), s_DetailedStatsColumnLabels)
		{
			opened = true
		};
	}

	protected DebugUI.Widget BuildStatsSummaryList(string title, List<ExtendedDebugDisplayStatMarker> samplers, CategoryInfo<WaaaghProfileCategory>[] categoryInfos)
	{
		return new DebugUI.Foldout(title, BuildProfilingCategoryWidgetList(samplers, categoryInfos))
		{
			opened = true
		};
	}

	private void UpdateListOfAveragedProfilerTimings(bool needUpdatingAverages, List<ExtendedDebugDisplayStatMarker> samplers)
	{
		foreach (ExtendedDebugDisplayStatMarker sampler2 in samplers)
		{
			ProfilingSampler sampler = sampler2.Sampler;
			bool flag = true;
			if (m_AccumulatedTiming[0].TryGetValue(sampler2, out var value))
			{
				value.AccumulatedValue += sampler.cpuElapsedTime;
				flag &= value.AccumulatedValue == 0f;
			}
			if (m_AccumulatedTiming[1].TryGetValue(sampler2, out var value2))
			{
				value2.AccumulatedValue += sampler.inlineCpuElapsedTime;
				flag &= value2.AccumulatedValue == 0f;
			}
			if (m_AccumulatedTiming[2].TryGetValue(sampler2, out var value3))
			{
				value3.AccumulatedValue += sampler.gpuElapsedTime;
				flag &= value3.AccumulatedValue == 0f;
			}
			if (needUpdatingAverages)
			{
				value?.UpdateLastAverage(m_AccumulatedFrames);
				value2?.UpdateLastAverage(m_AccumulatedFrames);
				value3?.UpdateLastAverage(m_AccumulatedFrames);
			}
			if (flag)
			{
				m_HiddenSamplers.Add(sampler2);
			}
		}
	}

	private float GetSamplerTiming(ExtendedDebugDisplayStatMarker statMarker, ProfilingSampler sampler, DebugProfilingType type)
	{
		if (AverageProfilerTimingsOverASecond && m_AccumulatedTiming[(int)type].TryGetValue(statMarker, out var value))
		{
			return value.LastAverage;
		}
		return type switch
		{
			DebugProfilingType.GPU => sampler.gpuElapsedTime, 
			DebugProfilingType.CPU => sampler.cpuElapsedTime, 
			_ => sampler.inlineCpuElapsedTime, 
		};
	}

	private float GetTotalSamplersTiming(IEnumerable<ExtendedDebugDisplayStatMarker> samplers, DebugProfilingType type, WaaaghProfileCategory? profileCategory)
	{
		float num = 0f;
		foreach (ExtendedDebugDisplayStatMarker sampler2 in samplers)
		{
			if (!profileCategory.HasValue || sampler2.Category == profileCategory)
			{
				ProfilingSampler sampler = sampler2.Sampler;
				if (sampler != null)
				{
					num += GetSamplerTiming(sampler2, sampler, type);
				}
			}
		}
		return num;
	}

	private float GetTotalSamplersTiming(IEnumerable<ExtendedDebugDisplayStatMarker> samplers, DebugProfilingType type, CategoryInfo<WaaaghProfileCategory>[] categoryInfos)
	{
		float num = 0f;
		foreach (ExtendedDebugDisplayStatMarker sampler2 in samplers)
		{
			if (Contains(categoryInfos, sampler2.Category))
			{
				ProfilingSampler sampler = sampler2.Sampler;
				if (sampler != null)
				{
					num += GetSamplerTiming(sampler2, sampler, type);
				}
			}
		}
		return num;
		static bool Contains(CategoryInfo<WaaaghProfileCategory>[] categoryInfos, WaaaghProfileCategory category)
		{
			for (int i = 0; i < categoryInfos.Length; i++)
			{
				if (categoryInfos[i].Category == category)
				{
					return true;
				}
			}
			return false;
		}
	}

	private ObservableList<DebugUI.Widget> BuildProfilingCategoryWidgetList(List<ExtendedDebugDisplayStatMarker> samplers, CategoryInfo<WaaaghProfileCategory>[] categoryInfos)
	{
		ObservableList<DebugUI.Widget> result = new ObservableList<DebugUI.Widget>();
		AddProgressBars(samplers.ToArray(), SummaryStatsViewMode.CPU, new DebugProfilingType[2]
		{
			DebugProfilingType.CPU,
			DebugProfilingType.InlineCPU
		});
		AddProgressBars(samplers.ToArray(), SummaryStatsViewMode.GPU, new DebugProfilingType[1] { DebugProfilingType.GPU });
		return result;
		void AddProgressBars(ExtendedDebugDisplayStatMarker[] statMarkers, SummaryStatsViewMode viewMode, DebugProfilingType[] debugProfilingTypes)
		{
			CategoryInfo<WaaaghProfileCategory>[] array = categoryInfos;
			for (int i = 0; i < array.Length; i++)
			{
				CategoryInfo<WaaaghProfileCategory> categoryInfo = array[i];
				WaaaghProfileCategory category = categoryInfo.Category;
				result.Add(new DebugUI.ProgressBarValue
				{
					displayName = categoryInfo.Name,
					tooltip = categoryInfo.Tooltip,
					refreshRate = 0.2f,
					isHiddenCallback = () => SummaryViewMode != viewMode,
					getter = () => GetSamplersTimingRatio(statMarkers, category, categoryInfos, debugProfilingTypes)
				});
				result.Add();
			}
		}
	}

	private float GetSamplersTimingRatio(ExtendedDebugDisplayStatMarker[] statMarkers, WaaaghProfileCategory category, CategoryInfo<WaaaghProfileCategory>[] categoryInfos, DebugProfilingType[] debugProfilingTypes)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (DebugProfilingType type in debugProfilingTypes)
		{
			num += GetTotalSamplersTiming(statMarkers, type, category);
			num2 += GetTotalSamplersTiming(statMarkers, type, categoryInfos);
		}
		if (!(num2 > 0f))
		{
			return 0f;
		}
		return num / num2;
	}

	private ObservableList<DebugUI.Widget> BuildProfilingSamplerWidgetList(IEnumerable<ExtendedDebugDisplayStatMarker> samplers, WaaaghProfileCategory? profileCategory)
	{
		ObservableList<DebugUI.Widget> observableList = new ObservableList<DebugUI.Widget>();
		ExtendedDebugDisplayStatMarker[] statMarkers = samplers.ToArray();
		ExtendedDebugDisplayStatMarker[] array = statMarkers;
		foreach (ExtendedDebugDisplayStatMarker statMarker in array)
		{
			ProfilingSampler sampler = statMarker.Sampler;
			if ((!profileCategory.HasValue || statMarker.Category == profileCategory) && sampler != null)
			{
				sampler.enableRecording = true;
				observableList.Add(new DebugUI.ValueTuple
				{
					displayName = sampler.name,
					isHiddenCallback = () => (HideEmptyScopes && m_HiddenSamplers.Contains(statMarker)) ? true : false,
					values = (from e in AllProfilingTypes()
						select CreateWidgetForSampler(statMarker, sampler, e)).ToArray()
				});
			}
		}
		observableList.Add(new DebugUI.ValueTuple
		{
			displayName = "TOTAL",
			values = (from e in AllProfilingTypes()
				select CreateTotalWidget(statMarkers, e, profileCategory)).ToArray()
		});
		return observableList;
		DebugUI.Value CreateTotalWidget(ExtendedDebugDisplayStatMarker[] markers, DebugProfilingType type, WaaaghProfileCategory? category)
		{
			return new DebugUI.Value
			{
				formatString = "{0:F2}ms",
				refreshRate = 0.2f,
				getter = () => GetTotalSamplersTiming(markers, type, category)
			};
		}
		DebugUI.Value CreateWidgetForSampler(ExtendedDebugDisplayStatMarker marker, ProfilingSampler sampler, DebugProfilingType type)
		{
			Dictionary<ExtendedDebugDisplayStatMarker, AccumulatedTiming> dictionary = m_AccumulatedTiming[(int)type];
			if (!dictionary.ContainsKey(marker))
			{
				dictionary.Add(marker, new AccumulatedTiming());
			}
			return new DebugUI.Value
			{
				formatString = "{0:F2}ms",
				refreshRate = 0.2f,
				getter = () => GetSamplerTiming(marker, sampler, type)
			};
		}
	}

	private static IEnumerable<DebugProfilingType> AllProfilingTypes()
	{
		return Enum.GetValues(typeof(DebugProfilingType)).Cast<DebugProfilingType>();
	}
}
