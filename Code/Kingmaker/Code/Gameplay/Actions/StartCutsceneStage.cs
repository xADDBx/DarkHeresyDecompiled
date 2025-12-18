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
[ComponentName("Actions/Start Cutscene Stage")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("f7abb4e814fb418a86bcd5083bf8ad65")]
public class StartCutsceneStage : GameAction, ICutsceneReference
{
	[ValidateNotNull]
	[SerializeField]
	private CutsceneReference m_Cutscene;

	[SerializeField]
	private int m_Stage = 2;

	public override string GetCaption()
	{
		return $"Start cts stage {m_Stage}";
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
			item.StartCutsceneStage(m_Stage);
		}
	}

	public bool GetUsagesFor(BlueprintCutscene cutscene)
	{
		return cutscene == m_Cutscene.Get();
	}
}
