using Code.UI.Pointer;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class IngameMenuPCView : IngameMenuBasePCView<IngameMenuVM>
{
	[Header("Buttons Part")]
	[SerializeField]
	private OwlcatMultiButton m_Inventory;

	[SerializeField]
	private OwlcatMultiButton m_Character;

	[SerializeField]
	private OwlcatMultiButton m_Journal;

	[SerializeField]
	private OwlcatMultiButton m_Map;

	[SerializeField]
	private OwlcatMultiButton m_Encyclopedia;

	[SerializeField]
	private OwlcatMultiButton m_Formation;

	[SerializeField]
	private OwlcatMultiButton m_DetectiveJournal;

	[Header("Highlighter")]
	[SerializeField]
	private UIHighlighter m_UIHighlighter;

	[Header("Notifications")]
	[SerializeField]
	private DetectiveIngameMenuNotificatorView m_DetectiveNotifications;

	[SerializeField]
	private QuestIngameMenuNotificatorView m_QuestNotifications;

	private bool m_IsInit;

	public override void Awake()
	{
		base.Awake();
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		EventBus.Subscribe(this).AddTo(this);
		SetPlastickButtonsSoundsTypes();
		ObservableSubscribeExtensions.Subscribe(m_Map.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenMap();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Journal.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenJournal();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Character.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenCharScreen();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Inventory.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenInventory();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Formation.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenFormation();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DetectiveJournal.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenDetectiveJournal();
		}).AddTo(this);
		m_Map.SetHint(UIStrings.Instance.MainMenu.LocalMap, "OpenMap").AddTo(this);
		m_Journal.SetHint(UIStrings.Instance.MainMenu.Journal, "OpenJournal").AddTo(this);
		m_Encyclopedia.SetHint(UIStrings.Instance.MainMenu.Encyclopedia).AddTo(this);
		m_Character.SetHint(UIStrings.Instance.MainMenu.CharacterInfo, "OpenCharacterScreen").AddTo(this);
		m_Inventory.SetHint(UIStrings.Instance.MainMenu.Inventory, "OpenInventory").AddTo(this);
		m_Formation.SetHint(UIStrings.Instance.EscapeMenu.EscMenuFormation, "OpenFormation").AddTo(this);
		m_DetectiveJournal.SetHint(UIStrings.Instance.EscapeMenu.EscMenuDetectiveJournal, "OpenDetectiveJournal").AddTo(this);
		SubscribeButtonLayer(base.ViewModel.IsInventoryActive, m_Inventory);
		SubscribeButtonLayer(base.ViewModel.IsCharScreenActive, m_Character);
		SubscribeButtonLayer(base.ViewModel.IsJournalActive, m_Journal);
		SubscribeButtonLayer(base.ViewModel.IsLocalMapActive, m_Map);
		SubscribeButtonLayer(base.ViewModel.IsFormationActive, m_Formation);
		SubscribeButtonLayer(base.ViewModel.IsDetectiveJournalActive, m_DetectiveJournal);
		if (m_UIHighlighter != null)
		{
			m_UIHighlighter.Subscribe().AddTo(this);
		}
		m_DetectiveNotifications.Bind(base.ViewModel.DetectiveNotificationsVM);
		m_QuestNotifications.Bind(base.ViewModel.QuestNotificationsVM);
		void SubscribeButtonLayer(ReadOnlyReactiveProperty<bool> activeValue, OwlcatMultiButton button)
		{
			activeValue?.Subscribe(delegate(bool value)
			{
				PFLog.UI.Log($"{button.name} : {activeValue}");
				button.SetActiveLayer(value ? 1 : 0);
			}).AddTo(this);
		}
	}

	private void SetPlastickButtonsSoundsTypes()
	{
		UISounds.Instance.SetClickAndHoverSound(m_Inventory, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Character, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Journal, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Map, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Formation, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_DetectiveJournal, ButtonSoundsEnum.PlastickSound);
	}

	public void OnUIReset()
	{
		m_Map.SetHint(UIStrings.Instance.MainMenu.LocalMap, "OpenMap").AddTo(this);
		m_Journal.SetHint(UIStrings.Instance.MainMenu.Journal, "OpenJournal").AddTo(this);
		m_Encyclopedia.SetHint(UIStrings.Instance.MainMenu.Encyclopedia).AddTo(this);
		m_Character.SetHint(UIStrings.Instance.MainMenu.CharacterInfo, "OpenCharacterScreen").AddTo(this);
		m_Inventory.SetHint(UIStrings.Instance.MainMenu.Inventory, "OpenInventory").AddTo(this);
		m_DetectiveJournal.SetHint(UIStrings.Instance.EscapeMenu.EscMenuDetectiveJournal, "OpenDetectiveJournal").AddTo(this);
	}
}
