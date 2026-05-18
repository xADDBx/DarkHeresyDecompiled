using System;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[Serializable]
[ComponentName("Actions/PlayCutsceneStage")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("f7abb4e814fb418a86bcd5083bf8ad65")]
public class PlayCutsceneStage : GameAction, ICutsceneReference
{
	[ValidateNotNull]
	[SerializeField]
	private CutsceneReference m_Cutscene;

	[SerializeField]
	private int m_Stage = 2;

	[Tooltip("If set, cutscene will stop all other running stages")]
	[SerializeField]
	private bool m_StopOtherPlayingStages = true;

	public override string GetCaption()
	{
		return $"Play stage {m_Stage} of cts {m_Cutscene?.NameSafe()}";
	}

	protected override void RunAction()
	{
		BlueprintCutscene cutscene = m_Cutscene?.Get();
		if (cutscene == null)
		{
			PFLog.Cutscene.Error("Cutscene " + m_Cutscene?.NameSafe() + " not found");
			return;
		}
		foreach (CutscenePlayerData item in Game.Instance.EntityPools.Cutscenes.Where((CutscenePlayerData p) => p.Cutscene == cutscene))
		{
			item.StartCutsceneStage(m_Stage, m_StopOtherPlayingStages);
		}
	}

	public bool GetUsagesFor(BlueprintCutscene cutscene)
	{
		return cutscene == m_Cutscene.Get();
	}
}
