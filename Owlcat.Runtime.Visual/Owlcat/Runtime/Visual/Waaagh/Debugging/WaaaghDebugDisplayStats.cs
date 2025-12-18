using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Owlcat.Runtime.Visual.Waaagh.Debugging.DisplayStats;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

internal sealed class WaaaghDebugDisplayStats : ExtendedDebugDisplayStats
{
	private readonly DebugFrameTiming m_DebugFrameTiming = new DebugFrameTiming();

	private readonly CategoryInfo<WaaaghProfileCategory>[] m_ProfileCategoryInfos;

	private List<ExtendedDebugDisplayStatMarker> m_RecordedMarkers = new List<ExtendedDebugDisplayStatMarker>();

	public WaaaghDebugDisplayStats()
	{
		m_ProfileCategoryInfos = Enum.GetValues(typeof(WaaaghProfileCategory)).Cast<WaaaghProfileCategory>().Select(delegate(WaaaghProfileCategory category)
		{
			CategoryInfo<WaaaghProfileCategory> result = default(CategoryInfo<WaaaghProfileCategory>);
			result.Category = category;
			Type profileCategoryType = typeof(WaaaghProfileCategory);
			MemberInfo element = profileCategoryType.GetMember(category.ToString()).First((MemberInfo m) => m.DeclaringType == profileCategoryType);
			Attribute customAttribute = Attribute.GetCustomAttribute(element, typeof(DisplayInfoAttribute));
			result.Name = ((customAttribute is DisplayInfoAttribute displayInfoAttribute) ? displayInfoAttribute.name : category.ToString());
			Attribute customAttribute2 = Attribute.GetCustomAttribute(element, typeof(TooltipAttribute));
			result.Tooltip = ((customAttribute2 is TooltipAttribute tooltipAttribute) ? tooltipAttribute.tooltip : string.Empty);
			return result;
		})
			.ToArray();
	}

	public override void EnableProfilingRecorders()
	{
		m_RecordedMarkers = ExtendedDebugDisplayStatMarker.CreateMany<WaaaghProfileId>();
	}

	public override void DisableProfilingRecorders()
	{
		foreach (ExtendedDebugDisplayStatMarker recordedMarker in m_RecordedMarkers)
		{
			recordedMarker.Sampler.enableRecording = false;
		}
		m_RecordedMarkers.Clear();
	}

	public override void RegisterDebugUI(List<DebugUI.Widget> list)
	{
		m_DebugFrameTiming.RegisterDebugUI(list);
		list.Add(BuildSummaryFoldout());
		list.Add(BuildDetailedStatsFoldout());
	}

	private DebugUI.Widget BuildSummaryFoldout()
	{
		CategoryInfo<WaaaghProfileCategory>[] categoryInfos = m_ProfileCategoryInfos.Where((CategoryInfo<WaaaghProfileCategory> info) => info.Category != WaaaghProfileCategory.Misc).ToArray();
		return new DebugUI.Foldout
		{
			displayName = "Summary",
			opened = false,
			children = 
			{
				(DebugUI.Widget)new DebugUI.EnumField
				{
					displayName = "View Mode",
					autoEnum = typeof(SummaryStatsViewMode),
					getter = () => (int)SummaryViewMode,
					getIndex = () => (int)SummaryViewMode,
					setIndex = delegate(int v)
					{
						SummaryViewMode = (SummaryStatsViewMode)v;
					},
					setter = delegate
					{
					}
				},
				BuildStatsSummaryList("Categories", m_RecordedMarkers, categoryInfos)
			}
		};
	}

	private DebugUI.Foldout BuildDetailedStatsFoldout()
	{
		DebugUI.Foldout foldout = new DebugUI.Foldout
		{
			displayName = "Detailed Stats",
			opened = false,
			children = 
			{
				(DebugUI.Widget)new DebugUI.BoolField
				{
					displayName = "Update every second with average",
					getter = () => AverageProfilerTimingsOverASecond,
					setter = delegate(bool value)
					{
						AverageProfilerTimingsOverASecond = value;
					}
				},
				(DebugUI.Widget)new DebugUI.BoolField
				{
					displayName = "Hide empty scopes",
					tooltip = "Hide profiling scopes where elapsed time in each category is zero",
					getter = () => HideEmptyScopes,
					setter = delegate(bool value)
					{
						HideEmptyScopes = value;
					}
				},
				(DebugUI.Widget)new DebugUI.EnumField
				{
					displayName = "View Mode",
					tooltip = "All - display all samplers together, Categorized - group samplers by category.",
					autoEnum = typeof(DetailedStatsViewMode),
					getter = () => (int)ViewMode,
					getIndex = () => (int)ViewMode,
					setter = delegate
					{
					},
					setIndex = delegate(int value)
					{
						ViewMode = (DetailedStatsViewMode)value;
					}
				}
			}
		};
		DebugUI.Widget widget = BuildDetailedStatsList("Profiling Scopes", m_RecordedMarkers);
		widget.isHiddenCallback = () => ViewMode != DetailedStatsViewMode.All;
		foldout.children.Add(widget);
		CategoryInfo<WaaaghProfileCategory>[] profileCategoryInfos = m_ProfileCategoryInfos;
		for (int i = 0; i < profileCategoryInfos.Length; i++)
		{
			CategoryInfo<WaaaghProfileCategory> categoryInfo = profileCategoryInfos[i];
			DebugUI.Widget widget2 = BuildDetailedStatsList("Profiling Scopes (" + categoryInfo.Name + ")", m_RecordedMarkers, categoryInfo.Category);
			widget2.isHiddenCallback = () => ViewMode != DetailedStatsViewMode.Categorized;
			foldout.children.Add(widget2);
		}
		return foldout;
	}

	public override void Update()
	{
		m_DebugFrameTiming.UpdateFrameTiming();
		UpdateDetailedStats(m_RecordedMarkers);
	}
}
