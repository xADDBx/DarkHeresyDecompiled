using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Code.View.Bridge.Data;

public class CharGenConfig
{
	public readonly BaseUnitEntity Unit;

	public readonly CharGenMode Mode;

	public readonly CharGenCompanionType CompanionType;

	private readonly bool m_IsCustomCompanionChargen;

	public Action OnClose { get; private set; }

	public Action<BaseUnitEntity> OnComplete { get; private set; }

	public Action EnterNewGameAction { get; private set; }

	public CharGenSoundActions SoundActions { get; private set; }

	public Action OnShowNewGameAction { get; private set; }

	private bool IsUIForbidden => false;

	public static CharGenConfig Create(BaseUnitEntity unit, CharGenMode mode, CharGenCompanionType companionType = CharGenCompanionType.Common, bool isCustomCompanionChargen = false)
	{
		return new CharGenConfig(unit, mode, companionType, isCustomCompanionChargen);
	}

	private CharGenConfig(BaseUnitEntity unit, CharGenMode mode, CharGenCompanionType companionType, bool isCustomCompanionChargen)
	{
		Unit = unit;
		Mode = mode;
		CompanionType = companionType;
		m_IsCustomCompanionChargen = isCustomCompanionChargen;
	}

	public CharGenConfig SetOnClose(Action onClose)
	{
		OnClose = onClose;
		return this;
	}

	public CharGenConfig SetSoundActions(CharGenSoundActions soundActions)
	{
		SoundActions = soundActions;
		return this;
	}

	public CharGenConfig SetOnShowNewGameAction(Action onShowNewGameAction)
	{
		OnShowNewGameAction = onShowNewGameAction;
		return this;
	}

	public CharGenConfig SetOnComplete(Action<BaseUnitEntity> onComplete)
	{
		OnComplete = onComplete;
		return this;
	}

	public CharGenConfig SetEnterNewGameAction(Action enterNewGameAction)
	{
		EnterNewGameAction = enterNewGameAction;
		return this;
	}

	public void OpenUI()
	{
		if (!IsUIForbidden)
		{
			EventBus.RaiseEvent(delegate(ICharGenInitiateUIHandler h)
			{
				h.HandleStartCharGen(this, m_IsCustomCompanionChargen);
			});
		}
	}
}
