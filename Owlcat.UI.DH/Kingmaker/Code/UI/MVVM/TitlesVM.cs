using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TitlesVM : ViewModel
{
	public List<(string, BlueprintCreditsGroup)> Titles;

	private readonly Action m_CloseAction;

	private const int MaxPersonsInBlock = 30;

	public TitlesVM(Action closeAction)
	{
		m_CloseAction = closeAction;
	}

	protected override void OnDispose()
	{
		CloseTitles();
	}

	public void TryGenerateTitles()
	{
		try
		{
			GenerateTitles();
		}
		catch (Exception value)
		{
			System.Console.WriteLine(value);
			throw;
		}
	}

	private void GenerateTitles()
	{
		Titles = new List<(string, BlueprintCreditsGroup)>();
		List<BlueprintCreditsGroup> list = ConfigRoot.Instance.UIConfig.Credits.EndTitlesGroups.Select((BlueprintCreditsGroupReference g) => g.Get()).ToList();
		StringBuilder b = new StringBuilder();
		foreach (BlueprintCreditsGroup item in list)
		{
			if (!item.IsBakers || item.ShowInFinalTitles)
			{
				FillHeader(item, b);
				if (item.IsBakers)
				{
					FillBackers(item, b);
				}
				else
				{
					FillTeamPersonsOrText(item, b);
				}
			}
		}
	}

	private void FillHeader(BlueprintCreditsGroup group, StringBuilder b)
	{
		string value = PageGenerator.WriteCompany(group.HeaderText);
		if (!string.IsNullOrEmpty(value))
		{
			b.AppendLine(value);
			AddBlock(b, group);
		}
	}

	private void FillBackers(BlueprintCreditsGroup group, StringBuilder b)
	{
		int count = group.Persones.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			string text = group.Persones[i].Name.Replace("\r", "");
			if (!string.IsNullOrEmpty(text))
			{
				b.AppendLine(PageGenerator.WritePerson(text));
				num++;
				if (num >= 30)
				{
					AddBlock(b, group);
					num = 0;
				}
			}
		}
		AddBlock(b, group);
	}

	private void FillTeamPersonsOrText(BlueprintCreditsGroup group, StringBuilder b)
	{
		for (int i = 0; i < group.OrderTeams.Count; i++)
		{
			string order = group.OrderTeams[i];
			CreditTeam team = group.TeamsData.Teams.Find((CreditTeam x) => x.KeyTeam.Trim() == order.Trim());
			if (team == null)
			{
				continue;
			}
			List<CreditPerson> list = group.Persones.FindAll((CreditPerson x) => x.KeyTeam.Trim() == team.KeyTeam.Trim());
			string value = PageGenerator.WriteHeader(team.NameTeam.Text);
			if (list.Count > 0)
			{
				b.AppendLine(value);
			}
			foreach (CreditPerson item in list)
			{
				if (!string.IsNullOrEmpty(item.Text?.Text))
				{
					b.Clear();
					b.AppendLine(PageGenerator.WriteText(item.Text?.Text ?? string.Empty));
					AddBlock(b, group);
				}
				else if (!string.IsNullOrEmpty(item.Name.Replace("\r", "")))
				{
					string value2 = PageGenerator.WritePerson(item.Name.Replace("\r", "")) + PageGenerator.WriteRole(item.KeyRole);
					b.AppendLine(value2);
				}
			}
			if (b.Length > 0)
			{
				AddBlock(b, group);
			}
		}
	}

	private void AddBlock(StringBuilder b, BlueprintCreditsGroup group)
	{
		if (b.Length > 0)
		{
			Titles.Add((b.ToString(), group));
			b.Clear();
		}
	}

	public void CloseTitles()
	{
		Titles?.Clear();
		Titles = null;
		m_CloseAction?.Invoke();
	}

	public void OpenCancelSettingsDialog()
	{
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(UIStrings.Instance.Credits.AreYouSureToSkipTitles, DialogMessageBoxType.Dialog, OnCancelDialogAnswer);
		});
		void OnCancelDialogAnswer(DialogMessageBoxButton buttonType)
		{
			if (buttonType == DialogMessageBoxButton.Yes)
			{
				CloseTitles();
			}
		}
	}
}
