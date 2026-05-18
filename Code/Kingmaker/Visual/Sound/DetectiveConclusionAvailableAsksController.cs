using System.Collections.Generic;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Visual.Sound;

public class DetectiveConclusionAvailableAsksController : BaseAsksController, IConclusionStatusChanged, ISubscriber, IClueAddendumStatusChanged, IClueStatusChanged
{
	private HashSet<BlueprintConclusion> m_CachedConclusions = new HashSet<BlueprintConclusion>();

	private void CacheConclusions()
	{
		foreach (BlueprintCase item in Game.Instance.DetectiveSystem.GetCasesWithStatus(CaseStatus.Opened))
		{
			m_CachedConclusions.AddRange(Game.Instance.DetectiveSystem.GetAvailableConclusions(item));
		}
	}

	public void HandleConclusionStatusChanged(BlueprintConclusion blueprint)
	{
		CheckConclusions();
	}

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		CheckConclusions();
	}

	public void HandleClueStatusChanged(BlueprintClue blueprint)
	{
		CheckConclusions();
	}

	private void CheckConclusions()
	{
		if (m_CachedConclusions.Count == 0)
		{
			CacheConclusions();
		}
		List<BlueprintConclusion> list = new List<BlueprintConclusion>();
		foreach (BlueprintCase item in Game.Instance.DetectiveSystem.GetCasesWithStatus(CaseStatus.Opened))
		{
			foreach (BlueprintConclusion availableConclusion in Game.Instance.DetectiveSystem.GetAvailableConclusions(item))
			{
				if (!m_CachedConclusions.Contains(availableConclusion))
				{
					list.Add(availableConclusion);
				}
			}
		}
		if (list.Count > 0)
		{
			ScheduleAsk();
		}
		m_CachedConclusions.AddRange(list);
	}

	private void ScheduleAsk()
	{
		PartDetectiveServoSkull.Find()?.Owner.View.Asks?.ConclusionAvailable.Schedule();
	}
}
