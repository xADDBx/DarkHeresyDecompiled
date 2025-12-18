using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhasePantographItemPCView : CharGenAttributesPhasePantographItemView
{
	[SerializeField]
	private OwlcatMultiButton m_MinusButton;

	[SerializeField]
	private OwlcatMultiButton m_PlusButton;

	private readonly ReactiveProperty<string> m_PlusButtonHint = new ReactiveProperty<string>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CheckCoopButtons(base.ViewModel.IsMainCharacter.CurrentValue);
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_MinusButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.RetreatStat();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_PlusButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.AdvanceStat();
		}));
		AddDisposable(base.ViewModel.CanRetreat.Subscribe(m_MinusButton.SetInteractable));
		AddDisposable(m_PlusButton.SetHint(m_PlusButtonHint));
		AddDisposable(base.ViewModel.CanAdvance.Subscribe(delegate(bool value)
		{
			m_PlusButton.SetInteractable(value);
			m_PlusButtonHint.Value = (value ? string.Empty : ((string)UIStrings.Instance.CharGen.CannotAdvanceStatHint));
		}));
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons));
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_MinusButton.gameObject.SetActive(isMainCharacter);
		m_PlusButton.gameObject.SetActive(isMainCharacter);
	}
}
