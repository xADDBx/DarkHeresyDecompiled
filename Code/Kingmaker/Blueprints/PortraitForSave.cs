using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class PortraitForSave
{
	[JsonProperty]
	[OwlPackInclude]
	private BlueprintPortrait m_Blueprint;

	[JsonProperty]
	[OwlPackInclude]
	private PortraitData m_Data;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsMainCharacter;

	public PortraitData Data
	{
		get
		{
			if (!m_Blueprint)
			{
				return m_Data;
			}
			return m_Blueprint.Data;
		}
	}

	public bool IsMainCharacter => m_IsMainCharacter;

	public PortraitForSave(BlueprintPortrait blueprint, bool isMainCharacter)
	{
		m_Blueprint = blueprint;
		m_IsMainCharacter = isMainCharacter;
	}

	public PortraitForSave(PortraitData data, bool isMainCharacter)
	{
		m_Data = data;
		m_IsMainCharacter = isMainCharacter;
	}

	[JsonConstructor]
	public PortraitForSave()
	{
	}
}
