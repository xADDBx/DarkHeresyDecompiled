using System;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
[CreateAssetMenu(menuName = "Character System/Outfit Part Type")]
public class OutfitPartType : ScriptableObject
{
	[SerializeField]
	private string m_Name;

	public string Name => m_Name;
}
