using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Blueprints;

[TypeId("2023998d28144ac58448456165c6766d")]
public class BlueprintAdditionalCombatObjective : BlueprintScriptableObject
{
	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	public string Name => m_Name;

	public string Description => m_Description;
}
