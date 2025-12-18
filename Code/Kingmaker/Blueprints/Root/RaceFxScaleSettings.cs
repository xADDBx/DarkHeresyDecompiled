using System;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class RaceFxScaleSettings
{
	public float Human = 1f;

	public float Spacemarine = 1f;

	public float Eldar = 1f;

	public float Kroot = 1f;

	[FormerlySerializedAs("Ogrin")]
	public float Ogryn = 1f;

	public float GetCoeff(Race? race)
	{
		return race switch
		{
			Race.Human => Human, 
			Race.Spacemarine => Spacemarine, 
			Race.Eldar => Eldar, 
			Race.Kroot => Kroot, 
			Race.Ogryn => Ogryn, 
			_ => 1f, 
		};
	}
}
