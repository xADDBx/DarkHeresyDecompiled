using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("d1dfc17d49354a9b980afe5f01bac608")]
public class EtudeBracketUnlockableFlagTrigger : EtudeBracketTrigger, IUnlockHandler, ISubscriber, IUnlockValueHandler
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnlockableFlagReference m_Flag;

	public bool RunActionsOnEnter;

	public ActionList OnUnlocked;

	public ActionList OnLocked;

	public ActionList OnChanged;

	public BlueprintUnlockableFlag Flag => m_Flag;

	void IUnlockHandler.HandleUnlock(BlueprintUnlockableFlag flag)
	{
		if (flag == Flag)
		{
			OnUnlocked.Run();
		}
	}

	void IUnlockHandler.HandleLock(BlueprintUnlockableFlag flag)
	{
		if (flag == Flag)
		{
			OnLocked.Run();
		}
	}

	void IUnlockValueHandler.HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		if (flag == Flag)
		{
			OnChanged.Run();
		}
	}

	protected override void OnEnter()
	{
		if (RunActionsOnEnter)
		{
			if (Flag.IsUnlocked)
			{
				OnUnlocked.Run();
			}
			else
			{
				OnLocked.Run();
			}
			OnChanged.Run();
		}
	}
}
