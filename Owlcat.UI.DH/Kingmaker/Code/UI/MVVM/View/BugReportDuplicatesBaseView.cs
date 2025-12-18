using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cheats;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BugReportDuplicatesBaseView : View<BugReportDuplicatesVM>
{
	[Header("Localizations")]
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[SerializeField]
	private TextMeshProUGUI m_LoadingProcessText;

	[Header("Widget")]
	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private BugDuplicateItemView m_WidgetEntityView;

	protected InputLayer InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private Coroutine m_DuplicatesListCoroutine;

	public const string InputLayerContextName = "BugReportDuplicatesViewInput";

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_LoadingProcessText.gameObject.SetActive(value: true);
		m_TitleText.text = UIStrings.Instance.UIBugReport.DuplicateBugsTitleText.Text;
		m_LoadingProcessText.text = UIStrings.Instance.UIBugReport.LoadingProcessDuplicatesListText.Text;
		BuildNavigation();
		m_DuplicatesListCoroutine = StartCoroutine(LoadDuplicatesListCoroutine());
	}

	protected override void OnUnbind()
	{
		if (m_DuplicatesListCoroutine != null)
		{
			StopCoroutine(m_DuplicatesListCoroutine);
			m_DuplicatesListCoroutine = null;
		}
		base.gameObject.SetActive(value: false);
		InputLayer = null;
		m_WidgetList.Clear();
	}

	private IEnumerator LoadDuplicatesListCoroutine()
	{
		Task<FindTicketsResponse> task = GetDuplicatesListAsync();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		List<BugDuplicateItemVM> list = new List<BugDuplicateItemVM>();
		Ticket[] tickets = task.Result.Tickets;
		foreach (Ticket ticket in tickets)
		{
			list.Add(new BugDuplicateItemVM(ticket));
		}
		if (list.Count == 0)
		{
			m_LoadingProcessText.text = UIStrings.Instance.UIBugReport.DuplicatesListIsEmptyText.Text;
		}
		else
		{
			m_LoadingProcessText.gameObject.SetActive(value: false);
			DrawEntities(list);
		}
		m_DuplicatesListCoroutine = null;
	}

	private async Task<FindTicketsResponse> GetDuplicatesListAsync()
	{
		try
		{
			return await new SirenClient().Ticket.FindTickets(new FindTicketsRequest
			{
				Context = base.ViewModel.Context,
				Area = Utilities.GetBlueprintName(CheatsJira.GetCurrentAreaPart()),
				Project = "WH"
			});
		}
		catch (Exception arg)
		{
			UberDebug.LogError($"Failed FindTicketsRequest {arg}");
			throw;
		}
	}

	private void DrawEntities(IEnumerable<BugDuplicateItemVM> duplicatesList)
	{
		m_WidgetList.DrawEntries(duplicatesList, m_WidgetEntityView).AddTo(this);
		UpdateNavigation();
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateInput();
		GamePad.Instance.PushLayer(InputLayer).AddTo(this);
	}

	protected virtual void CreateInput()
	{
		InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "BugReportDuplicatesViewInput"
		});
	}

	protected virtual void UpdateNavigation()
	{
		InputLayer.Unbind();
		List<IConsoleNavigationEntity> navigationEntities = m_WidgetList.GetNavigationEntities();
		m_NavigationBehaviour.AddColumn(navigationEntities);
		InputLayer.Bind();
	}
}
