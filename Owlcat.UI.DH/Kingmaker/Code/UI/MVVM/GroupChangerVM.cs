using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class GroupChangerVM : ViewModel, IAcceptChangeGroupHandler, ISubscriber, IChangeGroupHandler, ICloseChangeGroupHandler
{
	private Action m_ActionGo;

	private Action m_ActionClose;

	private List<UnitReference> m_OverridePartyCharacters;

	private List<UnitReference> m_OverrideRemoteCharacters;

	private readonly ReactiveProperty<bool> m_CloseActionsIsSame = new ReactiveProperty<bool>();

	protected readonly bool IsCapital;

	protected readonly bool AllowRemoteCompanionsEverywhere;

	protected readonly List<UnitReference> LastUnits = new List<UnitReference>();

	protected readonly List<BlueprintUnit> RequiredUnits = new List<BlueprintUnit>();

	protected readonly ObservableList<GroupChangerCharacterVM> m_PartyCharacter = new ObservableList<GroupChangerCharacterVM>();

	protected readonly ObservableList<GroupChangerCharacterVM> m_RemoteCharacter = new ObservableList<GroupChangerCharacterVM>();

	protected readonly ReactiveProperty<bool> m_CloseEnabled = new ReactiveProperty<bool>(value: true);

	private IEnumerable<UnitReference> PartyNavigatorsCharacterRef => PartyCharacterRef.Where((UnitReference p) => p.Entity.ToBaseUnitEntity().IsNavigatorCompanion());

	public ObservableList<GroupChangerCharacterVM> PartyCharacter => m_PartyCharacter;

	public ObservableList<GroupChangerCharacterVM> RemoteCharacter => m_RemoteCharacter;

	public IEnumerable<UnitReference> PartyCharacterRef => m_PartyCharacter.Select((GroupChangerCharacterVM p) => p.UnitRef);

	public IEnumerable<UnitReference> RemoteCharacterRef => m_RemoteCharacter.Select((GroupChangerCharacterVM p) => p.UnitRef);

	public IEnumerable<BlueprintUnitReference> RequiredCharactersRef => RequiredUnits.Select((BlueprintUnit p) => p.ToReference<BlueprintUnitReference>());

	public ReadOnlyReactiveProperty<bool> CloseEnabled => m_CloseEnabled;

	public ReadOnlyReactiveProperty<bool> CloseActionsIsSame => m_CloseActionsIsSame;

	protected GroupChangerVM(Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital = false, bool sameFinishActions = false, bool closeEnabled = true, bool allowRemoteCompanionsEverywhere = false)
	{
		m_ActionGo = go;
		m_ActionClose = close;
		m_CloseActionsIsSame.Value = sameFinishActions;
		LastUnits.AddRange(lastUnits);
		RequiredUnits.AddRange(requiredUnits);
		IsCapital = isCapital;
		AllowRemoteCompanionsEverywhere = allowRemoteCompanionsEverywhere;
		m_CloseEnabled.Value = closeEnabled;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		LastUnits.Clear();
		RequiredUnits.Clear();
		m_PartyCharacter.ForEach(delegate(GroupChangerCharacterVM c)
		{
			c.Dispose();
		});
		m_PartyCharacter.Clear();
		m_RemoteCharacter.ForEach(delegate(GroupChangerCharacterVM c)
		{
			c.Dispose();
		});
		m_RemoteCharacter.Clear();
	}

	public void AddActionGo(Action go)
	{
		m_ActionGo = (Action)Delegate.Combine(m_ActionGo, go);
	}

	public void AddActionClose(Action go)
	{
		m_ActionClose = (Action)Delegate.Combine(m_ActionClose, go);
	}

	void IAcceptChangeGroupHandler.HandleAcceptChangeGroup()
	{
		try
		{
			m_ActionGo?.Invoke();
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex);
		}
	}

	void ICloseChangeGroupHandler.HandleCloseChangeGroup()
	{
		Close();
	}

	public void Close()
	{
		if (!CloseCondition())
		{
			return;
		}
		try
		{
			m_ActionClose?.Invoke();
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex);
		}
	}

	public virtual bool CloseCondition()
	{
		return false;
	}

	public void OnSelectedClick()
	{
		foreach (GroupChangerCharacterVM item in RemoteCharacter)
		{
			if (item.IsFocused)
			{
				item.OnClick();
				return;
			}
		}
		foreach (GroupChangerCharacterVM item2 in PartyCharacter)
		{
			if (item2.IsFocused)
			{
				item2.OnClick();
				break;
			}
		}
	}

	protected void MoveCharacter(UnitReference unitReference)
	{
		if (UtilityNet.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.GroupChanger(unitReference);
		}
	}

	void IChangeGroupHandler.HandleChangeGroup(string unitUniqueId)
	{
		UnitReference unitReference = new UnitReference(unitUniqueId);
		GroupChangerCharacterVM groupChangerCharacterVM = m_PartyCharacter.FirstOrDefault((GroupChangerCharacterVM vm) => vm.UnitRef == unitReference);
		if (groupChangerCharacterVM != null)
		{
			if (!groupChangerCharacterVM.IsLock.CurrentValue)
			{
				MoveCharacterFromPartyToRemote(groupChangerCharacterVM);
			}
			return;
		}
		string cantMove = CanMoveCharacterFromRemoteToParty(unitReference);
		if (!string.IsNullOrEmpty(cantMove))
		{
			if (UtilityNet.IsControlMainCharacter())
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(cantMove);
				});
			}
			return;
		}
		groupChangerCharacterVM = m_RemoteCharacter.FirstOrDefault((GroupChangerCharacterVM vm) => vm.UnitRef == unitReference);
		if (groupChangerCharacterVM != null)
		{
			MoveCharacterFromRemoteToParty(groupChangerCharacterVM);
		}
	}

	protected virtual string CanMoveCharacterFromRemoteToParty(UnitReference unitReference)
	{
		if (m_PartyCharacter.Count == 6)
		{
			return UIStrings.Instance.GroupChangerTexts.MaxGroupCountWarning;
		}
		if (unitReference.Entity.ToBaseUnitEntity().IsNavigatorCompanion() && PartyNavigatorsCharacterRef.Count() >= 1)
		{
			return UIStrings.Instance.GroupChangerTexts.MaxNavigatorsCountWarning;
		}
		return null;
	}

	protected virtual void MoveCharacterFromPartyToRemote(GroupChangerCharacterVM characterVm)
	{
		m_PartyCharacter.Remove(characterVm);
		m_RemoteCharacter.Add(characterVm);
		characterVm.SetIsInParty(isInParty: false);
	}

	protected virtual void MoveCharacterFromRemoteToParty(GroupChangerCharacterVM characterVm)
	{
		m_RemoteCharacter.Remove(characterVm);
		m_PartyCharacter.Add(characterVm);
		characterVm.SetIsInParty(isInParty: true);
	}
}
