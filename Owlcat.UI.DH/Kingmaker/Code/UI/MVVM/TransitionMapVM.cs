using System;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionMapVM : ViewModel, IAcceptChangeGroupHandler, ISubscriber
{
	public readonly TransitionMapBoardVM LocalVM;

	private readonly Action m_Close;

	public TransitionMapVM(BlueprintMultiEntrance entrance, Action close)
	{
		m_Close = close;
		LocalVM = ((entrance.Map == BlueprintMultiEntranceMap.Global) ? ((TransitionMapBoardVM)new TransitionMapGlobalVM(entrance, Close)) : ((TransitionMapBoardVM)new TransitionMapLocalVM(entrance, Close)));
		LocalVM.AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Close()
	{
		m_Close?.Invoke();
	}

	public void HandleAcceptChangeGroup()
	{
		Close();
	}
}
