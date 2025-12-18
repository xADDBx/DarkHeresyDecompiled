using System;
using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionVM : ViewModel, IAreaTransitionHandler, ISubscriber, IScreenUIHandler
{
	public readonly string Name;

	public readonly BlueprintMultiEntrance.BlueprintMultiEntranceMap Map;

	public readonly List<TransitionEntryVM> EntryVms = new List<TransitionEntryVM>();

	private readonly Action m_Close;

	public TransitionVM(BlueprintMultiEntrance entrance, Action close)
	{
		m_Close = close;
		Map = entrance.Map;
		Name = entrance.Name;
		entrance.Entries.ForEach(delegate(BlueprintMultiEntranceEntry e)
		{
			EntryVms.Add(new TransitionEntryVM(e, Close).AddTo(this));
		});
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		EntryVms.ForEach(delegate(TransitionEntryVM vm)
		{
			vm.Dispose();
		});
		EntryVms.Clear();
	}

	public void Close()
	{
		bool flag = Game.Instance.LoadedAreaState != null && Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
		bool flag2 = Game.Instance.CurrentlyLoadedArea.IsGlobalmapArea || flag;
		if (!flag2 || UtilityNet.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.CloseScreen(IScreenUIHandler.ScreenType.Transition, flag2);
		}
	}

	void IScreenUIHandler.CloseScreen(IScreenUIHandler.ScreenType screen)
	{
		m_Close?.Invoke();
	}

	void IAreaTransitionHandler.HandleAreaTransition()
	{
		m_Close?.Invoke();
	}
}
