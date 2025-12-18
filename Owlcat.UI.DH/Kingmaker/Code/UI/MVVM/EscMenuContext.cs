using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EscMenuContext : ViewModel, IEscMenuHandler, ISubscriber
{
	private readonly ReactiveProperty<EscMenuVM> m_EscMenu;

	public bool IsEscMenuActive => m_EscMenu.CurrentValue != null;

	public EscMenuContext(ReactiveProperty<EscMenuVM> escMenu)
	{
		m_EscMenu = escMenu;
		EventBus.Subscribe(this).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(RequestEscMenu).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeEscMenu();
	}

	private void RequestEscMenu()
	{
		if (!Game.Instance.TutorialSystem.HasShownData && !GameUIState.Instance.IsInMainMenu && !LoadingProcess.Instance.IsLoadingScreenActive && !RootUIContext.Instance.ServiceWindowNowIsOpening && m_EscMenu.CurrentValue == null)
		{
			m_EscMenu.Value = new EscMenuVM(DisposeEscMenu);
		}
	}

	public void DisposeEscMenu()
	{
		m_EscMenu.CurrentValue?.Dispose();
		m_EscMenu.Value = null;
	}

	void IEscMenuHandler.HandleOpen()
	{
		RequestEscMenu();
	}
}
