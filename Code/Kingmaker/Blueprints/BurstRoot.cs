using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Serializable]
[ComponentName("Root/BurstRoot")]
[TypeId("f196a601fee74d508beb043ac9787838")]
public class BurstRoot : BlueprintScriptableObject
{
	public class BurstRootReference : BlueprintReference<BurstRoot>
	{
		public BurstRootReference()
		{
			guid = "41e5a9642d5f427ea1198d94ed0fe33e";
		}
	}

	private static readonly BurstRootReference s_Instance = new BurstRootReference();

	public BurstWeightSettings DefaultSettings;

	public static BurstRoot Instance => s_Instance;
}
