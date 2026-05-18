using System;
using Kingmaker.Blueprints.Base;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Decorators;

[Serializable]
[CreateAssetMenu(menuName = "Character System/Decorator Object")]
public class UnitAnimationDecoratorObject : ScriptableObject
{
	public bool UseGender;

	public Gender gender = Gender.Female;

	public DecoratorEntry[] Entries;
}
