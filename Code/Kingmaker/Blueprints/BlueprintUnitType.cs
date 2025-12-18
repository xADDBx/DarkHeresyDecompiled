using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("0ec4fe8b79e9541479efc2a8517046be")]
public class BlueprintUnitType : BlueprintScriptableObject
{
	public StatType KnowledgeStat;

	public Sprite Image;

	public LocalizedString Name;

	public LocalizedString Description;

	[SerializeField]
	private BlueprintUnitFactReference[] m_SignatureAbilities = new BlueprintUnitFactReference[0];

	public ReferenceArrayProxy<BlueprintUnitFact> SignatureAbilities
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] signatureAbilities = m_SignatureAbilities;
			return signatureAbilities;
		}
	}
}
