using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCombatTextBlockView : View<OvertipCombatTextBlockVM>
{
	[Header("Common Combat Texts")]
	[SerializeField]
	private FadeAnimator m_CombatTextContainerAnimator;

	[SerializeField]
	private CombatTextCommonCreator m_CombatTextCommonCreator;

	[SerializeField]
	private List<RectTransform> m_PartRects;

	[SerializeField]
	private float m_BottomPadding = 5f;

	[Header("Hit points Combat Texts")]
	[SerializeField]
	private CombatTextHitPointsCreator m_CombatTextHitPointsCreator;

	[Header("Morale Combat Texts")]
	[SerializeField]
	private FadeAnimator m_CombatTextMoraleContainerAnimator;

	[SerializeField]
	private CombatTextMoraleCreator m_CombatTextMoraleCreator;

	[SerializeField]
	private CanvasGroup[] m_fadeOnAwake;

	private List<CanvasGroup> m_PartCanvasGroups;

	private bool m_IsInitialize;

	private IDisposable m_MessageDelay;

	private bool m_IsCommonActive;

	private bool m_IsMoraleActive;

	public void UpdateVisualForCommon()
	{
		UpdateVisualInternal();
	}

	public void HideInstant()
	{
		m_CombatTextMoraleContainerAnimator.DisappearInstant();
		m_CombatTextContainerAnimator.DisappearInstant();
	}

	protected override void OnBind()
	{
		m_CombatTextCommonCreator.SetCallbacks(HandleMessageViewDisposed, UpdateVisualInternal);
		m_CombatTextHitPointsCreator.SetCallbacks(HandleMessageViewDisposed, UpdateVisualInternal);
		m_CombatTextMoraleCreator.SetCallbacks(HandleMessageViewDisposed, UpdateVisualInternal);
		m_PartCanvasGroups = m_PartRects.Select((RectTransform r) => r.GetComponent<CanvasGroup>()).ToList();
		base.ViewModel.CombatMessageEnqueued.Subscribe(OnCombatMessageEnqueued).AddTo(this);
		m_CombatTextCommonCreator.Clear();
		m_CombatTextHitPointsCreator.Clear();
		m_CombatTextMoraleCreator.Clear();
		base.ViewModel.IsForceHidden.Subscribe(delegate
		{
			UpdateVisualInternal();
		}).AddTo(this);
		UpdateVisualInternal();
	}

	protected override void OnUnbind()
	{
		base.ViewModel.ClearAllMessages();
		m_CombatTextCommonCreator.Clear();
		m_CombatTextHitPointsCreator.Clear();
		m_CombatTextMoraleCreator.Clear();
		m_MessageDelay?.Dispose();
		m_MessageDelay = null;
	}

	private void HandleMessageViewDisposed(CombatMessageBase message)
	{
		base.ViewModel.ClearMessage(message);
	}

	private void OnCombatMessageEnqueued(Unit unit)
	{
		if (base.ViewModel.IsForceHidden.CurrentValue)
		{
			base.ViewModel.ClearAllMessages();
			return;
		}
		m_MessageDelay?.Dispose();
		m_MessageDelay = Observable.Timer(0.1f.Seconds()).Subscribe(ShowCombatMessage);
	}

	private void ShowCombatMessage()
	{
		bool single = base.ViewModel.MessagesCount == 1;
		CombatMessageBase message;
		while (base.ViewModel.GetNextMessage(out message))
		{
			AddCombatText(message, single);
		}
		UpdateVisualInternal();
	}

	private void UpdateVisualInternal()
	{
		bool currentValue = base.ViewModel.IsForceHidden.CurrentValue;
		bool flag = !currentValue && m_CombatTextCommonCreator.ActiveViews.Count > 0;
		bool flag2 = !currentValue && m_CombatTextMoraleCreator.ActiveViews.Count > 0;
		if (flag2 && !m_IsMoraleActive)
		{
			m_CombatTextMoraleContainerAnimator.AppearAnimation();
			m_IsMoraleActive = true;
		}
		else if (!flag2 && m_IsMoraleActive)
		{
			m_CombatTextMoraleContainerAnimator.DisappearAnimation();
			m_IsMoraleActive = false;
		}
		if (flag && !m_IsCommonActive)
		{
			m_CombatTextContainerAnimator.AppearAnimation();
			m_IsCommonActive = true;
		}
		else if (!flag && m_IsCommonActive)
		{
			m_CombatTextContainerAnimator.DisappearAnimation();
			m_IsCommonActive = false;
			m_CombatTextCommonCreator.ContainerRect.sizeDelta = Vector2.zero;
			return;
		}
		if (m_PartRects.Count == 0)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < m_PartRects.Count; i++)
		{
			RectTransform rectTransform = m_PartRects[i];
			if (m_PartCanvasGroups[i].alpha > 0f && rectTransform.gameObject.activeInHierarchy)
			{
				num = Mathf.Max(RectTransformUtility.CalculateRelativeRectTransformBounds(m_CombatTextCommonCreator.ContainerRect.parent.transform, rectTransform).max.y, num);
			}
		}
		m_CombatTextCommonCreator.ContainerRect.anchoredPosition = new Vector2(m_CombatTextCommonCreator.ContainerRect.anchoredPosition.x, num + m_BottomPadding);
		Vector2 sizeDelta = default(Vector2);
		for (int j = 0; j < m_CombatTextCommonCreator.ActiveViews.Count; j++)
		{
			CombatTextCommonView combatTextCommonView = m_CombatTextCommonCreator.ActiveViews[j];
			sizeDelta = new Vector2(Mathf.Max(sizeDelta.x, combatTextCommonView.Size.x), sizeDelta.y + combatTextCommonView.Size.y);
			combatTextCommonView.Rect.anchoredPosition = new Vector2(combatTextCommonView.XPos, (float)j * combatTextCommonView.Rect.rect.height);
		}
		m_CombatTextCommonCreator.ContainerRect.sizeDelta = sizeDelta;
	}

	private void AddCombatText(CombatMessageBase message, bool single, bool even = true)
	{
		if (!(message is CombatMessageDamage combatMessageDamage))
		{
			if (!(message is CombatMessageAttackMiss combatMessageAttackMiss))
			{
				if (!(message is CombatMessageHealing))
				{
					if (message is CombatMessageMorale { Amount: not 0 } combatMessageMorale)
					{
						m_CombatTextMoraleCreator.Create(combatMessageMorale);
					}
					else
					{
						m_CombatTextCommonCreator.Create(message);
					}
				}
				else
				{
					CombatTextHitPointsView combatTextHitPointsView = m_CombatTextHitPointsCreator.Create(message);
					combatTextHitPointsView.SetDirection(UIUtilityRect.GetNormalizedPositionInCamera(Vector3.zero) - UIUtilityRect.GetNormalizedPositionInCamera(Vector3.zero), single: false, even: false);
					combatTextHitPointsView.PlayAnimation();
				}
			}
			else if (combatMessageAttackMiss.Result == AttackResult.Miss || combatMessageAttackMiss.Result == AttackResult.Defended)
			{
				CombatTextHitPointsView combatTextHitPointsView2 = m_CombatTextHitPointsCreator.Create(message);
				combatTextHitPointsView2.SetDirection(UIUtilityRect.GetNormalizedPositionInCamera(combatMessageAttackMiss.TargetPosition) - UIUtilityRect.GetNormalizedPositionInCamera(combatMessageAttackMiss.SourcePosition), single, even);
				combatTextHitPointsView2.PlayAnimation();
			}
			else
			{
				m_CombatTextCommonCreator.Create(message);
			}
		}
		else
		{
			CombatTextHitPointsView combatTextHitPointsView3 = m_CombatTextHitPointsCreator.Create(message);
			combatTextHitPointsView3.SetDirection(UIUtilityRect.GetNormalizedPositionInCamera(combatMessageDamage.TargetPosition) - UIUtilityRect.GetNormalizedPositionInCamera(combatMessageDamage.SourcePosition), single, even);
			combatTextHitPointsView3.PlayAnimation();
		}
	}

	private void Awake()
	{
		CanvasGroup[] fadeOnAwake = m_fadeOnAwake;
		for (int i = 0; i < fadeOnAwake.Length; i++)
		{
			fadeOnAwake[i].alpha = 0f;
		}
	}
}
