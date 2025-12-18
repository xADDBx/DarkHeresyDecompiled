using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameModes;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PartyPCView : PartyBaseView<PartyCharacterPCView>
{
	[SerializeField]
	private bool m_ShowAlways;

	[Header("Show hide position")]
	[SerializeField]
	private float m_HidePosY = -295f;

	[SerializeField]
	private float m_ShowPosY;

	[Header("SelectAll")]
	[SerializeField]
	private OwlcatMultiButton m_SelectAllButton;

	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	private bool m_IsVisible;

	private bool m_IsBinded;

	private readonly ReactiveProperty<bool> m_CanBeVisible = new ReactiveProperty<bool>();

	private CanvasGroup CanvasGroup => m_CanvasGroup ?? (m_CanvasGroup = GetComponent<CanvasGroup>());

	private RectTransform RectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

	protected override void Awake()
	{
		base.Awake();
		m_Characters.ForEach(delegate(PartyCharacterPCView elem)
		{
			elem.SetSwitchAction(DragCharacter);
		});
	}

	protected override void OnBind()
	{
		base.OnBind();
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevCharacter.name, base.ViewModel.SelectPrevCharacter).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextCharacter.name, base.ViewModel.SelectNextCharacter).AddTo(this);
		Game.Instance.Keyboard.Bind("SelectAllCharacters", SelectAll).AddTo(this);
		m_CanBeVisible.Subscribe(DoVisibility).AddTo(this);
		GameUIState.Instance.GameMode.CombineLatest(GameUIState.Instance.CurrentFullScreenUIType, GameUIState.Instance.IsInCombat, (GameModeType _, FullScreenUIType _, bool _) => new { }).Subscribe(_ =>
		{
			UpdateVisibility();
		}).AddTo(this);
		if (m_SelectAllButton != null)
		{
			base.ViewModel.AllSelected.Subscribe(delegate(bool value)
			{
				m_SelectAllButton.SetActiveLayer(value ? "Disabled" : "Normal");
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_SelectAllButton.OnLeftClickAsObservable(), delegate
			{
				SelectAll();
			}).AddTo(this);
			string text = UIStrings.Instance.HUDTexts.SelectAllCharacters.Text;
			string prettyString = UISettingsRoot.Instance.UIKeybindSelectCharacterSettings.SelectAll.GetBinding(1).GetPrettyString();
			m_SelectAllButton.SetHint(text, prettyString).AddTo(this);
		}
		m_IsBinded = true;
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_IsBinded = false;
	}

	private void SelectAll()
	{
		RootUIContext.Instance.SelectionManager.SelectAll();
	}

	private void UpdateVisibility()
	{
		bool flag = GameUIState.Instance.CurrentFullScreenUIType.Value switch
		{
			FullScreenUIType.Encyclopedia => false, 
			FullScreenUIType.Journal => false, 
			FullScreenUIType.Reputation => false, 
			FullScreenUIType.Vendor => true, 
			FullScreenUIType.LocalMap => false, 
			FullScreenUIType.Inventory => false, 
			FullScreenUIType.EscapeMenu => false, 
			FullScreenUIType.DetectiveJournal => false, 
			FullScreenUIType.OneSlotLoot => false, 
			FullScreenUIType.PlayerChest => false, 
			_ => true, 
		};
		bool flag2 = GameUIState.Instance.GameMode.Value != GameModeType.Dialog && GameUIState.Instance.GameMode.Value != GameModeType.Cutscene && !GameUIState.Instance.IsInCombat.Value;
		m_CanBeVisible.Value = (flag && flag2) || m_ShowAlways;
	}

	public void DoVisibility(bool visible)
	{
		if (visible != m_IsVisible || !m_IsBinded)
		{
			m_IsVisible = visible;
			CanvasGroup.DOKill();
			RectTransform.DOKill();
			CanvasGroup.DOFade(m_IsVisible ? 1f : 0f, 0.2f).SetDelay(m_IsVisible ? 0.0001f : 0.2f).SetUpdate(isIndependentUpdate: true);
			RectTransform.DOAnchorPosY(m_IsVisible ? m_ShowPosY : m_HidePosY, 0.2f).SetDelay(m_IsVisible ? 0.2f : 0.0001f).SetEase(m_IsVisible ? Ease.OutCubic : Ease.InCubic)
				.SetUpdate(isIndependentUpdate: true);
		}
	}

	protected void DragCharacter(PartyCharacterBaseView mainCharacter)
	{
		foreach (PartyCharacterPCView character in m_Characters)
		{
			if (!(mainCharacter == character) && character.HasUnit)
			{
				float num = character.RectTransform.sizeDelta.x / 2f;
				if (mainCharacter.RectTransform.localPosition.x > character.BasePositionX - num && mainCharacter.RectTransform.localPosition.x < character.BasePositionX + num)
				{
					base.ViewModel.SwitchCharacter(mainCharacter.UnitEntityData, character.UnitEntityData);
					PartyCharacterPCView partyCharacterPCView = character;
					float basePositionX = character.BasePositionX;
					float basePositionX2 = mainCharacter.BasePositionX;
					mainCharacter.BasePositionX = basePositionX;
					partyCharacterPCView.BasePositionX = basePositionX2;
					character.RectTransform.DOLocalMoveX(character.BasePositionX, 0.2f).SetUpdate(isIndependentUpdate: true);
					break;
				}
			}
		}
	}
}
