using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Visual.Sound;

public class DetectiveStudyAvailableAsksController : BaseAsksController, ITickUnitAsksController, IUnitAsksController, IDisposable, IStudyConditionUnlocked, ISubscriber
{
	private List<BlueprintClueStudy> m_StudiesCache = new List<BlueprintClueStudy>();

	public void HandleStudyUnlockedCondition(BlueprintClueStudy study)
	{
		if (study.StudyCompanion != null && Game.Instance.DetectiveSystem.IsAvailableForStudy(study))
		{
			m_StudiesCache.Add(study);
		}
	}

	public void Tick()
	{
		if (m_StudiesCache.Count == 0)
		{
			return;
		}
		BlueprintClueStudy rndStudy = m_StudiesCache.Where((BlueprintClueStudy x) => x.StudyCompanion != null).Random(PFStatefulRandom.Bark);
		if (rndStudy == null)
		{
			m_StudiesCache.Clear();
			return;
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.Party.FirstOrDefault((BaseUnitEntity p) => p.Blueprint == rndStudy.StudyCompanion.Blueprint);
		if (baseUnitEntity == null)
		{
			m_StudiesCache.Clear();
			return;
		}
		baseUnitEntity.View.Asks?.DetectiveCanStudyClue.Schedule();
		m_StudiesCache.Clear();
	}
}
