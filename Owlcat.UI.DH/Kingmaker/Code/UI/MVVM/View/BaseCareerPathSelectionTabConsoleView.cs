using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BaseCareerPathSelectionTabConsoleView<TViewModel> : BaseCareerPathSelectionTabCommonView<TViewModel> where TViewModel : ViewModel
{
	protected readonly ReactiveProperty<bool> ButtonActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ButtonVisible = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		ButtonActive.And(IsTabActiveProp).Subscribe(delegate(bool value)
		{
			m_ButtonVisible.Value = value;
		}).AddTo(this);
	}

	public virtual void ScrollMainTab(float value)
	{
	}
}
