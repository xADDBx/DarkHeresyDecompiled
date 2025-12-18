using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class FirstLaunchEntityLanguageItemBaseView : View<FirstLaunchEntityLanguageItemVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	public IConsoleEntity ConsoleEntityProxy => m_Button;

	public bool IsSelected => base.ViewModel.IsSelected.CurrentValue;

	public OwlcatMultiButton EntityButton => m_Button;

	protected override void OnBind()
	{
		m_Title.text = base.ViewModel.Title;
		base.ViewModel.IsSelected.Subscribe(SetValueFromSettings).AddTo(this);
		m_Button.OnLeftClick.AsObservable().Subscribe(base.ViewModel.SetSelected).AddTo(this);
		m_Button.OnConfirmClickAsObservable().Subscribe(base.ViewModel.SetSelected).AddTo(this);
	}

	private void SetValueFromSettings(bool value)
	{
		m_Button.SetActiveLayer(value ? "On" : "Off");
	}
}
