using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OwlcatDropdownVM : ViewModel
{
	public readonly IReadOnlyList<DropdownItemVM> VMCollection;

	private readonly ReactiveProperty<DropdownItemVM> m_SelectedVM = new ReactiveProperty<DropdownItemVM>();

	private readonly ReactiveProperty<int> m_Index = new ReactiveProperty<int>();

	public ReadOnlyReactiveProperty<DropdownItemVM> SelectedVM => m_SelectedVM;

	public ReadOnlyReactiveProperty<int> Index => m_Index;

	public OwlcatDropdownVM(IReadOnlyList<DropdownItemVM> vmCollection, DropdownItemVM selectedVM)
	{
		VMCollection = vmCollection;
		SetSelected(selectedVM);
	}

	public OwlcatDropdownVM(IReadOnlyList<DropdownItemVM> vmCollection, int index = 0)
	{
		VMCollection = vmCollection;
		SetIndex(index);
	}

	protected override void OnDispose()
	{
		VMCollection?.ForEach(delegate(DropdownItemVM viewModel)
		{
			viewModel.Dispose();
		});
	}

	public void SetSelected(DropdownItemVM viewModel)
	{
		if (VMCollection == null || !VMCollection.Contains(viewModel))
		{
			UberDebug.LogError("Selected ViewModel not contains in VMCollection");
			return;
		}
		m_SelectedVM.Value = viewModel;
		m_Index.Value = VMCollection.IndexOf(viewModel);
	}

	public void SetIndex(int index)
	{
		if (VMCollection == null || index >= VMCollection.Count || index < 0)
		{
			UberDebug.LogError("Index is out of range");
			return;
		}
		m_SelectedVM.Value = VMCollection[index];
		m_Index.Value = index;
	}
}
