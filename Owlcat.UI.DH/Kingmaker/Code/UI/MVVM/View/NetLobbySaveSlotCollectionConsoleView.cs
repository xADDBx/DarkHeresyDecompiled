using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbySaveSlotCollectionConsoleView : NetLobbySaveSlotCollectionBaseView, ISavesUpdatedHandler, ISubscriber
{
	private readonly ReactiveProperty<bool> m_ShowWaitingSaveAnim = new ReactiveProperty<bool>(value: false);

	protected override void OnBind()
	{
		base.OnBind();
		m_ShowWaitingSaveAnim.Value = true;
		CreateInputImpl();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ShowWaitingSaveAnim.Value = false;
	}

	public void CreateInputImpl()
	{
	}

	public void OnSaveListUpdated()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			m_ShowWaitingSaveAnim.Value = false;
		}));
	}
}
