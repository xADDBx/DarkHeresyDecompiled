using System;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.AreaLogic.Cutscenes;

internal class CutsceneEntry
{
	[NotNull]
	private readonly EntityRef<CutscenePlayerData> m_Cutscene;

	public int OwnersCount;

	private bool m_Paused;

	public CutscenePlayerData Cutscene => m_Cutscene.Entity;

	public CutsceneEntry([NotNull] CutscenePlayerData cutscene)
	{
		m_Cutscene = cutscene;
		OwnersCount = 1;
	}

	public void PauseOrStop()
	{
		if (!m_Paused)
		{
			m_Paused = true;
			switch (Cutscene.Cutscene.MarkedUnitHandling)
			{
			case BlueprintCutscene.MarkedUnitHandlingType.Pause:
			case BlueprintCutscene.MarkedUnitHandlingType.PauseAndRestart:
				Cutscene.SetPaused(value: true, CutscenePauseReason.MarkedUnitControlledByOtherCutscene);
				break;
			case BlueprintCutscene.MarkedUnitHandlingType.Stop:
				Cutscene.Stop();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void Resume()
	{
		if (!m_Paused || Cutscene == null)
		{
			return;
		}
		m_Paused = false;
		if (Cutscene.Cutscene.MarkedUnitHandling == BlueprintCutscene.MarkedUnitHandlingType.PauseAndRestart)
		{
			Cutscene.Stop();
			using (Cutscene.Parameters.RequestContextData())
			{
				CutscenePlayerView.Play(Cutscene.Cutscene, Cutscene.ParameterSetter, queued: false, Cutscene.HoldingState);
				return;
			}
		}
		Cutscene.SetPaused(value: false, CutscenePauseReason.MarkedUnitControlledByOtherCutscene);
	}
}
