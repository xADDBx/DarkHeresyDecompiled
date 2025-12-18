using System.Collections;
using DG.Tweening;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GameConst;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.SkipCutscene;

public class SkipCutscenePCView : View<SkipCutsceneVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextMeshProUGUI m_SkipText;

	[SerializeField]
	private TextMeshProUGUI m_SkipHotKey;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_ButtonCanvasGroup;

	[SerializeField]
	private Image m_FillImage;

	[Header("Values")]
	[SerializeField]
	private float m_FillDuration = 1.5f;

	[SerializeField]
	private float m_ButtonShowTime = 0.75f;

	[SerializeField]
	private float m_ButtonShowDelay = 3f;

	private IEnumerator m_FillSkipCutsceneCoroutine;

	private bool m_IsFillingSkipCutscene;

	private const float FillTarget = 100f;

	private readonly ReactiveProperty<float> m_CurrentFill = new ReactiveProperty<float>(0f);

	protected override void OnBind()
	{
		_ = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		SetSkipCutsceneSettings();
		m_SkipText.text = UIStrings.Instance.CommonTexts.Skip.Text;
		m_SkipHotKey.text = UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.GetBinding(0).GetPrettyString();
		m_CurrentFill.Subscribe(delegate(float value)
		{
			m_FillImage.fillAmount = value / 100f;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			OnCutSceneDecline();
		}).AddTo(this);
		m_ButtonCanvasGroup.DOFade(1f, m_ButtonShowTime).SetDelay(m_ButtonShowDelay).SetUpdate(isIndependentUpdate: true)
			.OnComplete(delegate
			{
				m_Button.Interactable = true;
			});
	}

	protected override void OnUnbind()
	{
		if (m_FillSkipCutsceneCoroutine != null)
		{
			StopCoroutine(m_FillSkipCutsceneCoroutine);
			m_FillSkipCutsceneCoroutine = null;
		}
		m_CurrentFill.Value = 0f;
		m_Button.Interactable = false;
		DOTween.Kill(m_ButtonCanvasGroup);
		m_ButtonCanvasGroup.alpha = 0f;
	}

	private void SetSkipCutsceneSettings()
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOn, delegate
		{
			TrySkipCutscene(state: true);
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOff, delegate
		{
			TrySkipCutscene(state: false);
		}).AddTo(this);
	}

	private void HandleSkipCutsceneHintState(bool value)
	{
		if (!Game.Instance.EntityPools.Cutscenes.TryFind((CutscenePlayerData p) => p.HasActiveLockControl && p.Cutscene.NonSkippable, out var _))
		{
			if (m_FillSkipCutsceneCoroutine != null && !m_IsFillingSkipCutscene)
			{
				StopCoroutine(m_FillSkipCutsceneCoroutine);
			}
			if (value)
			{
				string prettyString = UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.GetBinding(0).GetPrettyString();
				m_SkipText.text = UIStrings.Instance.CommonTexts.Skip.Text;
				m_SkipHotKey.text = prettyString;
			}
		}
	}

	private void OnCutSceneDecline()
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.Cutscene))
		{
			EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
			{
				h.HandleOnHideBark();
			});
			m_IsFillingSkipCutscene = false;
			Game.Instance.GameCommandQueue.SkipCutscene();
		}
	}

	public void TrySkipCutscene(bool state)
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.Cutscene))
		{
			if (state && !m_IsFillingSkipCutscene)
			{
				m_IsFillingSkipCutscene = true;
				m_FillSkipCutsceneCoroutine = FillSkipCutsceneCoroutine();
				StartCoroutine(m_FillSkipCutsceneCoroutine);
			}
			else if (!state && m_IsFillingSkipCutscene)
			{
				m_IsFillingSkipCutscene = false;
				m_CurrentFill.Value = 0f;
				HandleSkipCutsceneHintState(value: true);
			}
		}
	}

	private IEnumerator FillSkipCutsceneCoroutine()
	{
		float elapsedTime = 0f;
		float startFill = m_CurrentFill.Value;
		while (elapsedTime < m_FillDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / m_FillDuration);
			m_CurrentFill.Value = Mathf.Lerp(startFill, 100f, t);
			yield return null;
		}
		m_CurrentFill.Value = 100f;
		OnCutSceneDecline();
	}
}
