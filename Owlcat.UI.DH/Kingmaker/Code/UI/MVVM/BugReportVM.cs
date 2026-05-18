using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BugReportVM : ViewModel
{
	private readonly ReactiveProperty<BugReportDrawingVM> m_BugReportDrawingVM = new ReactiveProperty<BugReportDrawingVM>();

	private readonly ReactiveProperty<BugReportDuplicatesVM> m_BugReportDuplicatesVM = new ReactiveProperty<BugReportDuplicatesVM>();

	private readonly (string Description, List<(string Aspect, int AspectIndex, string Assignee)> Assignees)[] m_AllContextData;

	public readonly OwlcatDropdownVM ContextDropdownVM;

	private OwlcatDropdownVM m_aspectDropdownVM;

	private OwlcatDropdownVM m_AssigneeDropdownVM;

	private OwlcatDropdownVM m_FixVersionDropdownVM;

	private OwlcatDropdownVM m_ManualSaveDropdownVM;

	private OwlcatDropdownVM m_devPriorityDropdownVM;

	private Dictionary<(int, int), int> m_ContextAspectToAssigneeMap = new Dictionary<(int, int), int>();

	public ReadOnlyReactiveProperty<BugReportDrawingVM> BugReportDrawingVM => m_BugReportDrawingVM;

	public ReadOnlyReactiveProperty<BugReportDuplicatesVM> BugReportDuplicatesVM => m_BugReportDuplicatesVM;

	public BugReportVM()
	{
		m_AllContextData = ReportingUtils.Instance.GetContextDescriptions();
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		(string, List<(string, int, string)>)[] allContextData = m_AllContextData;
		for (int i = 0; i < allContextData.Length; i++)
		{
			(string, List<(string, int, string)>) tuple = allContextData[i];
			list.Add(new DropdownItemVM(tuple.Item1));
		}
		ContextDropdownVM = new OwlcatDropdownVM(list).AddTo(this);
		Metrics.Interface.State(InterfaceMetricsEvent.InterfaceStates.Open).Type(InterfaceMetricsEvent.InterfaceTypes.BugReport).Send();
	}

	public OwlcatDropdownVM GetAspectDropDownVM()
	{
		return new OwlcatDropdownVM((from aspect in ReportingUtils.Instance.GetCurrentAspects()
			select new DropdownItemVM(aspect)).ToList());
	}

	public OwlcatDropdownVM GetAssigneeDropDownVM(int contextIndex)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (var item in m_AllContextData[contextIndex].Assignees)
		{
			dictionary.TryAdd(item.Assignee, dictionary.Count);
			m_ContextAspectToAssigneeMap[(contextIndex, item.AspectIndex)] = dictionary[item.Assignee];
		}
		return m_AssigneeDropdownVM = new OwlcatDropdownVM(dictionary.Select((KeyValuePair<string, int> x) => new DropdownItemVM(x.Key)).ToList());
	}

	public int GetAssigneeIndex(int contextIndex, int aspectIndex)
	{
		return CollectionExtensions.GetValueOrDefault(key: (contextIndex, aspectIndex), dictionary: m_ContextAspectToAssigneeMap, defaultValue: 0);
	}

	public OwlcatDropdownVM GetFixVersionDropDownVM()
	{
		List<DropdownItemVM> vmCollection = ReportingUtils.Instance.ReportOptions.Jira.FixVersions.Select((string fixVersion) => new DropdownItemVM(fixVersion)).ToList();
		return m_FixVersionDropdownVM = new OwlcatDropdownVM(vmCollection);
	}

	public OwlcatDropdownVM GetManualSaveDropDownVM()
	{
		List<DropdownItemVM> manualSaveVMCollection = new List<DropdownItemVM>
		{
			new DropdownItemVM("-")
		};
		ReportingUtils.Instance.InitializeManualSaves(delegate(SaveInfo saveInfo)
		{
			manualSaveVMCollection.Add(new DropdownItemVM(saveInfo.Name));
		});
		return m_ManualSaveDropdownVM = new OwlcatDropdownVM(manualSaveVMCollection);
	}

	public int GetDefaultDevPriorityIndex()
	{
		return 3;
	}

	public OwlcatDropdownVM GetPriorityDropDownVM()
	{
		return m_devPriorityDropdownVM = new OwlcatDropdownVM(new DropdownItemVM[5]
		{
			new DropdownItemVM("Blocker"),
			new DropdownItemVM("Crit"),
			new DropdownItemVM("Major"),
			new DropdownItemVM("Normal"),
			new DropdownItemVM("Minor")
		});
	}

	public void ShowDrawing()
	{
		m_BugReportDrawingVM.Value = new BugReportDrawingVM(HideDrawing);
	}

	private void HideDrawing()
	{
		m_BugReportDrawingVM.Value?.Dispose();
	}

	public void ShowDuplicates()
	{
		m_BugReportDuplicatesVM.Value = new BugReportDuplicatesVM(HideDuplicates, ReportingUtils.Instance.CurrentContextName);
	}

	private void HideDuplicates()
	{
		m_BugReportDuplicatesVM.Value?.Dispose();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		HideDrawing();
		HideDuplicates();
		m_AssigneeDropdownVM?.Dispose();
		m_FixVersionDropdownVM?.Dispose();
		m_ManualSaveDropdownVM?.Dispose();
		m_devPriorityDropdownVM?.Dispose();
		Metrics.Interface.State(InterfaceMetricsEvent.InterfaceStates.Close).Type(InterfaceMetricsEvent.InterfaceTypes.BugReport).Send();
	}
}
