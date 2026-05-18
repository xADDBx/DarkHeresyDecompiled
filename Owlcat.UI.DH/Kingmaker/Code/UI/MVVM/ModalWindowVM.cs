using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ModalWindowVM : ViewModel
{
	private readonly ReactiveCommand<(int index, bool enable)> m_ButtonInteractableStateChanged;

	public readonly string Header;

	[CanBeNull]
	public readonly string Description;

	public readonly IReadOnlyList<ModalWindowAction> Actions;

	public Observable<(int index, bool enable)> ButtonInteractableStateChanged => m_ButtonInteractableStateChanged;

	public ModalWindowVM(string header, string description = null, IReadOnlyList<ModalWindowAction> actions = null)
	{
		Header = header;
		Description = description;
		Actions = actions;
		m_ButtonInteractableStateChanged = new ReactiveCommand<(int, bool)>().AddTo(this);
	}

	public void SetButtonEnabled(int index, bool enabled)
	{
		if (Actions != null && index >= 0 && index < Actions.Count)
		{
			m_ButtonInteractableStateChanged.Execute((index, enabled));
		}
	}
}
