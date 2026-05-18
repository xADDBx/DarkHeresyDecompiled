using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class IngameMenuConsoleView : View<IngameMenuVM>
{
	[Header("Items")]
	[SerializeField]
	private IngameMenuItemConsoleView m_Inventory;

	[SerializeField]
	private IngameMenuItemConsoleView m_Character;

	[SerializeField]
	private IngameMenuItemConsoleView m_Journal;

	[SerializeField]
	private IngameMenuItemConsoleView m_Map;

	[SerializeField]
	private IngameMenuItemConsoleView m_Encyclopedia;

	[SerializeField]
	private IngameMenuItemConsoleView m_LevelUp;

	[Space]
	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_FirstSelectionButton;

	private readonly ReactiveProperty<bool> m_CanCancel = new ReactiveProperty<bool>();

	public void Initialize()
	{
		InitializeItems();
		m_FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		BindItems();
		m_FirstSelectionButton.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation();
		ModalWindowsSounds.Instance.MessageBox.Show.Play();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: true, ModalWindowUIType.InGameMenu);
		});
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
		ModalWindowsSounds.Instance.MessageBox.Hide.Play();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: false, ModalWindowUIType.InGameMenu);
		});
	}

	private void InitializeItems()
	{
		UIMeinMenuTexts mainMenu = UIStrings.Instance.MainMenu;
		m_Inventory.Initialize(mainMenu.Inventory);
		m_Character.Initialize(mainMenu.CharacterInfo);
		m_Journal.Initialize(mainMenu.Journal);
		m_Map.Initialize(mainMenu.LocalMap);
		m_Encyclopedia.Initialize(mainMenu.Encyclopedia);
		m_LevelUp.Initialize(mainMenu.LevelUp);
	}

	private void BindItems()
	{
		m_Inventory.Bind(base.ViewModel.OpenInventory);
		m_Character.Bind(base.ViewModel.OpenCharScreen);
		m_Journal.Bind(base.ViewModel.OpenJournal);
		m_Map.Bind(base.ViewModel.OpenMap);
		m_Map.gameObject.SetActive(value: true);
		m_Encyclopedia.Bind(base.ViewModel.OpenEncyclopedia);
		if (base.ViewModel.HasLevelUp())
		{
			m_LevelUp.Bind(base.ViewModel.OpenLevelUpOnFirstDecentUnit);
		}
		m_LevelUp.gameObject.SetActive(base.ViewModel.HasLevelUp());
	}
}
