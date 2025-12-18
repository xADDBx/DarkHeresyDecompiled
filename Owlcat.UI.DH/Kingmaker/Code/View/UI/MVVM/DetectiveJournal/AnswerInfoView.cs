using System.Collections.Generic;
using Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnswerInfoView : View<BlueprintCaseAnswer>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private OwlcatMultiSelectable m_SelectedSelectable;

	protected override void OnBind()
	{
		m_Description.text = UIUtilityDetective.GetAnswerDegreeDescription(base.ViewModel).Text;
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.TooltipPlace = m_TooltipPlace;
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0f, 0.5f),
			new Vector2(0f, 0.75f),
			new Vector2(0f, 0.25f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		TooltipConfig config = tooltipConfig;
		m_Description.SetTooltip(new TooltipTemplateDetectiveAnswer(base.ViewModel), config).AddTo(this);
		int activeLayer = 0;
		BlueprintCase blueprintCase = base.ViewModel.RelatedItem?.Blueprint.ParentCase.Blueprint;
		if (blueprintCase != null && blueprintCase.IsClosed() && Game.Instance.DetectiveSystem.GetCaseAnswer(blueprintCase)?.Answer == base.ViewModel)
		{
			activeLayer = 1;
		}
		m_SelectedSelectable.SetActiveLayer(activeLayer);
	}
}
