using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatEndWindowVM : ViewModel, IGameModeHandler, ISubscriber
{
	private readonly CombatEndReason m_CombatEndReason;

	private Action<bool> m_CloseCallback;

	public readonly bool CloseByTimer;

	public ModalWindowVM ModalWindowVM { get; }

	public CombatEndWindowVM(Action<bool> closeCallback, CombatEndReason reason)
	{
		m_CloseCallback = closeCallback;
		m_CombatEndReason = reason;
		ModalWindowVM = GetModalWindowVM(reason).AddTo(this);
		CloseByTimer = reason != CombatEndReason.MoraleVictory;
		if (reason == CombatEndReason.MoraleVictory)
		{
			Game.Instance.RequestPauseUi(isPaused: true);
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Close(bool endCombat)
	{
		m_CloseCallback?.Invoke(endCombat);
		m_CloseCallback = null;
	}

	protected override void OnDispose()
	{
		if (!CloseByTimer)
		{
			Game.Instance.RequestPauseUi(isPaused: false);
		}
	}

	private ModalWindowVM GetModalWindowVM(CombatEndReason combatEndReason)
	{
		UICombatEndWindowTexts instance = UICombatEndWindowTexts.Instance;
		if (combatEndReason == CombatEndReason.MoraleVictory)
		{
			ModalWindowAction[] actions = new ModalWindowAction[2]
			{
				new ModalWindowAction
				{
					Name = instance.ExecuteEnemiesButton.Text,
					Action = delegate
					{
						Close(endCombat: true);
					}
				},
				new ModalWindowAction
				{
					Name = instance.ContinueCombatButton.Text,
					Action = delegate
					{
						Close(endCombat: false);
					}
				}
			};
			return new ModalWindowVM(instance.MoraleVictoryTitle.Text, instance.MoraleVictoryDescription.Text, actions);
		}
		return new ModalWindowVM(instance.VictoryTitle);
	}

	void IGameModeHandler.OnGameModeStart(GameModeType gameMode)
	{
		if (!(gameMode == GameModeType.Default) && m_CombatEndReason == CombatEndReason.RegularVictory)
		{
			Close(endCombat: true);
		}
	}

	void IGameModeHandler.OnGameModeStop(GameModeType gameMode)
	{
	}
}
