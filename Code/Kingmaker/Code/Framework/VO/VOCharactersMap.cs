using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
[VerticalLayout]
public class VOCharactersMap
{
	public List<CharacterEntry> Characters = new List<CharacterEntry>();

	public List<string> Folders = new List<string>();

	private Dictionary<string, CharacterEntry> m_VoGuidToCharacterEntryMap;

	private Dictionary<string, string> m_BlueprintUnitNameToVoGuidMap;

	private Dictionary<string, string> m_AsksAssetGuidToVoGuidMap;

	public Dictionary<string, CharacterEntry> VoGuidToCharacterEntryMap => m_VoGuidToCharacterEntryMap ?? (m_VoGuidToCharacterEntryMap = Characters.ToDictionary((CharacterEntry c) => c.Guid, (CharacterEntry c) => c));

	public Dictionary<string, string> BlueprintUnitNameToVoGuidMap
	{
		get
		{
			if (m_BlueprintUnitNameToVoGuidMap == null)
			{
				m_BlueprintUnitNameToVoGuidMap = new Dictionary<string, string>();
				foreach (CharacterEntry character in Characters)
				{
					foreach (BlueprintUnitReference unit in character.Units)
					{
						BlueprintUnit blueprintUnit = unit.Get();
						if (blueprintUnit != null && !string.IsNullOrEmpty(blueprintUnit.name))
						{
							m_BlueprintUnitNameToVoGuidMap[blueprintUnit.name] = character.Guid;
						}
					}
				}
			}
			return m_BlueprintUnitNameToVoGuidMap;
		}
	}

	public Dictionary<string, string> AsksAssetGuidToVoGuidMap
	{
		get
		{
			if (m_AsksAssetGuidToVoGuidMap == null)
			{
				m_AsksAssetGuidToVoGuidMap = new Dictionary<string, string>();
				foreach (CharacterEntry character in Characters)
				{
					if (character.Asks != null && !string.IsNullOrEmpty(character.Asks.Guid))
					{
						m_AsksAssetGuidToVoGuidMap[character.Asks.Guid] = character.Guid;
					}
				}
			}
			return m_AsksAssetGuidToVoGuidMap;
		}
	}

	public List<BlueprintUnitReference> GetUsagesOfVoId(string guid)
	{
		if (!VoGuidToCharacterEntryMap.TryGetValue(guid, out var value))
		{
			return new List<BlueprintUnitReference>();
		}
		return value.Units;
	}

	public void ClearHelper()
	{
		m_BlueprintUnitNameToVoGuidMap = null;
		m_VoGuidToCharacterEntryMap = null;
		m_AsksAssetGuidToVoGuidMap = null;
	}
}
