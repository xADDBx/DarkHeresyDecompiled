using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging.DisplayStats;

internal sealed class ExtendedDebugDisplaySettingsStats : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	[DisplayInfo(name = "Display Stats", order = int.MinValue)]
	private class StatsPanel : UnityEngine.Rendering.DebugDisplaySettingsPanel
	{
		private readonly ExtendedDebugDisplaySettingsStats m_Data;

		public override DebugUI.Flags Flags => DebugUI.Flags.RuntimeOnly;

		public StatsPanel(ExtendedDebugDisplaySettingsStats displaySettingsStats)
		{
			m_Data = displaySettingsStats;
			m_Data.debugDisplayStats.EnableProfilingRecorders();
			List<DebugUI.Widget> list = new List<DebugUI.Widget>();
			m_Data.debugDisplayStats.RegisterDebugUI(list);
			foreach (DebugUI.Widget item in list)
			{
				AddWidget(item);
			}
		}

		public override void Dispose()
		{
			m_Data.debugDisplayStats.DisableProfilingRecorders();
			base.Dispose();
		}
	}

	public ExtendedDebugDisplayStats debugDisplayStats { get; }

	public bool AreAnySettingsActive => false;

	public ExtendedDebugDisplaySettingsStats(ExtendedDebugDisplayStats debugDisplayStats)
	{
		this.debugDisplayStats = debugDisplayStats;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new StatsPanel(this);
	}
}
