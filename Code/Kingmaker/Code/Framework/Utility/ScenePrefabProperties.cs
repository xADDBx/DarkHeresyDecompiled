using UnityEngine;
using Warhammer.Utility.Author;

namespace Kingmaker.Code.Framework.Utility;

[DisallowMultipleComponent]
public class ScenePrefabProperties : MonoBehaviour
{
	[SerializeField]
	public Author PrefabOwner;
}
