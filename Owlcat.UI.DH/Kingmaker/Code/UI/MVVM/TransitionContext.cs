using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionContext : ViewModel, IUIMultiEntranceHandler, ISubscriber
{
	private readonly ReactiveProperty<TransitionVM> m_TransitionVM;

	public TransitionContext(ReactiveProperty<TransitionVM> transitionVM)
	{
		m_TransitionVM = transitionVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleMultiEntranceUI(BlueprintMultiEntrance multiEntrance)
	{
		bool flag = true;
		if ((bool)ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current)
		{
			EntityRef<BaseUnitEntity> entityRef = ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current.EntityRef;
			flag = entityRef.IsNull || (!entityRef.IsNull && ((BaseUnitEntity)entityRef).IsDirectlyControllable());
		}
		if (flag)
		{
			m_TransitionVM.Value = new TransitionVM(multiEntrance, DisposeTransition).AddTo(this);
		}
	}

	private void DisposeTransition()
	{
		m_TransitionVM.Value?.Dispose();
		m_TransitionVM.Value = null;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		DisposeTransition();
	}
}
