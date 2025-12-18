using System;
using Code.View.UI.UIUtils;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.CodeTimer;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoView : View<UnitInfoVM>
{
	[SerializeField]
	private RectTransform m_Root;

	[SerializeField]
	private RectTransform m_ScaleRoot;

	[SerializeField]
	private RectTransform m_SelectHintContainer;

	[Space]
	[SerializeField]
	private Vector2 m_AnchoredPositionOffset;

	[SerializeField]
	private Vector2 m_ReferenceScreenSize = new Vector2(1920f, 1080f);

	[SerializeField]
	private Vector3 m_PreciseAttackPosition;

	[SerializeField]
	private float m_PreciseAttackScale = 1f;

	[SerializeField]
	private float m_DefaultScale = 0.6f;

	[Space]
	[SerializeField]
	private TextMeshProUGUI m_RMBHintLabel;

	[SerializeField]
	private TextMeshProUGUI m_LMBHintLabel;

	[Space]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private FadeAnimator m_CompareBlockAnimator;

	[Header("Parts")]
	[SerializeField]
	private RectTransform m_PartsRectTransform;

	[SerializeField]
	private VerticalLayoutGroup m_PartsVerticalLayoutGroup;

	[SerializeField]
	private Transform m_PartsIgnoreLayout;

	[SerializeField]
	private UnitInfoPart[] m_UnitInfoParts;

	[Space]
	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Color m_PreciseAttackBackroundColor;

	[SerializeField]
	private Color m_DefaultBackroundColor;

	private bool m_IsVisible;

	private RectTransform m_ParentRectTransform;

	private RectTransform ParentRectTransform
	{
		get
		{
			if (!(m_ParentRectTransform != null))
			{
				return m_ParentRectTransform = m_Root.parent.transform as RectTransform;
			}
			return m_ParentRectTransform;
		}
	}

	protected override void OnBind()
	{
		GameUIState instance = GameUIState.Instance;
		base.ViewModel.IsHover.CombineLatest(base.ViewModel.IsPreciseAttack, base.ViewModel.HUDContext.ForceHotKeyPressed, base.ViewModel.HUDContext.IsTurnBasedActive, base.ViewModel.HUDContext.DeploymentPhase, base.ViewModel.HUDContext.IsPlayer, base.ViewModel.Data.IsDeadOrUnconsciousIsDead, base.ViewModel.HasAbility, base.ViewModel.Data.IsTarget, instance.CurrentFullScreenUIType, (bool hover, bool isPreciseAttack, bool forceHotKeyPressed, bool isTBM, bool isPreparationTurn, bool isPlayerTurn, bool isDeadOrUnconsciousIsDead, bool hasAbility, bool isTarget, FullScreenUIType currentFullScreenUIType) => new { hover, isPreciseAttack, forceHotKeyPressed, isTBM, isPreparationTurn, isPlayerTurn, isDeadOrUnconsciousIsDead, hasAbility, isTarget, currentFullScreenUIType }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			bool num = value.currentFullScreenUIType != FullScreenUIType.Unknown;
			bool flag = value.isPlayerTurn || value.isPreparationTurn;
			bool flag2 = value.isPreciseAttack || value.forceHotKeyPressed || UIUtilityCombat.NeedShowMiniInspect(base.ViewModel.UnitWrapper);
			bool flag3 = value.hover || value.isPreciseAttack;
			bool isVisible = !num && (value.isTBM || value.isPreciseAttack) && flag && flag2 && !value.isDeadOrUnconsciousIsDead && flag3;
			UpdateVisibility(isVisible, value.isPreciseAttack);
			m_SelectHintContainer.gameObject.SetActive(value.hasAbility && !value.isTarget);
		})
			.AddTo(this);
		base.ViewModel.HasCompareData.Subscribe(ShowCompareBlock).AddTo(this);
		UpdateVisibility(isVisible: false, isPreciseAttack: false);
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(InternalUpdate).AddTo(this);
		UnitInfoPart[] unitInfoParts = m_UnitInfoParts;
		for (int i = 0; i < unitInfoParts.Length; i++)
		{
			unitInfoParts[i].Bind(base.ViewModel);
		}
		base.ViewModel.IsDirtyContent.Subscribe(delegate
		{
			Observable.NextFrame().Subscribe(UpdatePartsContainerHeight);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.BodyPartChanged, delegate
		{
			if (m_IsVisible)
			{
				m_FadeAnimator.PlayAnimation(value: false, delegate
				{
					m_FadeAnimator.PlayAnimation(value: true);
				});
			}
		}).AddTo(this);
	}

	private void UpdateVisibility(bool isVisible, bool isPreciseAttack)
	{
		m_IsVisible = isVisible;
		Color color = (isPreciseAttack ? m_PreciseAttackBackroundColor : m_DefaultBackroundColor);
		m_Background.color = color;
		m_FadeAnimator.BlockRaycastPermanent(isPreciseAttack);
		m_FadeAnimator.PlayAnimation(isVisible, delegate
		{
			UpdateVisibilityComplete(isVisible);
		});
	}

	private void ShowCompareBlock(bool hasCompareData)
	{
		m_CompareBlockAnimator.PlayAnimation(hasCompareData);
	}

	private void UpdateVisibilityComplete(bool isVisible)
	{
		if (base.ViewModel?.Unit != null)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.ViewModel.Unit, (Action<IUnitInfoVisibilityUIHandler>)delegate(IUnitInfoVisibilityUIHandler h)
			{
				h.HandleUnitInfoVisibilityChange(isVisible);
			}, isCheckRuntime: true);
		}
	}

	private void InternalUpdate()
	{
		if (base.ViewModel == null)
		{
			PFLog.UI.Error(base.gameObject.name + ": ViewModel == null, but View are still not Destroyed");
			return;
		}
		using (ProfileScope.New("NewUnitInfoView.InternalUpdate"))
		{
			if (m_IsVisible)
			{
				float num = (base.ViewModel.IsPreciseAttack.CurrentValue ? m_PreciseAttackScale : m_DefaultScale);
				m_ScaleRoot.localScale = Vector3.one * num;
				if (base.ViewModel.IsPreciseAttack.CurrentValue)
				{
					m_Root.anchoredPosition = m_PreciseAttackPosition;
					m_RMBHintLabel.text = UIStrings.Instance.CommonTexts.CloseWindow.Text;
					return;
				}
				m_RMBHintLabel.text = UIStrings.Instance.ActionTexts.Inspect.Text;
				Vector3 currentValue = base.ViewModel.Position.CurrentValue;
				Vector2 anchoredPosition = WorldToAnchoredPosition(currentValue);
				m_Root.anchoredPosition = anchoredPosition;
			}
		}
	}

	private Vector2 WorldToAnchoredPosition(Vector3 worldPosition)
	{
		Vector3 normalizedPositionInCamera = UIUtilityRect.GetNormalizedPositionInCamera(worldPosition);
		Rect rect = ParentRectTransform.rect;
		Vector2 pivot = ParentRectTransform.pivot;
		Vector2 vector = default(Vector2);
		vector.x = m_AnchoredPositionOffset.x / m_ReferenceScreenSize.x * rect.width;
		vector.y = m_AnchoredPositionOffset.y / m_ReferenceScreenSize.y * rect.height;
		Vector2 vector2 = vector;
		vector = default(Vector2);
		vector.x = (normalizedPositionInCamera.x - pivot.x) * rect.width;
		vector.y = (normalizedPositionInCamera.y - pivot.y) * rect.height;
		return vector + vector2;
	}

	public void UpdatePartsContainerHeight()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_PartsRectTransform);
		float partsContentHeight = GetPartsContentHeight();
		m_ScaleRoot.sizeDelta = new Vector2(m_ScaleRoot.sizeDelta.x, partsContentHeight);
		m_PartsRectTransform.DOSizeDelta(new Vector2(m_PartsRectTransform.sizeDelta.x, partsContentHeight), 0.2f).OnComplete(delegate
		{
			base.ViewModel.SetDirtyContent(isDirty: false);
		}).SetEase(Ease.OutCubic)
			.SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
	}

	private float GetPartsContentHeight()
	{
		float num = 0f;
		foreach (RectTransform item in m_PartsRectTransform)
		{
			if (item.gameObject.activeSelf && item != m_PartsIgnoreLayout)
			{
				num += LayoutUtility.GetPreferredHeight(item);
			}
		}
		if ((bool)m_PartsVerticalLayoutGroup)
		{
			int partsActiveChildCount = GetPartsActiveChildCount();
			if (partsActiveChildCount > 1)
			{
				num += (float)(partsActiveChildCount - 1) * m_PartsVerticalLayoutGroup.spacing;
			}
			num += (float)(m_PartsVerticalLayoutGroup.padding.top + m_PartsVerticalLayoutGroup.padding.bottom);
		}
		return num;
	}

	private int GetPartsActiveChildCount()
	{
		int num = 0;
		foreach (Transform item in m_PartsRectTransform)
		{
			if (item.gameObject.activeSelf && item != m_PartsIgnoreLayout)
			{
				num++;
			}
		}
		return num;
	}

	private void Awake()
	{
		m_LMBHintLabel.text = UIStrings.Instance.CommonTexts.Select.Text;
	}
}
