using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
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
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private OwlcatMultiButton m_FirstSelectionButton;

	private FloatConsoleNavigationBehaviour m_NewNavigationBehaviour;

	private SimpleConsoleNavigationEntity m_FirstSelection;

	private readonly ReactiveProperty<bool> m_CanCancel = new ReactiveProperty<bool>();

	private InputLayer m_InputLayer;

	public void Initialize()
	{
		InitializeItems();
		m_FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		BindItems();
		m_FirstSelectionButton.gameObject.SetActive(value: true);
		CreateNavigation();
		GamePad.Instance.PushLayer(GetInputLayer()).AddTo(this);
		m_FadeAnimator.AppearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: true, ModalWindowUIType.InGameMenu);
		});
		GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		IngameMenuItemConsoleView ingameMenuItemConsoleView = m_NewNavigationBehaviour.CurrentEntity as IngameMenuItemConsoleView;
		if (ingameMenuItemConsoleView != null)
		{
			ingameMenuItemConsoleView.OnConfirmClick();
		}
		m_NewNavigationBehaviour.Clear();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: false, ModalWindowUIType.InGameMenu);
		});
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = m_NewNavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "IngameMenuConsoleView"
		}, null, leftStick: true, rightStick: true);
		m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			m_NewNavigationBehaviour.ResetCurrentEntity();
			base.ViewModel.Dispose();
		}, 9, m_CanCancel), UIStrings.Instance.CommonTexts.Cancel).AddTo(this);
		inputLayer.AddButton(delegate
		{
			base.ViewModel.Dispose();
		}, 8, m_CanCancel).AddTo(this);
		m_InputLayer = inputLayer;
		return inputLayer;
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

	private void CreateNavigation()
	{
		if (m_NewNavigationBehaviour == null)
		{
			m_NewNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters).AddTo(this);
		}
		else
		{
			m_NewNavigationBehaviour.Clear();
		}
		List<IngameMenuItemConsoleView> entities = m_Content.GetComponentsInChildren<IngameMenuItemConsoleView>().ToList();
		m_FirstSelection = new SimpleConsoleNavigationEntity(m_FirstSelectionButton);
		m_NewNavigationBehaviour.AddEntities(entities);
		m_NewNavigationBehaviour.AddEntity(m_FirstSelection);
		m_NewNavigationBehaviour.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusEntity);
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NewNavigationBehaviour.FocusOnEntityManual(m_FirstSelection);
		}, 1);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CanCancel.Value = entity != m_FirstSelection && entity != null;
		if (entity != m_FirstSelection && m_NewNavigationBehaviour.Entities.Contains(m_FirstSelection))
		{
			m_NewNavigationBehaviour.RemoveEntity(m_FirstSelection);
			m_FirstSelectionButton.gameObject.SetActive(value: false);
		}
	}

	private void OnCurrentInputLayerChanged()
	{
		InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
		if (currentInputLayer != m_InputLayer && !(currentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(currentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName))
		{
			if (currentInputLayer.ContextName == TutorialBigWindowConsoleView.InputLayerContextName || currentInputLayer.ContextName == TutorialBigWindowConsoleView.GlossaryContextName)
			{
				base.ViewModel.Dispose();
				return;
			}
			GamePad.Instance.PopLayer(m_InputLayer);
			GamePad.Instance.PushLayer(m_InputLayer);
		}
	}
}
