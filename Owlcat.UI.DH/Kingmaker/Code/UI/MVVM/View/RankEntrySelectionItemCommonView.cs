using System;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Common.Animations;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntrySelectionItemCommonView : VirtualListElementViewBase<RankEntrySelectionVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasNeighbours, IRankEntryElement
{
	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_CharInfoRankEntryView;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_MainButtonImage;

	[SerializeField]
	private RankEntrySelectionStateSprites[] m_StateSprites;

	[SerializeField]
	private bool m_IsListEntry = true;

	[SerializeField]
	[ShowIf("m_IsListEntry")]
	private TextMeshProUGUI m_SelectionLabel;

	[SerializeField]
	[HideIf("m_IsListEntry")]
	private GameObject m_SelectedMark;

	[SerializeField]
	[HideIf("m_IsListEntry")]
	private RectTransform m_NextItemArrow;

	[SerializeField]
	private RankEntryAnimator m_Highlighter;

	private readonly ReactiveProperty<string> m_HintText = new ReactiveProperty<string>();

	private IDisposable m_TooltipHandle;

	private List<IFloatConsoleNavigationEntity> m_Neighbours;

	private RectTransform m_TooltipPlace;

	public MonoBehaviour MonoBehaviour => this;

	public void SetViewParameters(RectTransform tooltipPlace)
	{
		m_TooltipPlace = tooltipPlace;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.EntryState.Subscribe(delegate
		{
			m_HintText.Value = base.ViewModel.GetHintText();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.HandleClick();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MainButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.HandleClick();
		}));
		AddDisposable(base.ViewModel.SelectedFeature.Subscribe(delegate(RankEntrySelectionFeatureVM featureVM)
		{
			m_CharInfoRankEntryView.Bind(featureVM);
			m_TooltipHandle?.Dispose();
			if (featureVM != null)
			{
				m_TooltipHandle = m_MainButton.SetTooltip(featureVM.Tooltip, new TooltipConfig
				{
					TooltipPlace = m_TooltipPlace,
					PriorityPivots = new List<Vector2>
					{
						new Vector2(1f, 0.5f)
					}
				});
			}
			AddDisposable(m_MainButton.SetHint(m_HintText));
			if (m_SelectionLabel != null)
			{
				string text = UIUtilityEncyclopedy.GetGlossaryEntryName(base.ViewModel.GlossaryEntryKey);
				if (featureVM is RankEntrySelectionStatVM rankEntrySelectionStatVM)
				{
					text = rankEntrySelectionStatVM.StatDisplayName;
				}
				m_SelectionLabel.text = text;
			}
		}));
		if (m_SelectedMark != null)
		{
			AddDisposable(base.ViewModel.IsCurrentRankEntryItem.Subscribe(OnSelectedChanged));
		}
		AddDisposable(base.ViewModel.EntryState.Subscribe(UpdateState));
	}

	public void OnSelectedChanged(bool value)
	{
		m_SelectedMark.SetActive(value);
	}

	protected override void DestroyViewImplementation()
	{
		m_TooltipHandle?.Dispose();
		m_TooltipHandle = null;
	}

	private void UpdateState(RankEntryState entryState)
	{
		RankEntrySelectionStateSprites rankEntrySelectionStateSprites = m_StateSprites.FirstOrDefault((RankEntrySelectionStateSprites p) => p.FeatureGroup == base.ViewModel.FeatureGroup);
		if (rankEntrySelectionStateSprites == null)
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		else if (entryState == RankEntryState.NotSelectable || entryState == RankEntryState.FirstSelectable || entryState == RankEntryState.WaitPreviousToSelect || entryState == RankEntryState.Selectable)
		{
			m_MainButtonImage.sprite = rankEntrySelectionStateSprites.Icon;
			m_MainButtonImage.gameObject.SetActive(value: true);
		}
		else
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		m_MainButton.SetActiveLayer(entryState.ToString());
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return m_Neighbours;
	}

	public void SetNeighbours(List<IFloatConsoleNavigationEntity> entities)
	{
		m_Neighbours = entities;
	}

	public bool CanConfirmClick()
	{
		return m_MainButton.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		m_MainButton.OnConfirmClick();
		EventBus.RaiseEvent(delegate(IRankEntryConfirmClickHandler h)
		{
			h.OnRankEntryConfirmClick();
		});
	}

	public string GetConfirmClickHint()
	{
		if (m_IsListEntry)
		{
			return base.ViewModel.SelectionMade ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
		}
		return (base.ViewModel.IsCurrentRankEntryItem.CurrentValue == (bool)this) ? UIStrings.Instance.CommonTexts.Expand : UIStrings.Instance.CommonTexts.Select;
	}

	public void SetRotation(float angleDeg, bool hasArrow)
	{
		base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - angleDeg);
		if ((bool)m_NextItemArrow)
		{
			m_NextItemArrow.gameObject.SetActive(hasArrow);
			float num = GetComponent<RectTransform>().sizeDelta.x * 0.5f;
			float num2 = (90f + angleDeg) * (MathF.PI / 180f);
			m_NextItemArrow.anchoredPosition = new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * num;
			m_NextItemArrow.localRotation = Quaternion.Euler(0f, 0f, num2 * 57.29578f);
		}
	}

	public void StartHighlight(string key)
	{
		if (base.ViewModel.ContainsFeature(key))
		{
			m_Highlighter.Or(null)?.StartAnimation();
		}
	}

	public void StopHighlight()
	{
		m_Highlighter.Or(null)?.StopAnimation();
	}
}
