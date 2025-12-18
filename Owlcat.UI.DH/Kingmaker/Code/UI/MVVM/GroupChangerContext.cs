using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GroupChangerContext : ViewModel, IGroupChangerHandler, ISubscriber, IDetachUnitsUIHandler
{
	private readonly ReactiveProperty<GroupChangerVM> m_GroupChangerVm;

	private readonly List<UnitReference> m_LastUnits = new List<UnitReference>();

	private readonly List<BlueprintUnit> m_RequiredUnits = new List<BlueprintUnit>();

	public ReadOnlyReactiveProperty<GroupChangerVM> GroupChangerVm => m_GroupChangerVm;

	public GroupChangerContext(ReactiveProperty<GroupChangerVM> groupChangerVM)
	{
		m_GroupChangerVm = groupChangerVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeGroupChanger();
	}

	public void HandleCall(Action goAction, Action closeAction, bool isCapital, bool sameFinishActions = false, bool canCancel = true, bool allowRemoteCompanionsEverywhere = false)
	{
		goAction = (Action)Delegate.Combine(goAction, new Action(DisposeGroupChanger));
		closeAction = (Action)Delegate.Combine(closeAction, new Action(DisposeGroupChanger));
		if (GroupChangerVm.CurrentValue != null)
		{
			GroupChangerVm.CurrentValue.AddActionGo(goAction);
			GroupChangerVm.CurrentValue.AddActionClose(closeAction);
		}
		else
		{
			m_GroupChangerVm.Value = new GroupChangerCommonVM(goAction, closeAction, m_LastUnits, m_RequiredUnits, isCapital, sameFinishActions, canCancel, allowRemoteCompanionsEverywhere).AddTo(this);
		}
		m_LastUnits.Clear();
		m_RequiredUnits.Clear();
	}

	public void HandleSetLastUnits(List<UnitReference> lastUnits)
	{
		m_LastUnits.AddRange(lastUnits);
		RootUIContext.Instance.SelectionManager.SelectAll();
	}

	public void HandleSetRequiredUnits(List<BlueprintUnit> requiredUnits)
	{
		m_RequiredUnits.AddRange(requiredUnits);
		RootUIContext.Instance.SelectionManager.SelectAll();
	}

	public void HandleDetachUnits(int maxUnitInParty, ActionList afterDetach)
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			m_LastUnits.Clear();
			m_RequiredUnits.Clear();
			m_GroupChangerVm.Value = new GroupChangerDetachVM(Go, null, m_LastUnits, m_RequiredUnits).AddTo(this);
		}
		void Go()
		{
			afterDetach?.Run();
			DisposeGroupChanger();
			RootUIContext.Instance.SelectionManager.SelectAll();
		}
	}

	private void DisposeGroupChanger()
	{
		GroupChangerVm.CurrentValue?.Dispose();
		m_GroupChangerVm.Value = null;
		RootUIContext.Instance.SelectionManager.SelectAll();
	}
}
