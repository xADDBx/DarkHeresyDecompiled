using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class ParticleGroup : ScriptableObject
{
	public List<int> ParticleIndices = new List<int>();

	public int Count => ParticleIndices.Count;

	public bool ContainsParticle(int index)
	{
		return ParticleIndices.Contains(index);
	}
}
