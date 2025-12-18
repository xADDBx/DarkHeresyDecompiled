using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BugDuplicateItemVM : ViewModel
{
	public readonly string Title;

	public readonly string Assignee;

	public readonly int PriorityType;

	public readonly string Status;

	public readonly string BuildStatus;

	public readonly int Distance;

	public readonly bool IsFixed;

	public readonly string Created;

	public readonly string FixVersion;

	public readonly string JiraTaskUrl;

	public readonly string MetUrl;

	public BugDuplicateItemVM(Ticket ticket)
	{
		Title = ticket.Summary;
		Assignee = ticket.Assignee ?? "Aeon";
		PriorityType = GetPriorityByString(ticket.Priority);
		Status = ticket.Status;
		BuildStatus = ticket.BuildStatus;
		Distance = ticket.Distance;
		IsFixed = ticket.Fixed;
		Created = "dd.mm.year";
		FixVersion = "Fix Version";
		JiraTaskUrl = "https://jira.owlcat.local/browse/" + ticket.JiraTaskId;
		MetUrl = "https://404";
	}

	private int GetPriorityByString(string value)
	{
		switch (value)
		{
		case "Blocker":
			return 1;
		case "Crit":
			return 2;
		case "Normal":
			return 3;
		case "Minor":
			return 4;
		case "Trivial":
			return 5;
		default:
			UberDebug.LogError("Error: Priority type " + value + " not defined");
			return 1;
		}
	}
}
