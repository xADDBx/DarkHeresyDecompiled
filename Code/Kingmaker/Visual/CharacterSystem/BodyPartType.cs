using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
[CreateAssetMenu(menuName = "Character System/Body Part Type")]
[KnowledgeDatabaseID("e382d933d74b3c246a4aff1b8318fc40")]
public class BodyPartType : ScriptableObject
{
	[KDB("Уникальное имя. Некоторые имена захардкожены и используются логикой")]
	[SerializeField]
	private string m_Name;

	[KDB("Уникальная аббривеатура. Как правило из 2 букв. Используется для именования текстур")]
	[SerializeField]
	private string m_TexturePrefix;

	public string Name => m_Name;

	public string TexturePrefix => m_TexturePrefix;

	public bool IsTorso()
	{
		return Name == "Torso";
	}
}
