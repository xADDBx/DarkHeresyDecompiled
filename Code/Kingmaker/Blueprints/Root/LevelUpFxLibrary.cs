using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/LevelUpFxLibrary")]
[TypeId("097b5f538f844b69a517ba24758d78c7")]
public class LevelUpFxLibrary : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<LevelUpFxLibrary>
	{
	}

	[SerializeField]
	public PrefabLink LevelUpFx;

	[AkEventReference]
	public string SoundFx;
}
