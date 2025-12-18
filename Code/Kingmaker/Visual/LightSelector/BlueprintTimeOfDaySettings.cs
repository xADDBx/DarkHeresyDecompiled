using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.LightSelector;

[TypeId("da0522219ba94ed43bce440f6b52b5b6")]
public class BlueprintTimeOfDaySettings : BlueprintScriptableObject
{
	public GameObject Morning;

	public GameObject Day;

	public GameObject Evening;

	public GameObject Night;
}
