using System;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Core.Overlays;
using Pathfinding;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.Utility;

public class FPSCounter : MonoBehaviour
{
	[Range(1f, 60f)]
	public int updatesPerSecond = 10;

	[Tooltip("Offset to max system used memory to display warning. In megabytes.")]
	public int systemMemoryWarnOffsetMb = 700;

	public bool Clear;

	private int m_TickCount;

	private float m_DeltaTimeAccumulator;

	private float m_FPS;

	private int m_FPSChecksCount;

	private float m_FPSTotal;

	private float m_FPSMax;

	private float m_FPSMin = 100000000f;

	private float m_FPSMedian;

	private float m_MS;

	private float m_MSTotal;

	private float m_MSMax;

	private float m_MSMin = 100000000f;

	private float m_MSMedian;

	private long m_MaxSystemMemory;

	private double m_StatAvgFrameMs;

	private double m_StatPeakFrameMs;

	private double m_StatAvgPerCallMs;

	private int m_StatFrameCount;

	private double m_StatTotalFrameMs;

	private long m_StatTotalCalls;

	private double m_StatTotalTimeMs;

	private void Start()
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		m_TickCount = 0;
		m_DeltaTimeAccumulator = 0f;
		m_FPS = 0f;
		Overlay o = new Overlay("FPS", new Label("FPS", () => m_FPS.ToString("0.00")), new Label("FPS MIN", () => m_FPSMin.ToString("0.00")), new Label("FPS MAX", () => m_FPSMax.ToString("0.00")), new Label("FPS MED", () => m_FPSMedian.ToString("0.00"))
		{
			AddSeparator = true
		}, new Label("MS", () => m_MS.ToString("0.00")), new Label("MS MIN", () => m_MSMin.ToString("0.00")), new Label("MS MAX", () => m_MSMax.ToString("0.00")), new Label("MS MED", () => m_MSMedian.ToString("0.00"))
		{
			AddSeparator = true
		}, new Label("GFX MEM", () => MemString(Profiler.GetAllocatedMemoryForGraphicsDriver())), new Label("Native MEM", () => MemString(Profiler.GetTotalAllocatedMemoryLong()) + " + " + MemString(Profiler.GetTotalUnusedReservedMemoryLong()) + " = " + MemString(Profiler.GetTotalReservedMemoryLong())), new Label("Script MEM", () => MemString(Profiler.GetMonoUsedSizeLong()) + " + " + MemString(Profiler.GetMonoHeapSizeLong() - Profiler.GetMonoUsedSizeLong()) + " = " + MemString(Profiler.GetMonoHeapSizeLong())), MemoryLabelWithLimit("SYSTEM MEM", delegate
		{
			MemoryUsageHelper.MemoryStatsProvider stats = MemoryUsageHelper.Stats;
			return (Current: stats.SystemMemoryUsed, Max: stats.SystemMemoryLimit, Peak: stats.SystemMemoryUsedPeak);
		}, systemMemoryWarnOffsetMb * 1024 * 1024), new Label("NODE INDEX", GetNodeUnderPointerInfo)
		{
			AddSeparator = true
		}, new Label("STAT AVG", () => $"{m_StatAvgFrameMs:F3} ms/frame"), new Label("STAT PEAK", () => $"{m_StatPeakFrameMs:F3} ms/frame"), new Label("STAT AVG/CALL", () => $"{m_StatAvgPerCallMs:F4} ms/call"));
		OverlayService.Instance?.RegisterOverlay(o);
		m_MaxSystemMemory = MemoryUsageHelper.Stats.SystemMemoryLimit;
	}

	private Label MemoryLabelWithLimit(string name, Func<(long Current, long Max, long Peak)> valueGetter, int warnOffset, int errorOffset = 0)
	{
		return new Label(name, delegate
		{
			var (bytes, bytes2, num3) = valueGetter();
			return (num3 <= 0) ? (MemString(bytes) + " / " + MemString(bytes2)) : (MemString(bytes) + " (" + MemString(num3) + ") / " + MemString(bytes2));
		}, delegate
		{
			var (num, num2, _) = valueGetter();
			if (num >= num2 - errorOffset)
			{
				return Label.Severity.Error;
			}
			return (num >= num2 - warnOffset) ? Label.Severity.Warning : Label.Severity.Info;
		});
	}

	private string GetNodeUnderPointerInfo()
	{
		PointerController controller = Game.Instance.GetController<PointerController>();
		if (controller == null)
		{
			return "(-, -)";
		}
		GridNodeBase nearestNodeXZUnwalkable = controller.WorldPosition.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null)
		{
			return "(-, -)";
		}
		return $"({nearestNodeXZUnwalkable.XCoordinateInGrid}, {nearestNodeXZUnwalkable.ZCoordinateInGrid})";
	}

	private static string MemString(long bytes)
	{
		return ((float)bytes / 1024f / 1024f).ToString("#.0");
	}

	private void Update()
	{
		MechanicActor.FlushFrameCounters(out var callCount, out var totalMs);
		if (callCount > 0)
		{
			m_StatFrameCount++;
			m_StatTotalFrameMs += totalMs;
			m_StatTotalCalls += callCount;
			m_StatTotalTimeMs += totalMs;
			m_StatAvgFrameMs = m_StatTotalFrameMs / (double)m_StatFrameCount;
			if (totalMs > m_StatPeakFrameMs)
			{
				m_StatPeakFrameMs = totalMs;
			}
			m_StatAvgPerCallMs = ((m_StatTotalCalls > 0) ? (m_StatTotalTimeMs / (double)m_StatTotalCalls) : 0.0);
		}
		m_TickCount++;
		m_DeltaTimeAccumulator += Time.unscaledDeltaTime;
		if (m_DeltaTimeAccumulator > 1f / (float)updatesPerSecond)
		{
			m_FPS = (float)m_TickCount / m_DeltaTimeAccumulator;
			m_TickCount = 0;
			m_DeltaTimeAccumulator -= 1f / (float)updatesPerSecond;
			m_MS = m_DeltaTimeAccumulator * 1000f;
			if (m_FPS < m_FPSMin)
			{
				m_FPSMin = m_FPS;
			}
			if (m_FPS > m_FPSMax)
			{
				m_FPSMax = m_FPS;
			}
			if (m_MS < m_MSMin)
			{
				m_MSMin = m_MS;
			}
			if (m_MS > m_MSMax)
			{
				m_MSMax = m_MS;
			}
			m_FPSTotal += m_FPS;
			m_MSTotal += m_MS;
			if (m_FPSChecksCount > 0)
			{
				m_FPSMedian = m_FPSTotal / (float)m_FPSChecksCount;
				m_MSMedian = m_MSTotal / (float)m_FPSChecksCount;
			}
			if (Clear)
			{
				m_FPSMax = 0f;
				m_FPSMin = 100000000f;
				m_MSMax = 0f;
				m_MSMin = 100000000f;
				m_FPSChecksCount = 0;
				m_FPSTotal = 0f;
				m_MSTotal = 0f;
				m_StatAvgFrameMs = 0.0;
				m_StatPeakFrameMs = 0.0;
				m_StatAvgPerCallMs = 0.0;
				m_StatFrameCount = 0;
				m_StatTotalFrameMs = 0.0;
				m_StatTotalCalls = 0L;
				m_StatTotalTimeMs = 0.0;
				Clear = false;
			}
			m_FPSChecksCount++;
		}
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
		{
			Clear = true;
		}
		if (Input.GetKeyDown(KeyCode.F11))
		{
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
			{
				Clear = true;
			}
			else
			{
				OverlayService.Instance?.Next();
			}
		}
	}
}
