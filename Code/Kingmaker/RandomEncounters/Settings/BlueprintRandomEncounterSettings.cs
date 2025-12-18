using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.RandomEncounters.Settings;

[TypeId("afef9c7a73956614084db8749b6451f8")]
public class BlueprintRandomEncounterSettings : BlueprintScriptableObject, IDialogReference, IAreaEnterPointReference
{
	public static readonly string RootDirectory = PathUtils.BlueprintPath("World/Areas/Random_Encounters");

	public bool ExcludeFromREList;

	public bool IsPeaceful;

	public LocalizedString Name;

	public LocalizedString Description;

	public RandomEncounterAvoidType AvoidType;

	[InfoBox("Skill check Stealth")]
	public int AvoidDC;

	public int EncountersLimit;

	public ConditionsChecker Conditions;

	public RandomEncounterType Type;

	public ActionList OnEnter;

	public bool CanBeCampingEncounter;

	[ShowIf("NeedArea")]
	[SerializeField]
	private BlueprintAreaEnterPointReference m_AreaEntrance;

	[ShowIf("IsBookEvent")]
	[SerializeField]
	private BlueprintDialogReference m_BookEvent;

	public BlueprintAreaEnterPoint AreaEntrance => m_AreaEntrance?.Get();

	public BlueprintDialog BookEvent => m_BookEvent?.Get();

	public bool NeedArea
	{
		get
		{
			if (Type != RandomEncounterType.Custom)
			{
				return Type == RandomEncounterType.RandomizedCombat;
			}
			return true;
		}
	}

	public bool IsBookEvent => Type == RandomEncounterType.BookEvent;

	public bool IsRandomizedCombat => Type == RandomEncounterType.RandomizedCombat;

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != BookEvent)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		return point == AreaEntrance;
	}
}
