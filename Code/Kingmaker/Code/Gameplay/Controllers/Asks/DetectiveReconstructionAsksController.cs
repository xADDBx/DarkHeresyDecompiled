using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Sound;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Code.Gameplay.Controllers.Asks;

public class DetectiveReconstructionAsksController : BaseAsksController, IUnlockHandler, ISubscriber, IEtudesUpdateHandler
{
	private AsksState m_State => Game.Instance.AsksState;

	private DetectiveAsksSettings m_Settings => DetectiveAsksSettings.Instance;

	public void OnEtudesUpdate()
	{
		CheckConditions();
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		CheckConditions();
	}

	private void CheckConditions()
	{
		bool flag = false;
		foreach (BpRef<DetectiveAskTriggerCondition> reconstructionReadyAskCondition in m_Settings.ReconstructionReadyAskConditions)
		{
			if (!m_State.IsAlreadyTriggered(reconstructionReadyAskCondition) && reconstructionReadyAskCondition.Blueprint.Conditions.Check())
			{
				m_State.AddTriggeredCondition(reconstructionReadyAskCondition);
				flag = true;
			}
		}
		if (flag)
		{
			PartDetectiveServoSkull.Find()?.Owner.View.Asks?.DetectiveReconstructionReady.Schedule();
		}
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
	}
}
