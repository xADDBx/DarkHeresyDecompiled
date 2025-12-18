using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipMapObjectInteractionView : BaseOvertipMapObjectView
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private OwlcatMultiSelectable m_VariantsSelectable;

	[SerializeField]
	protected OwlcatMultiSelectable m_AppearanceStateSelectable;

	[SerializeField]
	private List<OwlcatMultiSelectable> m_StateSelectables;

	[SerializeField]
	private AdditionalCombatHintView AdditionalCombatHintView;

	[Header("Parts Block")]
	[FormerlySerializedAs("m_BarkBlockPCView")]
	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockView;

	[FormerlySerializedAs("m_OvertipNameBlockPCView")]
	[SerializeField]
	private MapObjectOvertipNameBlockView m_OvertipNameBlockView;

	[FormerlySerializedAs("m_OvertipSkillCheckBlockPCView")]
	[SerializeField]
	private MapObjectOvertipSkillCheckBlockView m_OvertipSkillCheckBlockView;

	[SerializeField]
	private TextMeshProUGUI m_ResourceCount;

	[Header("Images")]
	[SerializeField]
	private Image m_IconImage;

	[Header("Common Block")]
	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	private IDisposable m_AddHintDisposable;

	private Transform m_ScalePivot;

	private float? m_PreviousScale;

	private float? m_PreviousAlpha;

	private static BlueprintInteractionRoot.OvertipVisualSettings VisualSettings => ConfigRoot.Instance.Interaction.MapObjectOvertipsVisualSettings;

	private bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.IsMouseOverUI.CurrentValue && !base.ViewModel.MapObjectIsHighlighted.CurrentValue && (!base.ViewModel.ForceHotKeyPressed.CurrentValue || !base.ViewModel.CanBeForceShown) && !base.ViewModel.ActiveCharacterIsNear && !base.ViewModel.IsBarkActive.CurrentValue)
			{
				return base.ViewModel.IsInCombat;
			}
			return true;
		}
	}

	protected override bool CheckVisibility => base.CheckCanBeVisible;

	protected override bool ForceUpdatePosition => base.ViewModel.IsBarkActive.CurrentValue;

	protected override void OnBind()
	{
		base.OnBind();
		m_ScalePivot = base.transform;
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			m_AddHintDisposable?.Dispose();
			AdditionalCombatHintView?.Unbind();
			base.ViewModel.Interact();
		}).AddTo(this);
		base.ViewModel.IsVariative.Subscribe(delegate(bool value)
		{
			string activeLayer = (value ? "Variants" : "Default");
			m_VariantsSelectable.SetActiveLayer(activeLayer);
		}).AddTo(this);
		m_Button.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (GameUIState.Instance.IsInCombat.CurrentValue && (!base.ViewModel.IsVariative.CurrentValue || RootVM.Instance.OvertipsContext.OpenedVariantsMapEntity != base.ViewModel.MapObjectEntity))
			{
				m_AddHintDisposable?.Dispose();
				m_AddHintDisposable = base.ViewModel.AddCombatInfo.Subscribe(delegate(AdditionalCombatHintVM value)
				{
					AdditionalCombatHintView.Bind(value);
					if (value != null)
					{
						base.ViewModel.FirstInteractionPart.SetVisited();
					}
				}).AddTo(this);
			}
		}).AddTo(this);
		m_Button.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (base.ViewModel.AddCombatInfo.CurrentValue != null)
			{
				base.ViewModel.UpdateInteraction(active: false);
			}
			m_AddHintDisposable?.Dispose();
			AdditionalCombatHintView?.Unbind();
		}).AddTo(this);
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_InteractionOvertip";
		m_BarkBlockView.Bind(base.ViewModel.BarkBlockVM);
		m_OvertipNameBlockView.Bind(base.ViewModel);
		m_OvertipSkillCheckBlockView.Bind(base.ViewModel);
		base.ViewModel.CurrentState.Subscribe(delegate(OvertipState state)
		{
			float num = ((state == OvertipState.Default) ? 0.85f : 1f);
			DOTween.Kill(m_IconImage.transform);
			m_IconImage.transform.DOScale(Vector3.one * num, 0.25f).SetUpdate(isIndependentUpdate: true);
			m_AppearanceStateSelectable.SetActiveLayer(state.ToString());
			SetupSprites();
		}).AddTo(this);
		SetupSprites();
		EventBus.Subscribe(this).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(base.ViewModel.VisibilityChanged, delegate
		{
			if (CheckVisibility)
			{
				UpdateVisibility();
			}
		}).AddTo(this);
		base.ViewModel.CombatObjStateChanged.Subscribe(SetupSprites).AddTo(this);
		base.ViewModel.CanInteract.Subscribe(SetInteractable).AddTo(this);
		if (base.ViewModel.RequiredResourceCount.HasValue)
		{
			base.ViewModel.HasResourceCount.Subscribe(delegate(int? value)
			{
				m_ResourceCount.text = ((value > 0) ? $"{value}" : string.Empty);
				m_Button.SetInteractable(value >= base.ViewModel.RequiredResourceCount.Value);
			}).AddTo(this);
		}
		else
		{
			m_ResourceCount.text = string.Empty;
		}
	}

	private void UpdateVisibility()
	{
		float magnitude = base.ViewModel.CameraDistance.CurrentValue.magnitude;
		bool flag = base.ViewModel.ActiveCharacterIsNear || CheckVisibleTrigger;
		float value2;
		float value;
		if (OvertipUtils.IsDetectiveInteract(base.ViewModel.Type) && (base.ViewModel.MapObjectEntity is DetectiveTraceEntity || (base.ViewModel.MapObjectEntity is DetectiveClueEntity detectiveClueEntity && !detectiveClueEntity.View.Signal.IsJammer)))
		{
			value = (base.ViewModel.ActiveCharacterIsNear ? 1f : 0f);
			value2 = (base.ViewModel.ActiveCharacterIsNear ? 1f : 0f);
		}
		else
		{
			value = (flag ? 1f : CalculateScaleFactor(magnitude));
			value2 = (flag ? VisualSettings.OnHoverVisibility : LerpScale(magnitude, VisualSettings.DefaultSizeMaxDistance, VisualSettings.ReducedSizeMaxDistance, VisualSettings.OnHoverVisibility, VisualSettings.NotHoverVisibility));
		}
		value2 = Mathf.Clamp01(value2);
		value = Mathf.Clamp01(value);
		if (!m_PreviousScale.HasValue || Mathf.Abs(m_PreviousScale.Value - value) > float.Epsilon)
		{
			m_PreviousScale = value;
			m_ScalePivot.localScale = Vector3.one * value;
		}
		if (!m_PreviousAlpha.HasValue || Mathf.Abs(m_PreviousAlpha.Value - value2) > float.Epsilon)
		{
			m_PreviousAlpha = value2;
			m_InnerCanvasGroup.alpha = value2;
		}
	}

	protected override void SetActiveInternal(bool isActive)
	{
		base.SetActiveInternal(isActive);
		UpdateVisibility();
	}

	private float CalculateScaleFactor(float cameraDistance)
	{
		float num = Mathf.Max(VisualSettings.DefaultSizeMaxDistance, base.ViewModel.FirstInteractionPart.ApproachRadius.Cells().Meters);
		if (cameraDistance < num)
		{
			return 1f;
		}
		if (!ConfigRoot.Instance.Interaction.ReduceIconsSize)
		{
			return 0f;
		}
		if (cameraDistance < VisualSettings.ReducedSizeMinDistance)
		{
			return LerpScale(cameraDistance, VisualSettings.DefaultSizeMaxDistance, VisualSettings.ReducedSizeMinDistance, 1f, VisualSettings.ReducedSizeValue);
		}
		if (cameraDistance < VisualSettings.ReducedSizeMaxDistance)
		{
			return LerpScale(cameraDistance, VisualSettings.ReducedSizeMinDistance, VisualSettings.ReducedSizeMaxDistance, VisualSettings.ReducedSizeValue, 0f);
		}
		return 0f;
	}

	private float LerpScale(float current, float min, float max, float startValue, float endValue)
	{
		float t = (current - min) / (max - min);
		return Mathf.Lerp(startValue, endValue, t);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_BarkBlockView.Unbind();
		m_PreviousScale = null;
		m_PreviousAlpha = null;
	}

	protected override void UpdateActive(bool isActive)
	{
		m_InnerCanvasGroup.blocksRaycasts = isActive;
	}

	private void SetupSprites()
	{
		UIInteractionType uIInteractionType = base.ViewModel.FirstInteractionPart?.UIInteractionType ?? UIInteractionType.Action;
		Sprite interactionIcon = ConfigRoot.Instance.Interaction.GetInteractionIcon(uIInteractionType);
		m_IconImage.sprite = interactionIcon;
		string stateLayer = GetStateLayer(uIInteractionType);
		m_StateSelectables.ForEach(delegate(OwlcatMultiSelectable s)
		{
			s.SetActiveLayer(stateLayer);
		});
	}

	private string GetStateLayer(UIInteractionType uiInteractionType)
	{
		if (OvertipUtils.IsAdditionalCombatOvertip(base.ViewModel.MapObjectEntity) && Game.Instance.Player.IsInCombat)
		{
			return "AdditionalCombat";
		}
		if (!OvertipUtils.IsDetectiveInteract(uiInteractionType))
		{
			return "Default";
		}
		return "Detective";
	}

	protected void SetInteractable(bool interactable)
	{
		m_Button.Interactable = interactable;
	}
}
