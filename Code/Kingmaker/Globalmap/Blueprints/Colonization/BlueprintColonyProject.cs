using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.UIDataProvider;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Obsolete]
[TypeId("86139952581c4e7fa135794685e236f9")]
public class BlueprintColonyProject : BlueprintScriptableObject, IUIDataProvider
{
	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private LocalizedString m_MechanicString;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	public bool IsStartingProject;

	public int SegmentsToBuild;

	[SerializeField]
	public ActionList ActionsOnStart;

	[SerializeField]
	public ActionList ActionsOnFinish;

	[SerializeField]
	public ConditionsChecker AvailabilityConditions;

	public string MechanicString => m_MechanicString;

	public string Name => m_Name;

	public string Description => m_Description;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => m_Name;
}
