using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.DetectiveJournal;

public class ServiceWindowsContext : ViewModel, IServiceWindowUIHandler, IInventoryUIHandler, ISubscriber, IJournalUIHandler, IDetectiveJournalUIHandler, IEncyclopediaHandler
{
	private readonly ReactiveProperty<ServiceWindowsPanelVM> m_WindowsPanelVM;

	public bool HasPrevWindow => (m_WindowsPanelVM?.Value?.HasPrevWindow).GetValueOrDefault();

	public ServiceWindowsContext(ReactiveProperty<ServiceWindowsPanelVM> windowPanelVM)
	{
		m_WindowsPanelVM = windowPanelVM;
		BindKeys();
		EventBus.Subscribe(this).AddTo(this);
		GameUIState.Instance.GameMode.Where((GameModeType modeValue) => modeValue != GameModeType.Default && GameUIState.Instance.GameMode.Value != GameModeType.Pause).Subscribe(delegate
		{
			HandleCloseAll();
		}).AddTo(this);
	}

	public void HandleCloseAll()
	{
		CloseWindowsPanel();
	}

	public void HandleOpenLocalMap()
	{
		OpenAt(ServiceWindowsType.LocalMap);
	}

	public void HandleOpenInventory()
	{
		OpenAt(ServiceWindowsType.Inventory);
	}

	public void HandleOpenCharacterInfo()
	{
		OpenAt(ServiceWindowsType.CharacterInfo);
	}

	public void HandleOpenCharacterInfo(CharInfoPageType pageType, BaseUnitEntity unitEntity)
	{
		if (m_WindowsPanelVM.Value == null)
		{
			ReactiveProperty<ServiceWindowsPanelVM> windowsPanelVM = m_WindowsPanelVM;
			if (windowsPanelVM.Value == null)
			{
				ServiceWindowsPanelVM serviceWindowsPanelVM2 = (windowsPanelVM.Value = new ServiceWindowsPanelVM(ServiceWindowsType.CharacterInfo, CloseWindowsPanel, pageType, unitEntity));
			}
		}
		else
		{
			m_WindowsPanelVM.Value.HandleCharInfo(pageType, unitEntity);
		}
	}

	public void HandleOpenJournal(Quest questToOpen)
	{
		if (questToOpen != null)
		{
			JournalHelper.ChangeCurrentQuest(questToOpen);
		}
		OpenAt(ServiceWindowsType.Journal);
	}

	public void HandleOpenDetectiveJournal(BlueprintCase caseToOpen, BlueprintClue focusClue = null, bool requireReport = false)
	{
		OpenAt(ServiceWindowsType.DetectiveJournal);
		if (caseToOpen != null || focusClue != null)
		{
			m_WindowsPanelVM.Value.HandleDetectiveJournal(caseToOpen, focusClue, requireReport);
		}
	}

	public void HandleUnknownClues()
	{
		OpenAt(ServiceWindowsType.DetectiveJournal);
		m_WindowsPanelVM.Value.HandleDetectiveJournal(null);
	}

	public void HandleEncyclopediaPage(string pageKey)
	{
	}

	public void HandleEncyclopediaPage(INode page)
	{
	}

	private void BindKeys()
	{
		Game.Instance.Keyboard.Bind("OpenMap", HandleToggleLocalMap).AddTo(this);
		Game.Instance.Keyboard.Bind("OpenInventory", HandleToggleInventory).AddTo(this);
		Game.Instance.Keyboard.Bind("OpenCharacterScreen", HandleToggleCharacterInfo).AddTo(this);
		Game.Instance.Keyboard.Bind("OpenJournal", HandleToggleJournal).AddTo(this);
		Game.Instance.Keyboard.Bind("OpenDetectiveJournal", HandleToggleDetectiveJournal).AddTo(this);
	}

	private void HandleToggleLocalMap()
	{
		if (!CloseIfTypeIs(FullScreenUIType.LocalMap))
		{
			HandleOpenLocalMap();
		}
	}

	private void HandleToggleInventory()
	{
		if (!CloseIfTypeIs(FullScreenUIType.Inventory))
		{
			HandleOpenInventory();
		}
	}

	private void HandleToggleCharacterInfo()
	{
		if (!CloseIfTypeIs(FullScreenUIType.CharacterScreen))
		{
			HandleOpenCharacterInfo();
		}
	}

	private void HandleToggleJournal()
	{
		if (!CloseIfTypeIs(FullScreenUIType.Journal))
		{
			HandleOpenJournal(null);
		}
	}

	private void HandleToggleDetectiveJournal()
	{
		if (!CloseIfTypeIs(FullScreenUIType.DetectiveJournal))
		{
			HandleOpenDetectiveJournal(null);
		}
	}

	private void HandleToggleEncyclopedia()
	{
		if (!CloseIfTypeIs(FullScreenUIType.Encyclopedia))
		{
			HandleEncyclopediaPage((INode)null);
		}
	}

	private bool CloseIfTypeIs(FullScreenUIType type)
	{
		ServiceWindowsPanelVM value = m_WindowsPanelVM.Value;
		if (value == null || value.CurrentUIType.CurrentValue != type)
		{
			return false;
		}
		m_WindowsPanelVM.Value?.Close();
		return true;
	}

	private void OpenAt(ServiceWindowsType type)
	{
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleInfoRequest(null);
		});
		ReactiveProperty<ServiceWindowsPanelVM> windowsPanelVM = m_WindowsPanelVM;
		if (windowsPanelVM.Value == null)
		{
			ServiceWindowsPanelVM serviceWindowsPanelVM2 = (windowsPanelVM.Value = new ServiceWindowsPanelVM(type, CloseWindowsPanel));
		}
		m_WindowsPanelVM.Value.HandleOpenAt(type);
	}

	private void CloseWindowsPanel()
	{
		ServiceWindowsPanelVM value = m_WindowsPanelVM.Value;
		m_WindowsPanelVM.Value = null;
		value?.Dispose();
	}
}
