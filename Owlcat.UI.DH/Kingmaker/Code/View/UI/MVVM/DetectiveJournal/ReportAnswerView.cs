using System.Collections.Generic;
using Code.View.UI.MVVM.Tooltip.Templates;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ReportAnswerView : SelectionGroupEntityView<ReportAnswerVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_ClueName;

	[FormerlySerializedAs("m_ClueNameParent")]
	[SerializeField]
	private OwlcatMultiButton m_ClueNameButton;

	[SerializeField]
	private TMP_Text m_AnswerDescription;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_NewStateSelectable;

	[SerializeField]
	private List<CanvasGroup> m_ClosedCanvasGroup = new List<CanvasGroup>();

	[SerializeField]
	private TextStyle m_TextStyle;

	protected override void OnBind()
	{
		m_AnswerDescription.text = UIUtilityDetective.GetAnswerDegreeDescription(base.ViewModel.Answer).Text;
		base.ViewModel.IsSelected.Subscribe(SetSelectedState).AddTo(this);
		m_ClueNameButton.gameObject.SetActive(base.ViewModel.Answer.RelatedItem?.Blueprint != null);
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.TooltipPlace = m_TooltipPlace;
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0.75f),
			new Vector2(1f, 0.5f),
			new Vector2(1f, 0.25f),
			new Vector2(1f, 0f)
		};
		TooltipConfig config = tooltipConfig;
		using (GameLogContext.Scope)
		{
			GameLogContext.CaseItem = base.ViewModel.Answer.RelatedItem?.Blueprint;
			GameLogContext.TextStyle = m_TextStyle;
			LocalizedString answerRelatedItemName = GetAnswerRelatedItemName();
			m_ClueNameButton.SetActiveLayer((answerRelatedItemName == null) ? 1 : 0);
			m_ClueName.text = answerRelatedItemName?.Text;
			m_ClueNameButton.SetTooltip(new TooltipTemplateDetective(base.ViewModel.Answer.RelatedItem?.Blueprint), config).AddTo(this);
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
		}
		m_Button.SetTooltip(new TooltipTemplateDetectiveAnswer(base.ViewModel.Answer), config).AddTo(this);
		m_ClueNameButton.OnLeftClickAsObservable().Subscribe(OnClick).AddTo(this);
		base.ViewModel.IsNew.Subscribe(delegate(bool value)
		{
			m_NewStateSelectable.SetActiveLayer(value ? 1 : 0);
		}).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(delegate
		{
			TooltipHelper.HideTooltip();
		}).AddTo(this);
		base.OnBind();
		bool caseIsClosed = base.ViewModel.BlueprintCase.IsClosed();
		m_ClueNameButton.Interactable = !caseIsClosed;
		m_Button.Interactable = !caseIsClosed;
		bool flag = base.ViewModel.BlueprintCase.IsClosed() && Game.Instance.DetectiveSystem.GetCaseAnswer(base.ViewModel.BlueprintCase)?.Answer == base.ViewModel.Answer;
		m_StateSelectable.SetActiveLayer(flag ? 1 : 0);
		m_ClosedCanvasGroup.ForEach(delegate(CanvasGroup cg)
		{
			cg.alpha = ((!caseIsClosed) ? 1 : 0);
		});
	}

	private void SetSelectedState(bool isSelected)
	{
		int activeLayer = (isSelected ? 1 : 0);
		m_Button.SetActiveLayer(activeLayer);
	}

	[CanBeNull]
	private LocalizedString GetAnswerRelatedItemName()
	{
		if (base.ViewModel.Answer.TryGetComponent<CaseAnswerUISettings>(out var component))
		{
			if (!component.HideCaseAnswerRelatedItem)
			{
				return component.GetAnswerFormat();
			}
			return null;
		}
		if (base.ViewModel.Answer.RelatedItem?.Blueprint is BlueprintClue)
		{
			return UIStrings.Instance.DetectiveJournal.AccusationLabel;
		}
		return UIStrings.Instance.DetectiveJournal.BasedOnConclusionPlainLabel;
	}
}
