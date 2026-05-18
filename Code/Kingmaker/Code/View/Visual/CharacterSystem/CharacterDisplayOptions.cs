using System;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem;

public struct CharacterDisplayOptions
{
	private bool m_IsPeacefulMode;

	private bool m_ShowHelmet;

	private bool m_ShowArmor;

	private bool m_ShowBackpack;

	private bool m_ShowCloak;

	private bool m_ShowGloves;

	private bool m_ShowBoots;

	private bool m_ShowCloth;

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
			result.ShowArmor = true;
			result.ShowBackpack = true;
			result.ShowCloak = true;
			result.ShowGloves = true;
			result.ShowBoots = true;
			result.ShowCloth = true;
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
			SetVisualProperty(ref m_IsPeacefulMode, value, Character.CharacterRebuildMode.OnlyOutfit);
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
			SetVisualProperty(ref m_ShowHelmet, value, Character.CharacterRebuildMode.FullUpdate);
		}
	}

	public bool ShowArmor
	{
		get
		{
			return m_ShowArmor;
		}
		set
		{
			SetVisualProperty(ref m_ShowArmor, value, Character.CharacterRebuildMode.FullUpdate);
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
			SetVisualProperty(ref m_ShowBackpack, value, Character.CharacterRebuildMode.OnlyOutfit);
		}
	}

	public bool ShowCloak
	{
		get
		{
			return m_ShowCloak;
		}
		set
		{
			SetVisualProperty(ref m_ShowCloak, value, Character.CharacterRebuildMode.OnlyOutfit);
		}
	}

	public bool ShowGloves
	{
		get
		{
			return m_ShowGloves;
		}
		set
		{
			SetVisualProperty(ref m_ShowGloves, value, Character.CharacterRebuildMode.FullUpdate);
		}
	}

	public bool ShowBoots
	{
		get
		{
			return m_ShowBoots;
		}
		set
		{
			SetVisualProperty(ref m_ShowBoots, value, Character.CharacterRebuildMode.FullUpdate);
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
			SetVisualProperty(ref m_ShowCloth, value, Character.CharacterRebuildMode.OnlyOutfit);
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
			SetVisualProperty(ref m_ShowHelmetAboveAll, value, Character.CharacterRebuildMode.FullUpdate);
		}
	}

	private void SetVisualProperty(ref bool field, bool value, Character.CharacterRebuildMode mode)
	{
		if (field != value)
		{
			field = value;
			CharacterRebuildRequest |= mode;
		}
	}
}
