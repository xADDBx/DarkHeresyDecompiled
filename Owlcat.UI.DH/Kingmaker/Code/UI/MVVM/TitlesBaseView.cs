using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Transitions;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TitlesBaseView : View<TitlesVM>, ICreditsView, ITransitable
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	private FadeAnimator m_TheEndTitleAnimator;

	[SerializeField]
	private TextMeshProUGUI m_TheEndTitle;

	[FormerlySerializedAs("m_ScrumbledTheEndTitle")]
	[SerializeField]
	private ScrambledTMP m_ScrambledTheEndTitle;

	[SerializeField]
	private RectTransform m_Content;

	[Header("Prefabs and Misc")]
	[SerializeField]
	protected CreditsOneColumnPage m_OneColumnPrefab;

	[SerializeField]
	protected CreditsTwoColumnsPage m_MultipleColumnsPrefab;

	[SerializeField]
	protected Sprite m_FirstBackground;

	[SerializeField]
	protected float m_ScrollSpeedPxPerSec = 80f;

	[SerializeField]
	protected float m_ScrollSpeedUpMultiplyer = 2f;

	[SerializeField]
	protected float m_StartDelayTime = 3f;

	[SerializeField]
	protected float m_ChangeScreenTime = 10f;

	[SerializeField]
	protected float m_ScrollBottomOffset = 1080f;

	[SerializeField]
	protected float m_DestinationBgrAlpha = 0.75f;

	[SerializeField]
	protected float m_BgrFadeTime = 1f;

	[Header("Backgrounds")]
	[SerializeField]
	private List<Image> m_BackgroundImages;

	private readonly List<CreditElement> m_VisibleElements = new List<CreditElement>();

	private readonly List<CreditElement> m_ElementsToDestroy = new List<CreditElement>();

	private bool m_TheEndShowed = true;

	private bool m_IsSpeedUp;

	private Tweener m_Tweener;

	private int m_BgrPointer;

	private int m_CurrentBgrImageId;

	private float m_PastTime;

	private bool m_WaitingEndDelay;

	private float m_EndTime;

	private SpriteLink m_OldBackgroundSprite;

	private SpriteLink m_CurrentBackgroundSprite;

	private SpriteLink m_NextBackgroundSprite;

	private int m_StringIndex;

	private float m_BaseHeight;

	private bool m_WaitingLayout;

	private float m_NextChangeScreenTime;

	private bool m_Initialized;

	private UIAnimatorTransition m_ShowTransition;

	private UIAnimatorTransition m_HideTransition;

	private List<SpriteLink> BackgroundSprites => UIConfig.Instance.Credits.BackgroundSprites;

	public Transform Content => m_Content;

	Transition ITransitable.Show()
	{
		m_TheEndShowed = true;
		return m_ShowTransition.Run(HandleOnShown);
	}

	Transition ITransitable.Hide()
	{
		return m_HideTransition.Run(HandleOnHidden);
	}

	protected override void OnBind()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_FadeAnimator.Initialize();
			m_TheEndTitleAnimator.Initialize();
			m_BaseHeight = m_Content.rect.height;
		}
		if (m_ShowTransition == null)
		{
			m_ShowTransition = new UIAnimatorShowTransition(m_FadeAnimator);
		}
		if (m_HideTransition == null)
		{
			m_HideTransition = new UIAnimatorHideTransition(m_FadeAnimator);
		}
		m_TheEndTitle.text = string.Empty;
		ClearBackgrounds();
		Observable.EveryUpdate().Subscribe(MoveTitles).AddTo(this);
		SetupFirstBackground();
		SetSoundSettings(state: true);
	}

	protected override void OnUnbind()
	{
		SetSoundSettings(state: false);
	}

	protected void SpeedUp(bool state)
	{
		m_IsSpeedUp = state;
	}

	private void HandleOnShown()
	{
		DrawTheEnd();
		CreateTitles();
	}

	private void HandleOnHidden()
	{
		m_WaitingEndDelay = false;
		m_PastTime = 0f;
		m_Tweener.Kill();
		foreach (CreditElement visibleElement in m_VisibleElements)
		{
			Object.Destroy(visibleElement.gameObject);
		}
		m_VisibleElements.Clear();
		m_BgrPointer = 0;
		m_CurrentBgrImageId = 0;
		m_StringIndex = 0;
		m_Content.anchoredPosition = Vector2.zero;
		m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, m_BaseHeight);
		m_NextChangeScreenTime = 0f;
	}

	private void DrawTheEnd()
	{
		m_ScrambledTheEndTitle.SetText(string.Empty, UIStrings.Instance.Credits.TheEndText);
	}

	private void MoveTitles()
	{
		m_PastTime += Time.unscaledDeltaTime;
		if (m_PastTime < m_StartDelayTime)
		{
			return;
		}
		if (m_PastTime > m_NextChangeScreenTime)
		{
			float num = (m_IsSpeedUp ? (m_ChangeScreenTime / m_ScrollSpeedUpMultiplyer) : m_ChangeScreenTime);
			m_NextChangeScreenTime = m_PastTime + num;
			ChangeBgrImage();
		}
		if (m_TheEndShowed)
		{
			m_TheEndTitleAnimator.DisappearAnimation();
			m_TheEndShowed = false;
		}
		if (m_WaitingEndDelay)
		{
			if (m_PastTime < m_EndTime + m_StartDelayTime)
			{
				return;
			}
			base.ViewModel.CloseTitles();
		}
		if (!m_WaitingLayout && m_Content.anchoredPosition.y > m_Content.rect.height - m_ScrollBottomOffset && !TryLoadNext())
		{
			m_EndTime = m_PastTime;
			m_WaitingEndDelay = true;
			return;
		}
		float num2 = Time.deltaTime * m_ScrollSpeedPxPerSec;
		if (m_IsSpeedUp)
		{
			num2 *= m_ScrollSpeedUpMultiplyer;
		}
		m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, m_Content.anchoredPosition.y + num2);
		CheckGoneElemets();
	}

	private void CheckGoneElemets()
	{
		foreach (CreditElement visibleElement in m_VisibleElements)
		{
			RectTransform rectTransform = visibleElement.transform as RectTransform;
			if ((double)(Mathf.Abs(rectTransform.anchoredPosition.y) + rectTransform.rect.height) < (double)m_Content.anchoredPosition.y - (double)m_ScrollBottomOffset * 1.2)
			{
				m_ElementsToDestroy.Add(visibleElement);
			}
		}
		foreach (CreditElement item in m_ElementsToDestroy)
		{
			m_VisibleElements.Remove(item);
			item.Destroy();
		}
		m_ElementsToDestroy.Clear();
	}

	private void SetSoundSettings(bool state)
	{
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state, ModalWindowUIType.GameEndingTitles);
		});
		SoundState.Instance.OnMusicStateChange((!state) ? MusicStateHandler.MusicState.MainMenu : MusicStateHandler.MusicState.Credits);
	}

	private void ClearBackgrounds()
	{
		foreach (Image backgroundImage in m_BackgroundImages)
		{
			backgroundImage.DOKill();
			backgroundImage.color = new Color(1f, 1f, 1f, 0f);
		}
	}

	private void SetupFirstBackground()
	{
		if ((bool)m_FirstBackground)
		{
			m_BackgroundImages[m_CurrentBgrImageId].DOKill();
			m_BackgroundImages[m_CurrentBgrImageId].sprite = m_FirstBackground;
			m_BackgroundImages[m_CurrentBgrImageId].DOFade(m_DestinationBgrAlpha, m_BgrFadeTime);
		}
	}

	private bool TryLoadNext()
	{
		if (base.ViewModel?.Titles == null || m_StringIndex >= base.ViewModel.Titles.Count)
		{
			return false;
		}
		(string, BlueprintCreditsGroup) titlesBlock = base.ViewModel.Titles[m_StringIndex];
		bool isSingleBlock = titlesBlock.Item2.TeamsData != null || !string.IsNullOrEmpty(PageGenerator.ReadCompany(titlesBlock.Item1));
		ICreditsBlock blockView = GetBlockView(isSingleBlock);
		FillBlockView(titlesBlock, blockView);
		m_StringIndex++;
		return true;
	}

	private ICreditsBlock GetBlockView(bool isSingleBlock)
	{
		ICreditsBlock creditsBlock;
		if (!isSingleBlock)
		{
			ICreditsBlock instance = m_MultipleColumnsPrefab.GetInstance<CreditsTwoColumnsPage>();
			creditsBlock = instance;
		}
		else
		{
			ICreditsBlock instance = m_OneColumnPrefab.GetInstance<CreditsOneColumnPage>();
			creditsBlock = instance;
		}
		ICreditsBlock creditsBlock2 = creditsBlock;
		creditsBlock2.Initialize(this);
		RectTransform rect = (creditsBlock2 as CreditElement).transform as RectTransform;
		rect.anchoredPosition = new Vector2(0f, 0f - m_Content.sizeDelta.y);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
		m_VisibleElements.Add(creditsBlock2 as CreditElement);
		m_WaitingLayout = true;
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, m_Content.rect.height + rect.rect.height);
			m_WaitingLayout = false;
		}).AddTo(this);
		return creditsBlock2;
	}

	private static void FillBlockView((string, BlueprintCreditsGroup) titlesBlock, ICreditsBlock block)
	{
		using StringReader stringReader = new StringReader(titlesBlock.Item1);
		string row;
		while (!string.IsNullOrEmpty(row = stringReader.ReadLine()))
		{
			block.Append(row, titlesBlock.Item2);
		}
	}

	private void CreateTitles()
	{
		if (base.ViewModel.Titles == null)
		{
			base.ViewModel.TryGenerateTitles();
		}
		TryLoadNext();
	}

	private void ChangeBgrImage()
	{
		m_BgrPointer++;
		if (m_BgrPointer >= BackgroundSprites.Count)
		{
			m_BgrPointer = 0;
		}
		if (!(BackgroundSprites.ElementAtOrDefault(m_BgrPointer) == null))
		{
			m_OldBackgroundSprite = m_CurrentBackgroundSprite;
			m_CurrentBackgroundSprite = m_NextBackgroundSprite ?? BackgroundSprites[m_BgrPointer];
			if (BackgroundSprites.ElementAtOrDefault(m_BgrPointer + 1) != null)
			{
				m_NextBackgroundSprite = BackgroundSprites[m_BgrPointer + 1];
				m_NextBackgroundSprite.LoadAsync();
			}
			m_BackgroundImages[m_CurrentBgrImageId].DOKill();
			m_BackgroundImages[m_CurrentBgrImageId].DOFade(0f, m_BgrFadeTime);
			m_CurrentBgrImageId = (m_CurrentBgrImageId + 1) % m_BackgroundImages.Count;
			m_BackgroundImages[m_CurrentBgrImageId].sprite = m_CurrentBackgroundSprite.Load();
			m_BackgroundImages[m_CurrentBgrImageId].DOKill();
			m_BackgroundImages[m_CurrentBgrImageId].DOFade(m_DestinationBgrAlpha, m_BgrFadeTime).OnComplete(delegate
			{
				m_OldBackgroundSprite?.ForceUnload();
			});
		}
	}
}
