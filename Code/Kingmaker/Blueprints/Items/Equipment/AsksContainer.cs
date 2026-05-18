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
public class AsksContainer : IReadOnlyList<AskEntry>, IEnumerable<AskEntry>, IEnumerable, IReadOnlyCollection<AskEntry>
{
	private const int EntryCount = 12;

	public AskEntry AskEntry0 = new AskEntry();

	public AskEntry AskEntry1 = new AskEntry();

	public AskEntry AskEntry2 = new AskEntry();

	public AskEntry AskEntry3 = new AskEntry();

	public AskEntry AskEntry4 = new AskEntry();

	public AskEntry AskEntry5 = new AskEntry();

	public AskEntry AskEntry6 = new AskEntry();

	public AskEntry AskEntry7 = new AskEntry();

	public AskEntry AskEntry8 = new AskEntry();

	public AskEntry AskEntry9 = new AskEntry();

	public AskEntry AskEntry10 = new AskEntry();

	public AskEntry AskEntry11 = new AskEntry();

	public int Count => Length;

	public int Length => 12;

	[NotNull]
	public AskEntry this[int index]
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
				11 => AskEntry11, 
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
			case 11:
				AskEntry11 = value;
				break;
			}
		}
	}

	public IEnumerable<AskEntry> All
	{
		get
		{
			yield return AskEntry0 ?? (AskEntry0 = new AskEntry());
			yield return AskEntry1 ?? (AskEntry1 = new AskEntry());
			yield return AskEntry2 ?? (AskEntry2 = new AskEntry());
			yield return AskEntry3 ?? (AskEntry3 = new AskEntry());
			yield return AskEntry4 ?? (AskEntry4 = new AskEntry());
			yield return AskEntry5 ?? (AskEntry5 = new AskEntry());
			yield return AskEntry6 ?? (AskEntry6 = new AskEntry());
			yield return AskEntry7 ?? (AskEntry7 = new AskEntry());
			yield return AskEntry8 ?? (AskEntry8 = new AskEntry());
			yield return AskEntry9 ?? (AskEntry9 = new AskEntry());
			yield return AskEntry10 ?? (AskEntry10 = new AskEntry());
			yield return AskEntry11 ?? (AskEntry11 = new AskEntry());
		}
	}

	public IEnumerable<(int Index, AskEntry ask)> AllWithIndex => Enumerable.Range(0, Count).Zip(All, (int i, AskEntry a) => (i: i, a: a));

	public IEnumerator<AskEntry> GetEnumerator()
	{
		return All.Where((AskEntry i) => i != null).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
