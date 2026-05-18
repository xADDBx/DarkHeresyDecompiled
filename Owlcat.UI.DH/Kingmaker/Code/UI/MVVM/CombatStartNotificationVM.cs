using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartNotificationVM : ViewModel, IGameModeHandler, ISubscriber
{
	private readonly Action m_ClosedCallback;

	private readonly ReactiveProperty<bool> m_CanShow;

	public readonly string HeaderText;

	public ReadOnlyReactiveProperty<bool> CanShow => m_CanShow;

	public CombatStartNotificationVM(Action closedCallback)
	{
		m_ClosedCallback = closedCallback;
		m_CanShow = new ReactiveProperty<bool>(Game.Instance.CurrentModeType == GameModeType.Default).AddTo(this);
		HeaderText = UIStrings.Instance.TurnBasedTexts.BattleBegins;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Close()
	{
		m_ClosedCallback();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_CanShow.Value = Game.Instance.CurrentModeType == GameModeType.Default;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
