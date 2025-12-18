using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class PauseNotificationVM : ViewModel, IPauseHandler, ISubscriber, IAreaHandler, IFullScreenUIHandler
{
	private readonly ReactiveProperty<bool> m_IsPaused;

	private readonly ReactiveCommand<bool> m_ChangeAlphaPause;

	public ReadOnlyReactiveProperty<bool> IsPaused => m_IsPaused;

	public Observable<bool> ChangeAlphaPause => m_ChangeAlphaPause;

	public PauseNotificationVM()
	{
		m_IsPaused = new ReactiveProperty<bool>(IsGamePaused()).AddTo(this);
		m_ChangeAlphaPause = new ReactiveCommand<bool>().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Unpause()
	{
		Game.Instance.PauseBind();
	}

	public void OnPauseToggled()
	{
		m_IsPaused.Value = IsGamePaused();
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		m_ChangeAlphaPause.Execute(IsGamePaused());
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_ChangeAlphaPause.Execute(!state && IsGamePaused());
	}

	private bool IsGamePaused()
	{
		if (Game.Instance.IsPaused)
		{
			return !Game.Instance.Controllers.PauseController.IsPausedByPlayers;
		}
		return false;
	}
}
