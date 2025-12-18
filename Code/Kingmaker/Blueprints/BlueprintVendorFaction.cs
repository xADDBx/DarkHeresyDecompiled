using System;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("535b405b948543eca5e301737aece91b")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintVendorFaction : BlueprintScriptableObject
{
	[SerializeField]
	private FactionType m_Faction;

	[SerializeField]
	private LocalizedString m_DisplayName;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	public FactionType FactionType => m_Faction;

	public LocalizedString DisplayName => m_DisplayName;

	public LocalizedString Description => m_Description;

	public Sprite Icon => m_Icon;
}
