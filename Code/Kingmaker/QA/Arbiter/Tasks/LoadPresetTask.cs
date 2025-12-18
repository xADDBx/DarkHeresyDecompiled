using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.QA.Arbiter.Tasks;

public class LoadPresetTask : ArbiterTask
{
	private BlueprintAreaPreset m_Preset;

	public LoadPresetTask(ArbiterTask parentTask, BlueprintAreaPreset preset)
		: base(parentTask)
	{
		m_Preset = preset;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		base.Status = "Load preset '" + m_Preset.Area.Name + "'";
		using (ArbiterMeasurementTimer.StartTimer("StartNewGameFromPreset"))
		{
			Runner.ClearError();
			m_Preset.OverrideGameDifficulty = ConfigRoot.Instance.DifficultyList.CoreDifficulty;
			Game.Instance.RootUIContext.EnterGame(delegate
			{
				Game.Instance.LoadNewGame(m_Preset);
			});
			yield return new GameLoadingWaitTask(this);
			base.Status = "Preset '" + m_Preset.Area.Name + "' loaded";
		}
	}
}
