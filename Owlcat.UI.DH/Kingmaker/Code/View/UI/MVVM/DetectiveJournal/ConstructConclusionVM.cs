using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConstructConclusionVM : ViewModel, IConclusionsUpdateHandler, ISubscriber
{
	public readonly BlueprintCaseItem CaseItemFrom;

	public readonly bool IsRefuted;

	private readonly ReactiveProperty<bool> m_HasNewConclusion = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_NewConclusionsCount = new ReactiveProperty<int>();

	private readonly Action m_OnClick;

	private readonly Action<BlueprintCaseItem> m_OnHover;

	public ReadOnlyReactiveProperty<bool> HasNewConclusion => m_HasNewConclusion;

	public ReadOnlyReactiveProperty<int> NewConclusionsCount => m_NewConclusionsCount;

	public ConstructConclusionVM(BlueprintCaseItem caseItemFrom, Action onClick, Action<BlueprintCaseItem> onHover)
	{
		CaseItemFrom = caseItemFrom;
		m_OnClick = onClick;
		m_OnHover = onHover;
		List<BlueprintConclusion> conclusionsFor = UIUtilityDetective.GetConclusionsFor(caseItemFrom);
		IsRefuted = conclusionsFor.All((BlueprintConclusion c) => c.IsRefuted());
		UpdateConclusions();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnHover()
	{
		m_OnHover?.Invoke(CaseItemFrom);
	}

	public void OnHoverEnd()
	{
		m_OnHover?.Invoke(null);
	}

	public void OnClick()
	{
		m_OnClick?.Invoke();
	}

	public void UpdateConclusions()
	{
		List<BlueprintConclusion> conclusionsFor = UIUtilityDetective.GetConclusionsFor(CaseItemFrom);
		int num = conclusionsFor.Count(UIUtilityDetective.ExaminedDetectiveData.ExaminedConclusions.IsEntityNew);
		m_HasNewConclusion.Value = num > 0;
		m_NewConclusionsCount.Value = ((conclusionsFor.Count != num) ? num : 0);
	}
}
