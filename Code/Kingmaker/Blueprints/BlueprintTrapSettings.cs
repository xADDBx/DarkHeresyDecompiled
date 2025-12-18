using System;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[HashRoot]
[TypeId("de4e0b0e79fa417a9a142251950680f2")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintTrapSettings : BlueprintScriptableObject
{
	[Serializable]
	public struct IntRange
	{
		public int from;

		public int to;

		public int PickRandom()
		{
			return PFStatefulRandom.Blueprints.Range(from, to);
		}
	}

	public int ActorLevel;

	public IntRange ActorStatMod;

	public IntRange DisableDC;

	public IntRange PerceptionDC;
}
