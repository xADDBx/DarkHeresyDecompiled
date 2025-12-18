using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Bugreport;

namespace Kingmaker.Utility;

public class BugContext : IComparable<BugContext>
{
	public enum InnerContextType
	{
		None,
		Dialog,
		Unit,
		Spell,
		Buff,
		Condition,
		Item,
		CharacterClass,
		Area
	}

	private const string DefaultAspect = "None";

	private static readonly IReadOnlyList<string> DefaultAspects = SortAspects(new List<string> { "None", "Animation", "Code", "Localization", "Mechanics", "Narrative", "Sound", "UI", "Visual" });

	public bool IsTooltip;

	public string UiFeature;

	public string OtherUiFeature;

	private readonly Dictionary<string, int> _aspectIndexMap = new Dictionary<string, int>();

	private readonly List<(string Aspect, string Name)> _aspectAssigneeList = new List<(string, string)>();

	public FullScreenUIType ActiveFullScreenUIType { get; set; }

	public string Type { get; private set; }

	public string Aspect { get; private set; }

	public IReadOnlyList<string> Aspects { get; private set; }

	public BlueprintScriptableObject ContextObject { get; set; }

	public BugContext(string context)
	{
		Type = context;
		BuildAspectIndex();
		FillAssigneeList();
	}

	public BugContext(BlueprintScriptableObject contextObject)
	{
		ContextObject = contextObject;
		ResetContextType();
		BuildAspectIndex();
		FillAssigneeList();
	}

	public BugContext(BlueprintScriptableObject contextObject, string context)
	{
		Type = context;
		ContextObject = contextObject;
		BuildAspectIndex();
		FillAssigneeList();
	}

	public int CompareTo(BugContext other)
	{
		return string.Compare(Type, other.Type, StringComparison.OrdinalIgnoreCase);
	}

	public List<(string Aspect, int AspectIndex, string Assignee)> GetContextAspectAssignees()
	{
		List<(string, int, string)> list = new List<(string, int, string)>();
		CustomDevelopersContextsBugreportOptions customDevelopers = ReportingUtils.Instance.ReportOptions.Contexts.CustomDevelopers;
		foreach (var aspectAssignee in _aspectAssigneeList)
		{
			string item = aspectAssignee.Aspect;
			string item2 = aspectAssignee.Name;
			int valueOrDefault = _aspectIndexMap.GetValueOrDefault(item, -1);
			if (item2.Contains("ui_designer") && customDevelopers.TryGetGroupCustomDevelopersOptions("UI Designer", out GroupCustomDevelopersBugreportOptions group))
			{
				string text = UiFeature;
				if (string.IsNullOrEmpty(text))
				{
					text = OtherUiFeature;
				}
				if (string.IsNullOrEmpty(text))
				{
					text = GetContextObjectBlueprintName();
				}
				if (string.IsNullOrEmpty(text))
				{
					text = ActiveFullScreenUIType.ToString();
				}
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				if (group.TryGetDeveloperOptions(text, out UserInfoBugreportOptions developerOptions))
				{
					list.Add((null, -1, developerOptions.UserName));
					continue;
				}
			}
			list.Add((item, valueOrDefault, item2));
		}
		return list;
	}

	public string GetContextDescription()
	{
		if (Type == "Encounter")
		{
			return ContextObject.NameSafe() + "_Encounter";
		}
		if (ContextObject != null)
		{
			return GetContextObjectDescription();
		}
		if (!string.IsNullOrEmpty(UiFeature))
		{
			return "<B>Interface:</B> " + UiFeature;
		}
		if (!string.IsNullOrEmpty(OtherUiFeature))
		{
			return "<B>Interface:</B> " + OtherUiFeature;
		}
		if (Type == "Interface")
		{
			if (GameUIState.Instance.IsInMainMenu)
			{
				if (ActiveFullScreenUIType == FullScreenUIType.Chargen)
				{
					return $"<B>Interface:</B> {ActiveFullScreenUIType:G}";
				}
				return "<B>MainMenu</B>";
			}
			if (ActiveFullScreenUIType != 0)
			{
				return $"<B>Interface:</B> {ActiveFullScreenUIType:G}";
			}
		}
		return Type;
	}

	public int GetAspectIndex(string aspect)
	{
		return _aspectIndexMap.GetValueOrDefault(aspect, 0);
	}

	public void SelectAspect(int aspectIndex)
	{
		if (aspectIndex < 0 || aspectIndex >= Aspects.Count)
		{
			Aspect = Aspects[0];
		}
		else
		{
			Aspect = Aspects[aspectIndex];
		}
	}

	public string GetContextObjectBlueprintName()
	{
		if (ContextObject != null)
		{
			return Utilities.GetBlueprintName(ContextObject);
		}
		if (Type == "Crash")
		{
			return "TECHNICAL_CRASH";
		}
		if (!string.IsNullOrEmpty(UiFeature))
		{
			return UiFeature;
		}
		if (!string.IsNullOrEmpty(OtherUiFeature))
		{
			return OtherUiFeature;
		}
		if (ActiveFullScreenUIType != 0)
		{
			return ActiveFullScreenUIType.ToString("G");
		}
		if (Type == "Interface")
		{
			if (!GameUIState.Instance.IsInMainMenu)
			{
				return "Interface";
			}
			return "MainMenu";
		}
		return string.Empty;
	}

	public string GetDialogGuid()
	{
		if (!(ContextObject is BlueprintDialog))
		{
			return string.Empty;
		}
		return ContextObject.AssetGuid;
	}

	private void ResetContextType()
	{
		if (ContextObject != null)
		{
			if (ContextObject is BlueprintDialog)
			{
				Type = "Dialog";
			}
			if (ContextObject is BlueprintItem)
			{
				Type = "Item";
			}
			BlueprintScriptableObject contextObject = ContextObject;
			if (contextObject is BlueprintFeatureBase || contextObject is BlueprintAbility || contextObject is BlueprintToggleAbility || contextObject is BlueprintBuff || contextObject is BlueprintAbilityModifier || contextObject is BlueprintAbilityTag)
			{
				Type = "Ability";
			}
			contextObject = ContextObject;
			if (contextObject is BlueprintUnit || contextObject is BlueprintRace)
			{
				Type = "Unit";
			}
			contextObject = ContextObject;
			if (contextObject is BlueprintArea || contextObject is BlueprintEtude)
			{
				Type = "Area";
			}
		}
		else
		{
			Type = "Area";
		}
	}

	private string GetContextObjectDescription()
	{
		BlueprintScriptableObject contextObject = ContextObject;
		if (!(contextObject is BlueprintArea blueprintArea))
		{
			if (!(contextObject is BlueprintDialog))
			{
				if (!(contextObject is BlueprintItem blueprintItem))
				{
					if (!(contextObject is BlueprintRace blueprintRace))
					{
						if (!(contextObject is BlueprintFeatureBase blueprintFeatureBase))
						{
							if (!(contextObject is BlueprintAbility blueprintAbility))
							{
								if (!(contextObject is BlueprintToggleAbility blueprintToggleAbility))
								{
									if (!(contextObject is BlueprintAbilityModifier blueprintAbilityModifier))
									{
										if (!(contextObject is BlueprintAbilityTag blueprintAbilityTag))
										{
											if (!(contextObject is BlueprintBuff blueprintBuff))
											{
												if (!(contextObject is BlueprintUnit blueprintUnit))
												{
													if (!(contextObject is BlueprintEtude blueprintEtude))
													{
														if (contextObject != null)
														{
															return "<B>Blueprint:</B> " + contextObject.name;
														}
														return "<B>None</B>";
													}
													return "<B>Etude:</B> " + GetBlueprintNameIfEmpty(blueprintEtude.name);
												}
												return "<B>Unit:</B> " + GetBlueprintNameIfEmpty(blueprintUnit.CharacterName);
											}
											return "<B>Buff:</B> " + GetBlueprintNameIfEmpty(blueprintBuff.Name);
										}
										return "<B>Ability:</B> " + GetBlueprintNameIfEmpty(blueprintAbilityTag.Name);
									}
									return "<B>Ability:</B> " + GetBlueprintNameIfEmpty(blueprintAbilityModifier.Name);
								}
								return "<B>Ability:</B> " + GetBlueprintNameIfEmpty(blueprintToggleAbility.Name);
							}
							return "<B>Ability:</B> " + GetBlueprintNameIfEmpty(blueprintAbility.Name);
						}
						return "<B>Feature:</B> " + GetBlueprintNameIfEmpty(blueprintFeatureBase.Name);
					}
					return "<B>Race:</B> " + GetBlueprintNameIfEmpty(blueprintRace.Name);
				}
				return "<B>Item:</B> " + GetBlueprintNameIfEmpty(blueprintItem.Name);
			}
			return "<B>Current Dialog</B>";
		}
		return "<B>Area:</B> " + GetBlueprintNameIfEmpty(blueprintArea.AreaDisplayName);
	}

	private void BuildAspectIndex()
	{
		if (string.IsNullOrWhiteSpace(Type))
		{
			Aspects = DefaultAspects;
			return;
		}
		_aspectIndexMap.Clear();
		IReadOnlyList<string> aspects;
		if (!ReportingUtils.Instance.ReportOptions.Contexts.Developers.TryGetContextOptions(Type, out ContextBugreportOptions context))
		{
			aspects = DefaultAspects;
		}
		else
		{
			IReadOnlyList<string> readOnlyList = SortAspects(context.Aspects);
			aspects = readOnlyList;
		}
		Aspects = aspects;
		for (int i = 0; i < Aspects.Count; i++)
		{
			_aspectIndexMap.Add(Aspects[i], i);
		}
	}

	private void FillAssigneeList()
	{
		_aspectAssigneeList.Clear();
		if (string.IsNullOrWhiteSpace(Type) || !ReportingUtils.Instance.ReportOptions.Contexts.Developers.TryGetContextOptions(Type, out ContextBugreportOptions context))
		{
			return;
		}
		foreach (string aspect2 in Aspects)
		{
			AspectBugreportOptions aspect;
			string text = (context.TryGetAspectOptions(aspect2, out aspect) ? aspect.Assignee.UserName : string.Empty);
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (text == "area_designer")
				{
					text = Utilities.GetDesigner(CheatsJira.GetCurrentArea());
				}
				_aspectAssigneeList.Add((aspect2, text));
			}
		}
	}

	private string GetBlueprintNameIfEmpty(string localizedName)
	{
		if (!localizedName.IsNullOrEmpty())
		{
			return localizedName;
		}
		return Utilities.GetBlueprintName(ContextObject);
	}

	public string GetContextLink()
	{
		BlueprintScriptableObject contextObject = ContextObject;
		if (contextObject != null)
		{
			if (contextObject is BlueprintDialog)
			{
				BlueprintCue blueprintCue = Game.Instance.Controllers?.DialogController.CurrentCue;
				return CheatsJira.MakeOpenString("Cue", Utilities.GetBlueprintName(blueprintCue), "dialog", blueprintCue.AssetGuid);
			}
			return CheatsJira.MakeOpenString("Blueprint", Utilities.GetBlueprintName(ContextObject), "simple", ContextObject.AssetGuid);
		}
		return string.Empty;
	}

	public string GetHeader()
	{
		if (Type == "Encounter" && ContextObject != null)
		{
			return "[" + ContextObject.NameSafe() + "_Encounter]";
		}
		if (!string.IsNullOrEmpty(UiFeature))
		{
			return "[" + UiFeature + "]";
		}
		BlueprintArea currentArea = CheatsJira.GetCurrentArea();
		if (ContextObject is BlueprintDialog)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		BlueprintScriptableObject contextObject = ContextObject;
		if (contextObject is BlueprintAbility || contextObject is BlueprintActivatableAbility)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if ((IsTooltip || ActiveFullScreenUIType != 0) && ContextObject != null)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if (ContextObject != null)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if (Type == "Interface")
		{
			if (IsTooltip || ActiveFullScreenUIType != 0)
			{
				return "[" + ActiveFullScreenUIType.ToString() + "]";
			}
			return "[UI]";
		}
		switch (Type)
		{
		case "Crash":
		case "Exception":
		case "Coop":
		case "Desync":
			return "[" + Type + "]";
		default:
		{
			string blueprintName = Utilities.GetBlueprintName(currentArea);
			if (blueprintName != null)
			{
				return "[" + blueprintName + "]";
			}
			return "";
		}
		}
	}

	private static List<string> SortAspects(IEnumerable<string> aspects)
	{
		List<string> list = (from x in aspects
			where x != "None"
			orderby x
			select x).ToList();
		list.Insert(0, "None");
		return list;
	}
}
