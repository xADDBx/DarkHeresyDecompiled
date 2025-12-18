using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[TypeId("ee6b74f4d5a940c6915c5a11a16622e6")]
public class BlueprintCustomLoopActionType : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintCustomLoopActionType>
	{
	}
}
