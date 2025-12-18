using System;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem;

public struct CharacterDisplayOptions
{
	private bool m_IsPeacefulMode;

	private bool m_ShowBackpack;

	private bool m_ShowCloth;

	private bool m_ShowHelmet;

	private bool m_ShowHelmetAboveAll;

	public bool IsInDollRoom;

	public Func<OutfitPart, GameObject, bool> OutfitFilter;

	public Character.CharacterRebuildMode CharacterRebuildRequest;

	public static CharacterDisplayOptions Default
	{
		get
		{
			CharacterDisplayOptions result = default(CharacterDisplayOptions);
			result.IsPeacefulMode = false;
			result.ShowHelmet = true;
			result.ShowCloth = true;
			result.ShowBackpack = true;
			result.ShowHelmetAboveAll = false;
			result.IsInDollRoom = false;
			return result;
		}
	}

	public bool IsPeacefulMode
	{
		get
		{
			return m_IsPeacefulMode;
		}
		set
		{
			if (m_IsPeacefulMode != value)
			{
				m_IsPeacefulMode = value;
				CharacterRebuildRequest |= Character.CharacterRebuildMode.OnlyOutfit;
			}
		}
	}

	public bool ShowHelmet
	{
		get
		{
			return m_ShowHelmet;
		}
		set
		{
			if (m_ShowHelmet != value)
			{
				m_ShowHelmet = value;
				CharacterRebuildRequest |= Character.CharacterRebuildMode.FullUpdate;
			}
		}
	}

	public bool ShowCloth
	{
		get
		{
			return m_ShowCloth;
		}
		set
		{
			if (m_ShowCloth != value)
			{
				m_ShowCloth = value;
				CharacterRebuildRequest |= Character.CharacterRebuildMode.FullUpdate;
			}
		}
	}

	public bool ShowBackpack
	{
		get
		{
			return m_ShowBackpack;
		}
		set
		{
			if (m_ShowBackpack != value)
			{
				m_ShowBackpack = value;
				CharacterRebuildRequest |= Character.CharacterRebuildMode.FullUpdate;
			}
		}
	}

	public bool ShowHelmetAboveAll
	{
		get
		{
			return m_ShowHelmetAboveAll;
		}
		set
		{
			if (m_ShowHelmetAboveAll != value)
			{
				m_ShowHelmetAboveAll = value;
				CharacterRebuildRequest |= Character.CharacterRebuildMode.FullUpdate;
			}
		}
	}
}
