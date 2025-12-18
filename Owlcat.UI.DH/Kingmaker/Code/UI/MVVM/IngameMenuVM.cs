using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class IngameMenuVM : IngameMenuBaseVM, ICanAccessStarshipInventoryHandler, ISubscriber, ICanAccessColonizationHandler
{
	public readonly DetectiveIngameMenuNotificatorVM DetectiveNotificationsVM;

	public readonly QuestIngameMenuNotificatorVM QuestNotificationsVM;

	private List<UnitReference> m_PartyCharacters;

	private readonly ReactiveCommand<Unit> m_CheckCanAccessStarshipInventoryButtons = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_CheckCanAccessColonizationButton = new ReactiveCommand<Unit>();

	public Observable<Unit> CheckCanAccessStarshipInventoryButtons => m_CheckCanAccessStarshipInventoryButtons;

	public Observable<Unit> CheckCanAccessColonizationButton => m_CheckCanAccessColonizationButton;

	public IngameMenuVM()
	{
		UpdatePartyCharacters();
		DetectiveNotificationsVM = new DetectiveIngameMenuNotificatorVM();
		QuestNotificationsVM = new QuestIngameMenuNotificatorVM();
	}

	public void OpenInventory()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenInventory();
		});
	}

	public void OpenCharScreen()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfo();
		});
	}

	public void OpenJournal()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenJournal();
		});
	}

	public void OpenEncyclopedia()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleEncyclopediaPage((INode)null);
		});
	}

	public void OpenMap()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenLocalMap();
		});
	}

	public void OpenFormation()
	{
		if (!base.IsFormationActive.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
			{
				h.HandleOpenFormation();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
			{
				h.HandleCloseFormation();
			});
		}
	}

	public void OpenDetectiveJournal()
	{
		EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
		{
			h.HandleOpenDetectiveJournal(null);
		});
	}

	public void OpenEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void OpenLevelUpOnFirstDecentUnit()
	{
		if (Game.Instance.Controllers.SelectionCharacter.FirstSelectedUnit == null || !Game.Instance.Controllers.SelectionCharacter.FirstSelectedUnit.Progression.CanLevelUp)
		{
			UnitReference unitReference = m_PartyCharacters.FirstOrDefault((UnitReference c) => c.ToBaseUnitEntity().Progression.CanLevelUp);
			if (unitReference != null)
			{
				Game.Instance.Controllers.SelectionCharacter.SetSelected(unitReference.ToBaseUnitEntity());
			}
		}
		OpenLevelUp();
	}

	public void OpenLevelUp()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfo(CharInfoPageType.LevelProgression, null);
		});
	}

	public void OnOpenGroupChanger()
	{
		StartChangedPartyOnGlobalMap();
	}

	private void StartChangedPartyOnGlobalMap()
	{
		UpdatePartyCharacters();
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(ChangePartyOnMap, null, isCapital: true);
		});
	}

	private void ChangePartyOnMap()
	{
	}

	private void UpdatePartyCharacters()
	{
		m_PartyCharacters = Game.Instance.Player.Party.Select((BaseUnitEntity u) => UnitReference.FromIAbstractUnitEntity(u)).ToList();
	}

	public bool HasLevelUp()
	{
		UpdatePartyCharacters();
		return m_PartyCharacters.Any((UnitReference c) => c.ToBaseUnitEntity().Progression.CanLevelUp);
	}

	public void HandleCanAccessStarshipInventory()
	{
		m_CheckCanAccessStarshipInventoryButtons.Execute();
	}

	public void HandleCanAccessColonization()
	{
		m_CheckCanAccessColonizationButton.Execute();
	}
}
