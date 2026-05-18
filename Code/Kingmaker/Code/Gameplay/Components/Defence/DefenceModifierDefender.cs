using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Mechanics.Actor;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.Defence;

[Serializable]
[Obsolete("Use AddStatModifier instead")]
[ComponentName("Defence/DefenceModifierDefender")]
[TypeId("3aea4d4ca1a44155bb360fd875d554c8")]
public class DefenceModifierDefender : DefenceModifier
{
	protected override StatModifierScope Scope => StatModifierScope.Owner;
}
