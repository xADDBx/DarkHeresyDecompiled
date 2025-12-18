using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSummaryBaseView : CharInfoComponentView<CharInfoSummaryVM>
{
	[Header("Views")]
	[SerializeField]
	protected CharInfoStatusEffectsView m_StatusEffectsView;

	[Header("Move Points")]
	[SerializeField]
	protected OwlcatMultiButton m_MovePointsButton;

	[SerializeField]
	private TextMeshProUGUI m_MovePointsLabel;

	[SerializeField]
	private TextMeshProUGUI m_MovePoints;

	[Header("Action Points")]
	[SerializeField]
	protected OwlcatMultiButton m_ActionPointsButton;

	[SerializeField]
	private TextMeshProUGUI m_ActionPointsLabel;

	[SerializeField]
	private TextMeshProUGUI m_ActionPoints;

	private AccessibilityTextHelper m_TextHelper;

	public override void Initialize()
	{
		base.Initialize();
		m_StatusEffectsView.Or(null)?.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_MovePointsLabel, m_MovePoints, m_ActionPointsLabel, m_ActionPoints);
	}

	protected override void OnBind()
	{
		base.ViewModel.ActionPointVM.Subscribe(delegate
		{
			UpdatePoints();
		}).AddTo(this);
		m_StatusEffectsView.Or(null)?.Bind(base.ViewModel.StatusEffects.CurrentValue);
		base.OnBind();
		SetupTexts();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_StatusEffectsView.Or(null)?.Unbind();
		m_TextHelper.Dispose();
		base.gameObject.SetActive(value: false);
	}

	private void UpdatePoints()
	{
		ActionPointsVM currentValue = base.ViewModel.ActionPointVM.CurrentValue;
		if (currentValue != null)
		{
			float num = (base.ViewModel.IsInCombat.CurrentValue ? currentValue.CurrentMP.CurrentValue : currentValue.MaxMP.CurrentValue);
			float num2 = (base.ViewModel.IsInCombat.CurrentValue ? currentValue.CurrentAP.CurrentValue : currentValue.MaxAP.CurrentValue);
			m_MovePoints.text = $"{num}";
			m_ActionPoints.text = $"{num2}";
		}
	}

	private void SetupTexts()
	{
		m_MovePointsLabel.text = UIStrings.Instance.ActionBar.MovementPoints;
		m_ActionPointsLabel.text = UIStrings.Instance.ActionBar.ActionPoints;
		m_TextHelper.UpdateTextSize();
	}
}
