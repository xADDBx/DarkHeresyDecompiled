using System;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UIDataProvider;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Obsolete]
[TypeId("af36c530de7c4b50bb5395a2581ff6bc")]
public class BlueprintColonyEventResult : BlueprintScriptableObject, IUIDataProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintColonyEventResult>
	{
	}

	[SerializeField]
	private BlueprintColonyEventReference m_OwnerEvent;

	[SerializeField]
	private LocalizedString m_Name;

	public BlueprintColonyEvent OwnerEvent => m_OwnerEvent?.Get();

	public string Name => m_Name;

	public string Description => "";

	public Sprite Icon => null;

	public string NameForAcronym => null;
}
