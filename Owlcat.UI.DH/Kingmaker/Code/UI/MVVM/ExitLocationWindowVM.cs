using System;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ExitLocationWindowVM : ViewModel
{
	private readonly Action m_OnConfirm;

	private readonly Action m_OnDecline;

	public readonly string Header;

	public readonly string Description;

	public readonly string AdditionalInformation;

	public ExitLocationWindowVM(Action onConfirm, Action onDecline)
	{
		m_OnConfirm = onConfirm;
		m_OnDecline = onDecline;
		Header = UIStrings.Instance.ActionTexts.ExitArea;
		Description = UIStrings.Instance.LootWindow.ExitDescription;
		AdditionalInformation = UIStrings.Instance.LootWindow.CollectAllBeforeLeave;
	}

	protected override void OnDispose()
	{
		m_OnDecline?.Invoke();
	}

	public void Confirm()
	{
		m_OnConfirm?.Invoke();
		Decline();
	}

	public void Decline()
	{
		m_OnDecline?.Invoke();
	}
}
