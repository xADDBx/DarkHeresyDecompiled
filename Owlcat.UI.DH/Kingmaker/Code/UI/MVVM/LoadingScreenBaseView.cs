using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenBaseView : View<LoadingScreenVM>
{
	[Serializable]
	public class SettingTypeScreens
	{
		public BlueprintArea.SettingType Type;

		public List<LoadingScreenImage> Sprites;
	}

	private static readonly int ThresholdIndex = Shader.PropertyToID("_Threshold");

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenHints m_Hints;

	[SerializeField]
	[UsedImplicitly]
	private List<Image> m_Points;

	[Header("Content")]
	[SerializeField]
	[UsedImplicitly]
	private GameObject m_MapContainer;

	[SerializeField]
	[UsedImplicitly]
	private Image BigArt;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenGlitchAnimator m_GlitchAnimator;

	[Header("OverlayFX")]
	[SerializeField]
	private Image m_OverlayFX;

	[SerializeField]
	[Range(0f, 100f)]
	private float m_OverlayFXActivationThreshold = 100f;

	private static readonly int OverlayFXPowerIndex = Shader.PropertyToID("_Power");

	private bool m_OverlayFXActivated;

	[Header("LoadingSprites")]
	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenImage m_KeyArtTuple;

	[SerializeField]
	[UsedImplicitly]
	private List<SettingTypeScreens> m_SettingTypeScreensList;

	[Header("Character")]
	[SerializeField]
	[UsedImplicitly]
	private Image m_CharacterPortrait;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_CharacterNameText;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_CharacterDescriptionText;

	[SerializeField]
	[UsedImplicitly]
	private Image m_CharacterWeapon;

	[Header("Variables")]
	[SerializeField]
	[UsedImplicitly]
	private float m_LoadingDissolveTime = 2f;

	[SerializeField]
	[UsedImplicitly]
	private float m_HidingDissolveTime = 0.5f;

	[SerializeField]
	[UsedImplicitly]
	private float m_MinDissolveStep = 0.02f;

	[SerializeField]
	[UsedImplicitly]
	private float m_MaxDissolveStep = 0.1f;

	[SerializeField]
	[UsedImplicitly]
	private float m_HidingMaxDissolveStep = 0.3f;

	[Header("Progress")]
	[SerializeField]
	private CanvasGroup m_ProgressBarContainer;

	[SerializeField]
	[UsedImplicitly]
	private Image ProgressImage;

	[SerializeField]
	private Scrollbar m_ProgressScrollbar;

	[SerializeField]
	private CanvasGroup m_ProgressPercentContainer;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI ProgressPercent;

	[Header("SaveTransfer")]
	[SerializeField]
	private CanvasGroup m_TransferSaveProgressBarContainer;

	[SerializeField]
	private Image m_TransferSaveProgress;

	[SerializeField]
	private CanvasGroup m_TransferSaveCountersContainer;

	[SerializeField]
	private TextMeshProUGUI m_TransferSaveProgressPercent;

	[SerializeField]
	private TextMeshProUGUI m_TransferSaveProgressSize;

	[Header("WaitForInput")]
	[SerializeField]
	private CanvasGroup m_WaitForUserInputContainer;

	[SerializeField]
	private TextMeshProUGUI m_WaitForUserInputText;

	[Header("Main")]
	[SerializeField]
	private GameObject m_CharacterContainer;

	[Header("Main")]
	[SerializeField]
	private GameObject m_MainContainer;

	[SerializeField]
	private GameObject m_BottomTitleObject;

	[SerializeField]
	private GameObject m_BottomDescriptionObject;

	[SerializeField]
	protected TextMeshProUGUI m_BottomTitleText;

	[SerializeField]
	protected TextMeshProUGUI m_BottomDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_LocationName;

	[Header("Random")]
	[SerializeField]
	private int[] m_RandomTwoEqualPercents = new int[3] { 0, 50, 50 };

	[SerializeField]
	private int[] RandomTwoWithPriorityPercents = new int[3] { 0, 60, 40 };

	[SerializeField]
	private int[] RandomThreeWithPriorityPercents = new int[4] { 0, 40, 30, 30 };

	[SerializeField]
	private int[] RandomFourWithPriorityPercents = new int[5] { 0, 40, 20, 20, 20 };

	[SerializeField]
	private int[] RandomFiveWithPriorityPercents = new int[6] { 0, 40, 15, 15, 15, 15 };

	private bool m_IsInit;

	private bool m_ShowDissolve;

	private float m_CurrentThreshold;

	private float m_CurrentTime;

	private Sequence m_PointSequence;

	private float m_Progress;

	private float m_VirtualProgress;

	private Coroutine m_ProgressCoroutine;

	private int m_LoadingScreenType;

	private readonly LoadingScreenHints.LocationEnum m_LocationHints;

	private readonly LoadingScreenHints.LocationEnum m_BridgeHints = LoadingScreenHints.LocationEnum.BridgeHints;

	private readonly LoadingScreenHints.LocationEnum m_StarSystemHints = LoadingScreenHints.LocationEnum.StarSystemHints;

	private readonly LoadingScreenHints.LocationEnum m_GlobalMapHints = LoadingScreenHints.LocationEnum.GlobalMapHints;

	private readonly LoadingScreenHints.LocationEnum m_SpaceCombatHints = LoadingScreenHints.LocationEnum.SpaceCombatHints;

	private readonly LoadingScreenHints.LocationEnum m_MainMenuHints = LoadingScreenHints.LocationEnum.MainMenuHints;

	private Tween m_WaitForInputLoopAnimation;

	private LoadingScreenImage? m_LoaddedTuple;

	private bool m_IsCharacterScreen;

	private BaseUnitEntity m_CompanionOnLoadingScreen;

	private bool BetaTesting => m_Hints.BetaTesting;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			base.gameObject.SetActive(value: false);
			m_FadeAnimator.Initialize();
			if (m_OverlayFX != null && m_OverlayFX.material != null)
			{
				m_OverlayFX.material = UnityEngine.Object.Instantiate(m_OverlayFX.material);
			}
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.AreaProperty.Subscribe(SetupLoadingArea).AddTo(this);
		UISounds.Instance.Play(SystemSounds.Instance.Selector.LoopStop, isButton: false, playAnyway: true);
		Show();
		UISounds.Instance.Play(SystemSounds.Instance.Selector.LoopStop, isButton: false, playAnyway: true);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateThreshold();
		}).AddTo(this);
		base.ViewModel.NeedUserInput.Subscribe(ShowUserInputWaiting).AddTo(this);
		base.ViewModel.IsSaveTransfer.Subscribe(delegate(bool value)
		{
			m_TransferSaveProgressBarContainer.gameObject.SetActive(value);
			m_TransferSaveCountersContainer.gameObject.SetActive(value);
			m_ProgressBarContainer.gameObject.SetActive(!value);
			m_ProgressPercentContainer.gameObject.SetActive(!value);
			m_WaitForUserInputContainer.gameObject.SetActive(!value && base.ViewModel.NeedUserInput.CurrentValue);
		}).AddTo(this);
		base.ViewModel.SaveTransferProgress.CombineLatest(base.ViewModel.SaveTransferTarget, (int progress, int target) => new { progress, target }).Subscribe(transfer =>
		{
			if (transfer.target == 0 || !base.ViewModel.IsSaveTransfer.CurrentValue)
			{
				m_TransferSaveProgressSize.text = string.Empty;
				m_TransferSaveProgressPercent.text = string.Empty;
				m_TransferSaveProgress.fillAmount = 0f;
			}
			else
			{
				m_TransferSaveProgress.fillAmount = (float)transfer.progress / (float)transfer.target;
				m_TransferSaveProgressSize.text = $"{transfer.progress / 1024}/{transfer.target / 1024} KB";
				m_TransferSaveProgressPercent.text = $"{100f * ((float)transfer.progress / (float)transfer.target):00}%";
			}
		}).AddTo(this);
		base.ViewModel.UserInputProgress.CombineLatest(base.ViewModel.UserInputTarget, base.ViewModel.UserInputMeIsPressed, (int progress, int target, bool me) => new { progress, target, me }).Subscribe(value =>
		{
			if (!value.me)
			{
				m_WaitForUserInputText.text = (Game.Instance.IsControllerMouse ? UIStrings.Instance.CommonTexts.PressAnyKey : UIStrings.Instance.CommonTexts.PressAnyKeyConsole);
			}
			else
			{
				m_WaitForUserInputContainer.gameObject.SetActive(value: true);
				m_WaitForUserInputContainer.alpha = 1f;
				m_WaitForUserInputText.text = string.Format(UIStrings.Instance.CommonTexts.WaitingOtherPlayer, value.progress, value.target);
			}
		}).AddTo(this);
		SetTextFontSize(base.ViewModel.FontMultiplier);
	}

	protected override void OnUnbind()
	{
		if (!Game.Instance.IsSpaceCombat && PhotonManager.Lobby.IsActive && !PhotonManager.NetGame.NetRolesShowed && !GameUIState.Instance.IsInMainMenu)
		{
			EventBus.RaiseEvent(delegate(INetRolesRequest h)
			{
				h.HandleNetRolesRequest();
			});
			PhotonManager.NetGame.NetRolesShowed = true;
		}
		EventBus.RaiseEvent(delegate(INetEvents h)
		{
			h.HandleNLoadingScreenClosed();
		});
		m_GlitchAnimator.StartGlitch(Hide);
		if (m_LoaddedTuple.HasValue)
		{
			m_LoaddedTuple?.Main.ForceUnload();
			m_LoaddedTuple?.Glitch.ForceUnload();
			m_LoaddedTuple = null;
		}
	}

	protected virtual void SetTextFontSize(float multiplier)
	{
	}

	private void Show()
	{
		m_ProgressBarContainer.alpha = 1f;
		m_ProgressPercentContainer.alpha = 1f;
		KillWaitUserInputAnimation();
		m_OverlayFXActivated = false;
		if (m_OverlayFX != null && m_OverlayFX.material != null)
		{
			m_OverlayFX.material.SetFloat(OverlayFXPowerIndex, 0f);
		}
		Game.Instance.ResetLoadingProgress();
		base.ViewModel.State = LoadingScreenState.ShowAnimation;
		m_FadeAnimator.AppearAnimation(delegate
		{
			base.ViewModel.State = LoadingScreenState.Shown;
		});
		m_PointSequence = DOTween.Sequence();
		m_PointSequence.Play().SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Restart);
		m_CurrentThreshold = 1f;
		if (!m_ShowDissolve)
		{
			m_CurrentThreshold = 1f;
		}
		m_ProgressCoroutine = StartCoroutine(LoadingProgressCoroutine());
	}

	private void Hide()
	{
		KillWaitUserInputAnimation();
		if (base.ViewModel != null)
		{
			base.ViewModel.State = LoadingScreenState.HideAnimation;
		}
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
			if (base.ViewModel != null)
			{
				base.ViewModel.State = LoadingScreenState.Hidden;
			}
		});
		m_CurrentTime = 0f;
		m_ShowDissolve = false;
		m_PointSequence.Kill();
		m_PointSequence = null;
		Game.Instance.ResetLoadingProgress();
		if (m_ProgressCoroutine != null)
		{
			StopCoroutine(m_ProgressCoroutine);
		}
		SetProgress(1f);
		m_Progress = 0f;
		m_VirtualProgress = 0f;
		if (m_IsCharacterScreen)
		{
			RootUIContext.Instance.PreviousLoadingScreenCompanion = m_CompanionOnLoadingScreen;
		}
	}

	private void KillWaitUserInputAnimation()
	{
		m_WaitForUserInputContainer.DOKill();
		DOTween.Kill(m_WaitForUserInputContainer);
		m_WaitForInputLoopAnimation?.Kill();
		m_WaitForInputLoopAnimation = null;
		m_WaitForUserInputContainer.alpha = 0f;
		m_WaitForUserInputContainer.gameObject.SetActive(value: false);
	}

	private void SetupLoadingArea(BlueprintArea area)
	{
		SwitchLoadingScreen(1);
		if (area == null || BetaTesting)
		{
			if (m_LocationName != null)
			{
				m_LocationName.transform.parent.gameObject.SetActive(value: false);
			}
			ShowEmptyAreaScreen();
			return;
		}
		ShowClassicAreaScreen(area);
		m_ShowDissolve = true;
		m_CurrentThreshold = 1f;
		if (!(m_LocationName == null))
		{
			m_LocationName.transform.parent.gameObject.SetActive(!string.IsNullOrWhiteSpace(area.AreaDisplayName) || !string.IsNullOrWhiteSpace(area.Name));
			if (!string.IsNullOrWhiteSpace(area.AreaDisplayName) || !string.IsNullOrWhiteSpace(area.Name))
			{
				m_LocationName.text = ((!string.IsNullOrWhiteSpace(area.AreaDisplayName)) ? area.AreaDisplayName : area.Name);
			}
		}
	}

	private void ShowEmptyAreaScreen()
	{
		m_LoadingScreenType = base.ViewModel.RandomLoadingScreen(3, RandomThreeWithPriorityPercents);
		if (m_LoadingScreenType == 0)
		{
			ShowCompanionScreen(m_MainMenuHints);
			return;
		}
		LoadingScreenImage keyArtTuple = m_KeyArtTuple;
		SetMainSprites(keyArtTuple);
		m_BottomTitleObject.SetActive(value: false);
		m_BottomTitleText.gameObject.SetActive(value: false);
		m_BottomDescriptionObject.SetActive(value: true);
		m_BottomDescriptionText.text = m_Hints.TakeHint(m_MainMenuHints, base.ViewModel.Random);
	}

	private void ShowClassicAreaScreen(BlueprintArea area)
	{
		LoadingScreenImage mainSprites = (area.LoadingScreenSprites.Any() ? new LoadingScreenImage?(area.LoadingScreenSprites.Random(PFStatefulRandom.UI)) : ((area.ArtSetting == BlueprintArea.SettingType.Unspecified) ? new LoadingScreenImage?(m_KeyArtTuple) : m_SettingTypeScreensList.FirstItem((SettingTypeScreens s) => s.Type == area.ArtSetting)?.Sprites.Random(PFStatefulRandom.UI))) ?? m_KeyArtTuple;
		StandartDescriptionOrHint(area, m_LocationHints);
		if (area.LoadingScreenSprites.Any() || area.ArtSetting != 0)
		{
			SetMainSprites(mainSprites);
			return;
		}
		m_LoadingScreenType = base.ViewModel.RandomLoadingScreen(2, RandomTwoWithPriorityPercents);
		if (m_LoadingScreenType == 0)
		{
			SetMainSprites(mainSprites);
		}
		else if (m_LoadingScreenType == 1)
		{
			ShowCompanionScreen(m_LocationHints);
		}
	}

	private void SwitchLoadingScreen(int screen)
	{
		m_MainContainer.SetActive(screen == 1);
		m_CharacterContainer.SetActive(screen == 2);
		m_IsCharacterScreen = screen == 2;
	}

	private void StandartKeyArtAndHint(LoadingScreenHints.LocationEnum locationEnum)
	{
		SwitchLoadingScreen(1);
		m_BottomTitleObject.SetActive(value: false);
		m_BottomTitleText.gameObject.SetActive(value: false);
		m_BottomDescriptionObject.SetActive(value: true);
		m_BottomDescriptionText.text = m_Hints.TakeHint(locationEnum, base.ViewModel.Random);
		SetMainSprites(m_KeyArtTuple);
	}

	private void StandartDescriptionOrHint(BlueprintArea area, LoadingScreenHints.LocationEnum locationEnum)
	{
		bool flag = string.IsNullOrWhiteSpace(area.Description);
		bool flag2 = string.IsNullOrWhiteSpace(area.AreaDisplayName) && string.IsNullOrWhiteSpace(area.Name);
		m_BottomTitleObject.SetActive(value: false);
		m_BottomTitleText.gameObject.SetActive(value: false);
		m_BottomDescriptionObject.SetActive(value: true);
		m_BottomDescriptionText.text = ((!flag && !flag2) ? area.Description : m_Hints.TakeHint(locationEnum, base.ViewModel.Random));
	}

	private void ShowCompanionScreen(LoadingScreenHints.LocationEnum locationEnum)
	{
		SwitchLoadingScreen(2);
		List<BaseUnitEntity> list = Game.Instance.Player.RemoteCompanions.ToTempList();
		List<BaseUnitEntity> actualCompanionsGroup = Game.Instance.Controllers.SelectionCharacter.ActualGroup;
		list.AddRange(Game.Instance.Player.Party.Where((BaseUnitEntity pc) => pc != Game.Instance.Player.MainCharacterEntity && !pc.IsPet && !pc.IsCustomCompanion() && actualCompanionsGroup.Contains(pc)));
		if (list.Empty())
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		List<BaseUnitEntity> list2 = list.Where((BaseUnitEntity c) => c.Portrait.LoadingPortrait != null).ToList();
		list2 = ((list2.Count > 1) ? list2.Where((BaseUnitEntity c) => c.Blueprint != RootUIContext.Instance.PreviousLoadingScreenCompanion?.Blueprint).ToList() : list2);
		if (list2.Empty())
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		CompanionStoriesManager companionsStories = Game.Instance.Player.CompanionStories;
		list2 = list2.Where(delegate(BaseUnitEntity c)
		{
			BlueprintCompanionStory blueprintCompanionStory2 = companionsStories.Get(c.ToBaseUnitEntity()).LastOrDefault();
			return blueprintCompanionStory2 != null && !string.IsNullOrWhiteSpace(blueprintCompanionStory2.Title) && !string.IsNullOrWhiteSpace(blueprintCompanionStory2.Description);
		}).ToList();
		if (list2.Empty())
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		UnitReference unitReference = list2[base.ViewModel.Random.Range(0, list2.Count)].FromBaseUnitEntity();
		if (unitReference == null)
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		BlueprintCompanionStory blueprintCompanionStory = companionsStories.Get(unitReference.Entity.ToBaseUnitEntity()).LastOrDefault();
		TextMeshProUGUI characterNameText = m_CharacterNameText;
		LocalizedString localizedString = blueprintCompanionStory?.Title;
		characterNameText.text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		TextMeshProUGUI characterDescriptionText = m_CharacterDescriptionText;
		localizedString = blueprintCompanionStory?.Description;
		characterDescriptionText.text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		SetCharacterSprites(unitReference.Entity.ToBaseUnitEntity().Portrait.LoadingPortrait, unitReference.Entity.ToBaseUnitEntity().Portrait.LoadingGlitchPortrait);
		m_CompanionOnLoadingScreen = unitReference.Entity.ToBaseUnitEntity();
	}

	private void UpdateThreshold()
	{
		if (m_ShowDissolve)
		{
			m_CurrentTime += Time.unscaledDeltaTime;
			float num = ((base.ViewModel.State == LoadingScreenState.HideAnimation) ? m_HidingDissolveTime : m_LoadingDissolveTime);
			if (!(m_CurrentTime < num * m_MinDissolveStep))
			{
				m_CurrentTime = 0f;
				float minDissolveStep = m_MinDissolveStep;
				float max = ((base.ViewModel.State == LoadingScreenState.HideAnimation) ? m_HidingMaxDissolveStep : m_MaxDissolveStep);
				float num2 = Mathf.Clamp(m_CurrentTime / num, minDissolveStep, max);
				m_CurrentThreshold -= num2;
				m_CurrentThreshold = Mathf.Clamp01(m_CurrentThreshold);
			}
		}
	}

	private IEnumerator LoadingProgressCoroutine()
	{
		while (m_VirtualProgress < 1f)
		{
			while (Game.Instance.IsLoadingProgressPaused)
			{
				yield return null;
			}
			SetProgress(Game.Instance.UILoadingProgress);
			yield return null;
		}
	}

	private void SetProgress(float progress)
	{
		m_Progress = Mathf.Max(progress, m_Progress);
		m_VirtualProgress = ((m_Progress > m_VirtualProgress) ? m_Progress : (m_VirtualProgress + Time.unscaledDeltaTime / 300f));
		ProgressImage.fillAmount = m_VirtualProgress;
		if (m_ProgressScrollbar != null)
		{
			m_ProgressScrollbar.value = m_VirtualProgress;
		}
		float num = Mathf.Clamp(m_VirtualProgress * 100f, 0f, 100f);
		ProgressPercent.text = UIUtilityText.AddPercentTo($"{num:0}");
		if (!m_OverlayFXActivated && m_OverlayFX != null && m_OverlayFX.material != null && num >= m_OverlayFXActivationThreshold)
		{
			m_OverlayFX.material.SetFloat(OverlayFXPowerIndex, 1f);
			m_OverlayFXActivated = true;
		}
	}

	protected virtual void ShowUserInputWaiting(bool state)
	{
		if (!state)
		{
			KillWaitUserInputAnimation();
			return;
		}
		UISounds.Instance.Play(FullScreenUniqueSounds.Instance.LoadingScreen.WaitForUserInputShow, isButton: false, playAnyway: true);
		m_ProgressBarContainer.DOFade(0f, 1f).OnComplete(StartPressAnyKeyLoopAnimation).SetUpdate(isIndependentUpdate: true);
		m_ProgressPercentContainer.DOFade(0f, 1f).OnComplete(StartPressAnyKeyLoopAnimation).SetUpdate(isIndependentUpdate: true);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			if (Input.anyKeyDown)
			{
				CloseWait();
			}
		}).AddTo(this);
	}

	protected void CloseWait()
	{
		ButtonsSounds.Instance.Default.Click.Play();
		UISounds.Instance.Play(FullScreenUniqueSounds.Instance.LoadingScreen.WaitForUserInputHide, isButton: false, playAnyway: true);
		if (PhotonManager.Lobby.IsLoading)
		{
			PhotonManager.Instance.ContinueLoading();
		}
		EventBus.RaiseEvent(delegate(IContinueLoadingHandler h)
		{
			h.HandleContinueLoading();
		});
	}

	private void StartPressAnyKeyLoopAnimation()
	{
		m_WaitForUserInputContainer.gameObject.SetActive(value: true);
		m_WaitForInputLoopAnimation = m_WaitForUserInputContainer.DOFade(1f, 0.8f).SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Yoyo)
			.OnKill(delegate
			{
				m_WaitForUserInputContainer.alpha = 0f;
				m_WaitForUserInputContainer.gameObject.SetActive(value: false);
			})
			.SetAutoKill(autoKillOnCompletion: false);
	}

	private void SetMainSprites(LoadingScreenImage tuple)
	{
		m_LoaddedTuple = tuple;
		BigArt.sprite = tuple.Main.Load(ignorePreloadWarning: false, hold: true);
		m_GlitchAnimator.SetGlitchImage(tuple.Glitch.Load(ignorePreloadWarning: false, hold: true));
	}

	private void SetCharacterSprites(Sprite main, Sprite glitch)
	{
		m_CharacterPortrait.sprite = main;
		m_GlitchAnimator.SetGlitchImage(glitch);
	}
}
