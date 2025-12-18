using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CombatMessageReplacedAbility : CombatMessageBase
{
	private readonly string m_Name;

	private readonly Sprite m_Sprite;

	public CombatMessageReplacedAbility(string name, Sprite sprite)
	{
		m_Name = name;
		m_Sprite = sprite;
	}

	public override string GetText()
	{
		return m_Name;
	}

	public override Sprite GetSprite()
	{
		return m_Sprite;
	}

	public override bool GetAttention()
	{
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CombatMessageReplacedAbility combatMessageReplacedAbility))
		{
			return false;
		}
		if (m_Name == combatMessageReplacedAbility.m_Name)
		{
			return m_Sprite == combatMessageReplacedAbility.m_Sprite;
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(m_Name);
		hashCode.Add(m_Sprite ? m_Sprite.GetInstanceID() : 0);
		return hashCode.ToHashCode();
	}
}
