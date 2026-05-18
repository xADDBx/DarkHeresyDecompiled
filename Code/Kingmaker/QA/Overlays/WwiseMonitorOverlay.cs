using System;
using Owlcat.Core.Overlays;
using R3;

namespace Kingmaker.QA.Overlays;

public class WwiseMonitorOverlay : Overlay
{
	private const int PhysicalVoicesLimit = 100;

	private static AkResourceMonitorDataSummary s_LastDataSummary = new AkResourceMonitorDataSummary();

	private IDisposable m_MonitorTick;

	public WwiseMonitorOverlay()
		: base("Wwise", CreateElements())
	{
		StartMonitor();
	}

	private void StartMonitor()
	{
		AkUnitySoundEngine.StartResourceMonitoring();
		m_MonitorTick = ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			Tick();
		});
	}

	private void StopMonitor()
	{
		m_MonitorTick.Dispose();
		AkUnitySoundEngine.StopResourceMonitoring();
	}

	private void Tick()
	{
		AkUnitySoundEngine.GetResourceMonitorDataSummary(s_LastDataSummary);
		if (s_LastDataSummary.physicalVoices > 100)
		{
			QAModeExceptionReporter.MaybeShowError("[AUDIO] To many active physical voices may lead to a crash on consoles");
		}
	}

	private static OverlayElement[] CreateElements()
	{
		return new OverlayElement[5]
		{
			new Label("Physical Voices", () => s_LastDataSummary.physicalVoices.ToString()),
			new Label("Virtual Voices", () => s_LastDataSummary.virtualVoices.ToString()),
			new Label("Total Voices", () => s_LastDataSummary.totalVoices.ToString()),
			new Label("Total CPU time percent", () => s_LastDataSummary.totalCPU.ToString()),
			new Label("Total Active Events", () => s_LastDataSummary.nbActiveEvents.ToString())
		};
	}
}
