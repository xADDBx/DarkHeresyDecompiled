using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.Items.Equipment;

[Serializable]
[JsonObject]
public class PersonalizedAsksContainer : IReadOnlyList<PersonalizedAsk>, IEnumerable<PersonalizedAsk>, IEnumerable, IReadOnlyCollection<PersonalizedAsk>
{
	public PersonalizedAsk AskEntry0 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry1 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry2 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry3 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry4 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry5 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry6 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry7 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry8 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry9 = new PersonalizedAsk();

	public PersonalizedAsk AskEntry10 = new PersonalizedAsk();

	public int Count => Length;

	public int Length => All.Count();

	[NotNull]
	public PersonalizedAsk this[int index]
	{
		get
		{
			return index switch
			{
				0 => AskEntry0, 
				1 => AskEntry1, 
				2 => AskEntry2, 
				3 => AskEntry3, 
				4 => AskEntry4, 
				5 => AskEntry5, 
				6 => AskEntry6, 
				7 => AskEntry7, 
				8 => AskEntry8, 
				9 => AskEntry9, 
				10 => AskEntry10, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				AskEntry0 = value;
				break;
			case 1:
				AskEntry1 = value;
				break;
			case 2:
				AskEntry2 = value;
				break;
			case 3:
				AskEntry3 = value;
				break;
			case 4:
				AskEntry4 = value;
				break;
			case 5:
				AskEntry5 = value;
				break;
			case 6:
				AskEntry6 = value;
				break;
			case 7:
				AskEntry7 = value;
				break;
			case 8:
				AskEntry8 = value;
				break;
			case 9:
				AskEntry9 = value;
				break;
			case 10:
				AskEntry10 = value;
				break;
			}
		}
	}

	public IEnumerable<PersonalizedAsk> All
	{
		get
		{
			yield return AskEntry0 ?? (AskEntry0 = new PersonalizedAsk());
			yield return AskEntry1 ?? (AskEntry1 = new PersonalizedAsk());
			yield return AskEntry2 ?? (AskEntry2 = new PersonalizedAsk());
			yield return AskEntry3 ?? (AskEntry3 = new PersonalizedAsk());
			yield return AskEntry4 ?? (AskEntry4 = new PersonalizedAsk());
			yield return AskEntry5 ?? (AskEntry5 = new PersonalizedAsk());
			yield return AskEntry6 ?? (AskEntry6 = new PersonalizedAsk());
			yield return AskEntry7 ?? (AskEntry7 = new PersonalizedAsk());
			yield return AskEntry8 ?? (AskEntry8 = new PersonalizedAsk());
			yield return AskEntry9 ?? (AskEntry9 = new PersonalizedAsk());
			yield return AskEntry10 ?? (AskEntry10 = new PersonalizedAsk());
		}
	}

	public IEnumerable<(int Index, PersonalizedAsk ask)> AllWithIndex => Enumerable.Range(0, Count).Zip(All, (int i, PersonalizedAsk a) => (i: i, a: a));

	public IEnumerator<PersonalizedAsk> GetEnumerator()
	{
		return All.Where((PersonalizedAsk i) => i != null).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
