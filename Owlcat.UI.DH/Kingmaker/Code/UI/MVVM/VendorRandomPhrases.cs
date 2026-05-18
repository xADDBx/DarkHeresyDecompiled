using System;
using System.Collections.Generic;
using Code.View.UI.MVVM;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class VendorRandomPhrases : StringsContainer
{
	[SerializeField]
	private EnumToObjectSelector<FactionType, List<LocalizedString>> m_HelloPhrases = new EnumToObjectSelector<FactionType, List<LocalizedString>>();

	[SerializeField]
	private EnumToObjectSelector<FactionType, List<LocalizedString>> m_FinishDealPhrases = new EnumToObjectSelector<FactionType, List<LocalizedString>>();

	[Header("Special Vendors Phrases")]
	public SpecialPhrasesDict[] SpecialPhrases;

	private int m_Index;

	public string TakePhrase(FactionType factionType, BaseUnitEntity unit)
	{
		SpecialPhrasesDict[] specialPhrases = SpecialPhrases;
		foreach (SpecialPhrasesDict specialPhrasesDict in specialPhrases)
		{
			if (specialPhrasesDict.Vendors.Any((BlueprintUnitReference x) => x.Is(unit?.Blueprint)))
			{
				m_Index = UnityEngine.Random.Range(0, specialPhrasesDict.HelloPhrases.Count);
				return specialPhrasesDict.HelloPhrases[m_Index];
			}
		}
		List<LocalizedString> entity = m_HelloPhrases.GetEntity(factionType);
		if (entity == null || entity.Count == 0)
		{
			return string.Empty;
		}
		m_Index = UnityEngine.Random.Range(0, entity.Count);
		return entity[m_Index];
	}

	public string TakeFinishDealPhrase(FactionType factionType, BaseUnitEntity unit)
	{
		SpecialPhrasesDict[] specialPhrases = SpecialPhrases;
		foreach (SpecialPhrasesDict specialPhrasesDict in specialPhrases)
		{
			if (specialPhrasesDict.Vendors.Any((BlueprintUnitReference x) => x.Is(unit.Blueprint)))
			{
				m_Index = UnityEngine.Random.Range(0, specialPhrasesDict.FinishDealPhrases.Count);
				return specialPhrasesDict.FinishDealPhrases[m_Index];
			}
		}
		List<LocalizedString> entity = m_FinishDealPhrases.GetEntity(factionType);
		if (entity == null || entity.Count == 0)
		{
			return string.Empty;
		}
		m_Index = UnityEngine.Random.Range(0, entity.Count);
		return entity[m_Index];
	}
}
