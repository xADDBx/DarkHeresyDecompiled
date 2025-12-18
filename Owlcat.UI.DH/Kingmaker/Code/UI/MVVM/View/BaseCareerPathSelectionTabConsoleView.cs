using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BaseCareerPathSelectionTabConsoleView<TViewModel> : BaseCareerPathSelectionTabCommonView<TViewModel>, ICareerPathSelectionTabConsoleView, ICareerPathSelectionTabView where TViewModel : ViewModel
{
	protected readonly ReactiveProperty<bool> ButtonActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ButtonVisible = new ReactiveProperty<bool>();

	private ConsoleHintDescription m_HintDescription;

	protected bool InputAdded;

	protected override void OnBind()
	{
		base.OnBind();
		NextButtonLabel.Subscribe(delegate(string value)
		{
			m_HintDescription?.SetLabel(value);
		}).AddTo(this);
		ButtonActive.And(IsTabActiveProp).Subscribe(delegate(bool value)
		{
			m_ButtonVisible.Value = value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		InputAdded = false;
	}

	public virtual void ScrollMainTab(float value)
	{
	}

	public virtual void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
	}
}
