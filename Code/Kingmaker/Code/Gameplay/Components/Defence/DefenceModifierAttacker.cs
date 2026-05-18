using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Mechanics.Actor;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.Defence;

[Serializable]
[Obsolete("Use AddStatModifier instead")]
[ComponentName("Defence/DefenceModifierAttacker")]
[TypeId("5ddec4bbd57c4351abe700f32a93d202")]
public class DefenceModifierAttacker : DefenceModifier
{
	protected override StatModifierScope Scope => StatModifierScope.Against;
}
