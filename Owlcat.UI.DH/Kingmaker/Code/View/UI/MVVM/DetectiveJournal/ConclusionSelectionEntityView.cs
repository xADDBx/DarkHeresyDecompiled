using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionSelectionEntityView : SelectionGroupEntityView<ConclusionSelectionEntityVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_ConclusionText;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_ViewedStateSelectable;

	[Header("Values")]
	[SerializeField]
	private string m_TooltipBodyFormat = "<i><b>{0}</i></b><line-height=120%>\n</line-height>";

	[field: SerializeField]
	public LineDirectionData LineFrom { get; private set; }

	[field: SerializeField]
	public LineDirectionData LineTo { get; private set; }

	public OwlcatMultiButton Button => m_Button;

	protected override void OnBind()
	{
		base.OnBind();
		m_ConclusionText.text = ((base.ViewModel.Conclusion == null) ? ((string)UIStrings.Instance.DetectiveJournal.NoConclusionSelected) : base.ViewModel.Conclusion.Description.Text);
		bool flag = base.ViewModel.Conclusion?.IsRefuted() ?? false;
		BlueprintConclusion conclusion = base.ViewModel.Conclusion;
		if (conclusion != null && conclusion.IsRefuted())
		{
			string text = UIStrings.Instance.DetectiveJournal.RefutedLabel.Text;
			string text2 = string.Format(m_TooltipBodyFormat, UIStrings.Instance.DetectiveJournal.RefutedDescription.Text);
			string text3 = UIUtilityDetective.GetRefutedText(base.ViewModel.Conclusion).TrimEnd(' ', '\n');
			string description = text2 + "\"" + text3 + "\"";
			TooltipConfig tooltipConfig = default(TooltipConfig);
			tooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.75f),
				new Vector2(0f, 0.25f),
				new Vector2(0f, 1f),
				new Vector2(0f, 0f)
			};
			TooltipConfig config = tooltipConfig;
			m_Button.SetTooltip(new TooltipTemplateSimple(text, description), config).AddTo(this);
		}
		m_StateSelectable.SetActiveLayer(flag ? 1 : 0);
		base.ViewModel.IsViewed.Subscribe(delegate(bool value)
		{
			m_ViewedStateSelectable.SetActiveLayer((!value) ? 1 : 0);
		}).AddTo(this);
	}
}
