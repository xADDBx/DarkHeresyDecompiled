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

public class DeductionOnScreenVM : ViewModel, IConclusionStatusChanged, ISubscriber, IClueAddendumStatusChanged, IConclusionsUpdateHandler
{
	public readonly BlueprintConclusion Conclusion;

	public readonly BlueprintConclusion.Source SelectedSource;

	private readonly ReactiveProperty<ConstructConclusionVM> m_ConstructConclusionVM = new ReactiveProperty<ConstructConclusionVM>();

	private readonly ReactiveProperty<bool> m_RefutedConclusion = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanMakeDeduction = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsNewConclusion = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_Refresh = new ReactiveCommand<Unit>();

	private readonly Action<BlueprintCaseItem> m_OpenConclusionSelection;

	public ReadOnlyReactiveProperty<ConstructConclusionVM> ConstructConclusionVM => m_ConstructConclusionVM;

	public ReadOnlyReactiveProperty<bool> RefutedConclusion => m_RefutedConclusion;

	public ReadOnlyReactiveProperty<bool> CanMakeDeduction => m_CanMakeDeduction;

	public ReadOnlyReactiveProperty<bool> IsNewConclusion => m_IsNewConclusion;

	public Observable<Unit> Refresh => m_Refresh;

	public DeductionOnScreenVM(BlueprintConclusion conclusion, BlueprintConclusion.Source selectedSource, Action<BlueprintCaseItem> onConclusionClick)
	{
		Conclusion = conclusion;
		SelectedSource = selectedSource;
		m_OpenConclusionSelection = onConclusionClick;
		UpdateDeductionInfo();
		UpdateFalsity();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Game.Instance.Player.UISettings.DetectiveSystemData.ExaminedDetectiveData.SelectedConclusions.AddExaminedEntityIfNeeded(Conclusion);
	}

	private void UpdateDeductionInfo()
	{
		DetectiveSystem detective = Game.Instance.DetectiveSystem;
		List<BlueprintConclusion> conclusionsFor = UIUtilityDetective.GetConclusionsFor(Conclusion);
		m_CanMakeDeduction.Value = conclusionsFor.Any((BlueprintConclusion c) => c.Sources.Any((BlueprintConclusion.Source s) => detective.HasItem(s.Item2))) && !conclusionsFor.Any((BlueprintConclusion c) => Game.Instance.DetectiveSystem.HasConclusion(c));
		m_ConstructConclusionVM.Value = (CanMakeDeduction.CurrentValue ? new ConstructConclusionVM(Conclusion, MakeDeduction, null) : null);
		List<BlueprintConclusion> conclusionsFor2 = UIUtilityDetective.GetConclusionsFor(SelectedSource.Item1);
		m_IsNewConclusion.Value = UIUtilityDetective.ExaminedDetectiveData.SelectedConclusions.IsEntityNew(Conclusion) || conclusionsFor2.Any(UIUtilityDetective.ExaminedDetectiveData.ExaminedConclusions.IsEntityNew);
		m_Refresh.Execute();
	}

	public void OnConclusionClick()
	{
		m_OpenConclusionSelection?.Invoke(SelectedSource.Item1);
		Game.Instance.Player.UISettings.DetectiveSystemData.ExaminedDetectiveData.SelectedConclusions.AddExaminedEntityIfNeeded(Conclusion);
		m_IsNewConclusion.Value = false;
	}

	private void MakeDeduction()
	{
		m_OpenConclusionSelection?.Invoke(Conclusion);
	}

	private void UpdateFalsity()
	{
		m_RefutedConclusion.Value = Conclusion.IsRefuted();
	}

	public void HandleConclusionStatusChanged(BlueprintConclusion blueprint)
	{
		UpdateDeductionInfo();
		UpdateFalsity();
	}

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		UpdateFalsity();
	}

	public void UpdateConclusions()
	{
		m_Refresh.Execute();
	}
}
