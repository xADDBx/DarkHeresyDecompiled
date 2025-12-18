using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.UI.MVVM.PreciseAttackOvertip;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using Kingmaker.Visual.Particles;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;

public class PreciseAttackPCView : View<PreciseAttackVM>
{
	private class FxLocatorPair
	{
		public FxLocator locator;

		public GameObject fx;
	}

	private const string BodyPartBindingPrefix = "PreciseAttackBodyPart";

	[SerializeField]
	private PreciseAttackLineView m_LineView;

	[SerializeField]
	private PreciseAttackOvertipView m_OvertipView;

	[SerializeField]
	private Color m_LineActiveColor;

	[SerializeField]
	private Color m_LineInactiveColor;

	[SerializeField]
	private PreciseAttackBodyPartView[] m_BodyParts;

	[Space]
	[SerializeField]
	private OwlcatMultiButton m_ConfirmButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmButtonBindText;

	[SerializeField]
	private string m_ConfirmButtonBindFormat;

	[SerializeField]
	private CanvasGroup m_ConfirmButtonContainer;

	[Space]
	[SerializeField]
	private OwlcatMultiButton m_BackgroundButton;

	private readonly List<GameObject> m_SpawnedBodyPartFxs = new List<GameObject>();

	private bool m_CanConfirm;

	private List<FxLocatorPair> fxLocatorPairs = new List<FxLocatorPair>();

	protected override void OnBind()
	{
		base.OnBind();
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(InternalUpdate).AddTo(this);
		m_BackgroundButton.OnRightClickAsObservable().Subscribe(OnCloseClick).AddTo(this);
		base.ViewModel.PointsUpdated.Subscribe(CreateBodyParts).AddTo(this);
		CreateBodyParts();
		m_ConfirmButton.OnLeftClickAsObservable().Subscribe(OnConfirmClick).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(OnCloseClick).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindPreciseAttackSettings.PreciseAttackConfirm.name, OnConfirmClick).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindPreciseAttackSettings.PreciseAttackPrevTarget.name, base.ViewModel.SelectPrev).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindPreciseAttackSettings.PreciseAttackNextTarget.name, base.ViewModel.SelectNext).AddTo(this);
		m_ConfirmButtonBindText.text = GetConfirmButtonBindText();
		m_OvertipView.Bind(base.ViewModel.OvertipVM);
	}

	protected override void OnUnbind()
	{
		ClearBodyParts();
		m_OvertipView.Unbind();
	}

	private void InternalUpdate()
	{
		m_LineView.UpdatePosition();
	}

	private void CreateBodyParts()
	{
		ClearBodyParts();
		int num = 0;
		foreach (PreciseAttackController.BodyPartUIData bodyPart in base.ViewModel.BodyParts)
		{
			ActivateBodyPart(num, bodyPart);
			num++;
		}
	}

	private void ActivateBodyPart(int index, PreciseAttackController.BodyPartUIData bodyPartData)
	{
		if (bodyPartData.RestrictionPassed)
		{
			BlueprintBodyPart bodyPart = bodyPartData.BodyPart;
			int num = Math.Min(index, m_BodyParts.Length - 1);
			PreciseAttackBodyPartView bodyPartView = m_BodyParts[num];
			float hitChance = base.ViewModel.GetHitChance(bodyPart);
			string chance = Mathf.Round(hitChance).ToString(CultureInfo.InvariantCulture);
			string text = (index + 1).ToString();
			bodyPartView.Initialize(bodyPartData, text, bodyPart.Name.Text, chance, bodyPart.IsVital);
			PreciseAttackBodyPartView.BodyPartState state = ((!(hitChance > 0f) || bodyPartData.IsHiddenByCover) ? PreciseAttackBodyPartView.BodyPartState.Unavailable : PreciseAttackBodyPartView.BodyPartState.Available);
			bodyPartView.SetState(state);
			ObservableSubscribeExtensions.Subscribe(bodyPartView.Button.OnLeftClickAsObservable(), delegate
			{
				OnBodyPartSelect(bodyPartView);
			}).AddTo(this);
			bodyPartView.Button.OnHoverAsObservable().Subscribe(delegate(bool hover)
			{
				OnBodyPartHover(bodyPartView, hover);
			}).AddTo(this);
			Game.Instance.Keyboard.Bind("PreciseAttackBodyPart" + text, delegate
			{
				OnBodyPartSelect(bodyPartView, forceSetHover: true);
			}).AddTo(this);
			if (base.ViewModel.SelectedBodyPart == null)
			{
				OnBodyPartSelect(bodyPartView);
				OnBodyPartHover(null, hover: false);
			}
		}
	}

	private void OnCloseClick()
	{
		base.ViewModel.Close();
	}

	private void OnConfirmClick()
	{
		if (!m_CanConfirm)
		{
			return;
		}
		if (!CheckBodyPartAvailable(base.ViewModel.SelectedBodyPart, out var unavailabilityReason))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(unavailabilityReason, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			base.ViewModel.Accept();
			ClearBodyParts();
		}
	}

	private void ClearBodyPartsFxs()
	{
		foreach (GameObject spawnedBodyPartFx in m_SpawnedBodyPartFxs)
		{
			UnityEngine.Object.Destroy(spawnedBodyPartFx);
		}
		m_SpawnedBodyPartFxs.Clear();
		fxLocatorPairs.Clear();
	}

	private void OnBodyPartSelect(PreciseAttackBodyPartView clickedBodyPartView, bool forceSetHover = false)
	{
		ClearBodyPartsFxs();
		PreciseAttackController.BodyPartUIData bodyPartData = clickedBodyPartView.BodyPartData;
		if (bodyPartData == base.ViewModel.SelectedBodyPart)
		{
			return;
		}
		if (forceSetHover)
		{
			base.ViewModel.SetHoveredBodyPart(bodyPartData);
		}
		if (!CheckBodyPartAvailable(bodyPartData, out var unavailabilityReason))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(unavailabilityReason, addToLog: false, WarningNotificationFormat.Attention);
			});
			return;
		}
		PreciseAttackBodyPartView[] bodyParts = m_BodyParts;
		foreach (PreciseAttackBodyPartView obj in bodyParts)
		{
			obj.SetFocused(obj.BodyPartData == bodyPartData);
		}
		base.ViewModel.SetSelectedBodyPart(bodyPartData);
		m_CanConfirm = clickedBodyPartView.State == PreciseAttackBodyPartView.BodyPartState.Available;
		SetConfirmButtonActive(m_CanConfirm);
		DrawVisual(bodyPartData);
		Color color = (m_CanConfirm ? m_LineActiveColor : m_LineInactiveColor);
		m_LineView.SetColor(color);
	}

	private void OnBodyPartHover(PreciseAttackBodyPartView hoveredBodyPartView, bool hover)
	{
		PreciseAttackController.BodyPartUIData hoveredBodyPart = ((!hover) ? null : hoveredBodyPartView?.BodyPartData);
		base.ViewModel.SetHoveredBodyPart(hoveredBodyPart);
	}

	private void DrawVisual(PreciseAttackController.BodyPartUIData bodyPartData)
	{
		List<FxLocator> bodyPartBone = base.ViewModel.GetBodyPartBone(bodyPartData);
		DrawLine(bodyPartData, bodyPartBone);
		DrawBodyPartsFx(bodyPartBone);
	}

	private void DrawLine(PreciseAttackController.BodyPartUIData bodyPartData, List<FxLocator> locators)
	{
		Transform locatorTransform = ((locators.Count > 0) ? locators[UnityEngine.Random.Range(0, locators.Count)].transform : null);
		PreciseAttackBodyPartView preciseAttackBodyPartView = m_BodyParts.FirstOrDefault((PreciseAttackBodyPartView b) => b.BodyPartData == bodyPartData);
		if (preciseAttackBodyPartView == null)
		{
			PFLog.UI.Error("PreciseAttackPCView.DrawLine: cant find body part " + bodyPartData.BodyPart.Name.Text + " for unit " + base.ViewModel.Unit.View.ViewTransform.name + " in scene " + base.ViewModel.Unit.View.gameObject.scene.name);
		}
		else
		{
			m_LineView.SetLine(locatorTransform, preciseAttackBodyPartView.LineStartRect);
		}
	}

	private void DrawBodyPartsFx(List<FxLocator> locators)
	{
		if (locators.Count == 0)
		{
			return;
		}
		GameObject gameObject = ConfigRoot.Instance.FxRoot.BodyPartHighlightFx.Load();
		if (gameObject == null)
		{
			PFLog.TechArt.Error("No BodyPartHighlightFx prefab in ConfigRoot.FxRoot.");
		}
		foreach (FxLocator locator in locators)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			FxLocatorPair item = new FxLocatorPair
			{
				fx = gameObject2,
				locator = locator
			};
			gameObject2.transform.position = locator.transform.position;
			float particleSize = locator.Data.ParticleSize;
			gameObject2.transform.localScale = new Vector3(gameObject2.transform.localScale.x * particleSize, gameObject2.transform.localScale.y * particleSize, gameObject2.transform.localScale.z * particleSize);
			m_SpawnedBodyPartFxs.Add(gameObject2);
			fxLocatorPairs.Add(item);
		}
	}

	private void LateUpdate()
	{
		if (fxLocatorPairs.Count <= 0)
		{
			return;
		}
		foreach (FxLocatorPair fxLocatorPair in fxLocatorPairs)
		{
			fxLocatorPair.fx.transform.position = fxLocatorPair.locator.transform.position;
		}
	}

	private void SetConfirmButtonActive(bool active)
	{
		m_ConfirmButtonContainer.alpha = (active ? 1f : 0f);
		m_ConfirmButtonContainer.blocksRaycasts = active;
	}

	private void ClearBodyParts()
	{
		SetConfirmButtonActive(active: false);
		PreciseAttackBodyPartView[] bodyParts = m_BodyParts;
		for (int i = 0; i < bodyParts.Length; i++)
		{
			bodyParts[i].Clear();
		}
		ClearBodyPartsFxs();
	}

	private bool CheckBodyPartAvailable(PreciseAttackController.BodyPartUIData bodyPartData, out string unavailabilityReason)
	{
		unavailabilityReason = string.Empty;
		if (!bodyPartData.RestrictionPassed)
		{
			unavailabilityReason = LocalizedTexts.Instance.Reasons.CannotTargetBodyPartGeneric;
			return false;
		}
		if (bodyPartData.BodyPart.Tags.HasAnyFlag(BodyPartTags.PreciseIgnore))
		{
			unavailabilityReason = string.Format(LocalizedTexts.Instance.Reasons.CannotTargetBodyPartByPreciseAttack, bodyPartData.BodyPart.Name.Text);
			return false;
		}
		return true;
	}

	private string GetConfirmButtonBindText()
	{
		string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("PreciseAttackConfirm"));
		if (!string.IsNullOrEmpty(m_ConfirmButtonBindFormat))
		{
			try
			{
				return string.Format(m_ConfirmButtonBindFormat, stringByBinding);
			}
			catch (FormatException exception)
			{
				Debug.LogException(exception);
			}
		}
		return stringByBinding;
	}

	private void Awake()
	{
		m_ConfirmButtonLabel.text = LocalizedTexts.Instance.PreciseAttack.Confirm.Text;
		ClearBodyParts();
	}
}
