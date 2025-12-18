using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class RandomPickerBase : MonoBehaviour
{
	public abstract void Randomize(string seed);

	public abstract void Reset();

	protected List<int> GetRandomIds(string seed, int chooseCount, int objectsCount)
	{
		int seed2 = (seed ?? string.Empty).GetHashCode() & 0x7FFFFFFF;
		System.Random rng = new System.Random(seed2);
		return (from _ in Enumerable.Range(0, objectsCount)
			orderby rng.Next()
			select _).Take(chooseCount).ToList();
	}
}
