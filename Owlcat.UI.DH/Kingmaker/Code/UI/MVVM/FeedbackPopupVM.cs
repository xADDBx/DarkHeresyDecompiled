using System;
using Kingmaker.Code.View.Bridge.Enums;
using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FeedbackPopupVM : ViewModel
{
	public readonly ObservableList<FeedbackPopupItemVM> Items = new ObservableList<FeedbackPopupItemVM>();

	private readonly Action m_CloseAction;

	public FeedbackPopupVM(Action closeAction)
	{
		m_CloseAction = closeAction;
		FeedbackPopupItem[] items = FeedbackPopupConfigLoader.Instance.Items;
		foreach (FeedbackPopupItem config in items)
		{
			Items.Add(new FeedbackPopupItemVM(config));
		}
	}

	protected override void OnDispose()
	{
		Items.ForEach(delegate(FeedbackPopupItemVM vm)
		{
			vm.Dispose();
		});
		Items.Clear();
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
	}
}
