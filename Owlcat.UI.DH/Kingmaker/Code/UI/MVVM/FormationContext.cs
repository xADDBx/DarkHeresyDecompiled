using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FormationContext : ViewModel, IFormationWindowUIHandler, ISubscriber
{
	private readonly ReactiveProperty<FormationVM> m_FormationVM = new ReactiveProperty<FormationVM>();

	public bool IsFormationActive => m_FormationVM.CurrentValue != null;

	public FormationContext(ReactiveProperty<FormationVM> formationVM)
	{
		m_FormationVM = formationVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleOpenFormation()
	{
		if (m_FormationVM.CurrentValue == null)
		{
			m_FormationVM.Value = new FormationVM(DisposeFormation);
		}
	}

	public void HandleCloseFormation()
	{
		if (m_FormationVM.CurrentValue != null)
		{
			DisposeFormation();
		}
	}

	private void DisposeFormation()
	{
		m_FormationVM.CurrentValue?.Dispose();
		m_FormationVM.Value = null;
	}
}
