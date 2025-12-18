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

	private List<CanvasGroup> m_PartCanvasGroups;

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

	private bool m_HasActiveCommonCombatText;

	private bool m_HasActiveMoraleCombatText;

	private bool m_IsInitialize;

	private readonly ReactiveCommand<Unit> m_UpdateVisual = new ReactiveCommand<Unit>();

	private List<CombatMessageBase> m_MessagesList = new List<CombatMessageBase>();

	private IDisposable m_MessageDelay;

	private bool m_IsCommonActive;

	private bool m_IsMoraleActive;

	public bool HasCombatTextMessages
	{
		get
		{
			if (!m_HasActiveCommonCombatText)
			{
				return m_HasActiveMoraleCombatText;
			}
			return true;
		}
	}

	public void UpdateVisualForCommon()
	{
		m_UpdateVisual.Execute();
	}

	public void HideInstant()
	{
		m_CombatTextMoraleContainerAnimator.DisappearInstant();
		m_CombatTextContainerAnimator.DisappearInstant();
	}

	protected override void OnBind()
	{
		m_UpdateVisual.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(UpdateVisualInternal).AddTo(this);
		m_CombatTextCommonCreator.SetCallback(delegate
		{
			m_UpdateVisual.Execute();
		});
		m_CombatTextHitPointsCreator.SetCallback(delegate
		{
			m_UpdateVisual.Execute();
		});
		m_CombatTextMoraleCreator.SetCallback(delegate
		{
			m_UpdateVisual.Execute();
		});
		m_PartCanvasGroups = m_PartRects.Select((RectTransform r) => r.GetComponent<CanvasGroup>()).ToList();
		base.ViewModel.CombatMessage.Subscribe(OnCombatMessage).AddTo(this);
		m_CombatTextCommonCreator.Clear();
		m_CombatTextHitPointsCreator.Clear();
		m_CombatTextMoraleCreator.Clear();
		m_UpdateVisual.Execute();
	}

	protected override void OnUnbind()
	{
		m_CombatTextCommonCreator.Clear();
		m_CombatTextHitPointsCreator.Clear();
		m_CombatTextMoraleCreator.Clear();
		UpdateHasActiveCombatText();
		m_MessageDelay?.Dispose();
		m_MessageDelay = null;
	}

	private void OnCombatMessage(CombatMessageBase message)
	{
		if (!base.ViewModel.IsForbiddenToShow)
		{
			m_MessageDelay?.Dispose();
			m_MessagesList.Add(message);
			m_MessageDelay = Observable.Timer(0.1f.Seconds()).Subscribe(AddCombatMessage);
		}
	}

	private void AddCombatMessage()
	{
		if (m_MessagesList.Count == 1)
		{
			AddCombatText(m_MessagesList.First(), single: true);
		}
		else
		{
			for (int i = 0; i < m_MessagesList.Count; i++)
			{
				AddCombatText(m_MessagesList[i], single: false, i % 2 > 0);
			}
		}
		m_MessagesList.Clear();
		m_UpdateVisual.Execute();
	}

	private void UpdateVisualInternal()
	{
		UpdateHasActiveCombatText();
		if (m_HasActiveMoraleCombatText && !m_IsMoraleActive)
		{
			m_CombatTextMoraleContainerAnimator.AppearAnimation();
			m_IsMoraleActive = true;
		}
		else if (!m_HasActiveMoraleCombatText && m_IsMoraleActive)
		{
			m_CombatTextMoraleContainerAnimator.DisappearAnimation();
			m_IsMoraleActive = false;
		}
		if (m_HasActiveCommonCombatText && !m_IsCommonActive)
		{
			m_CombatTextContainerAnimator.AppearAnimation();
			m_IsCommonActive = true;
		}
		else if (!m_HasActiveCommonCombatText && m_IsCommonActive)
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
			CombatTextEntityBaseView<CombatMessageBase> combatTextEntityBaseView = m_CombatTextCommonCreator.ActiveViews[j];
			sizeDelta = new Vector2(Mathf.Max(sizeDelta.x, combatTextEntityBaseView.Size.x), sizeDelta.y + combatTextEntityBaseView.Size.y);
			combatTextEntityBaseView.Rect.anchoredPosition = new Vector2(combatTextEntityBaseView.XPos, (float)j * combatTextEntityBaseView.Rect.rect.height);
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
		UpdateHasActiveCombatText();
	}

	private void UpdateHasActiveCombatText()
	{
		m_HasActiveCommonCombatText = m_CombatTextCommonCreator.ActiveViews.Count > 0;
		m_HasActiveMoraleCombatText = m_CombatTextMoraleCreator.ActiveViews.Count > 0;
		int activeMessagesCount = m_CombatTextCommonCreator.ActiveViews.Count + m_CombatTextMoraleCreator.ActiveViews.Count + m_CombatTextHitPointsCreator.ActiveViews.Count;
		base.ViewModel.SetActiveMessagesCount(activeMessagesCount);
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
