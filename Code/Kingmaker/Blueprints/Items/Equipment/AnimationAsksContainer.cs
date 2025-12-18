using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Enums.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.Items.Equipment;

[Serializable]
[JsonObject]
public class AnimationAsksContainer : IReadOnlyList<AnimationAsk>, IEnumerable<AnimationAsk>, IEnumerable, IReadOnlyCollection<AnimationAsk>
{
	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry0 = new AnimationAsk(MappedAnimationEventType.Idle);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry1 = new AnimationAsk(MappedAnimationEventType.AttackShort);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry2 = new AnimationAsk(MappedAnimationEventType.AttackLong);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry3 = new AnimationAsk(MappedAnimationEventType.AttackCrit);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry4 = new AnimationAsk(MappedAnimationEventType.AttackSpecial);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry5 = new AnimationAsk(MappedAnimationEventType.Cast);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry6 = new AnimationAsk(MappedAnimationEventType.Stunned);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry7 = new AnimationAsk(MappedAnimationEventType.OutOfStun);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry8 = new AnimationAsk(MappedAnimationEventType.Sleep);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry9 = new AnimationAsk(MappedAnimationEventType.AlertSound1);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry10 = new AnimationAsk(MappedAnimationEventType.AlertSound2);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry11 = new AnimationAsk(MappedAnimationEventType.IdleCombat);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry12 = new AnimationAsk(MappedAnimationEventType.MoveVoice1);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry13 = new AnimationAsk(MappedAnimationEventType.MoveVoice2);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry14 = new AnimationAsk(MappedAnimationEventType.AttackBite);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry15 = new AnimationAsk(MappedAnimationEventType.AttackClaw);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry16 = new AnimationAsk(MappedAnimationEventType.AttackTail);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry17 = new AnimationAsk(MappedAnimationEventType.AttackWing);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry18 = new AnimationAsk(MappedAnimationEventType.CastShort);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry19 = new AnimationAsk(MappedAnimationEventType.CastLong);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry20 = new AnimationAsk(MappedAnimationEventType.Precast);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry21 = new AnimationAsk(MappedAnimationEventType.Dodge);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry22 = new AnimationAsk(MappedAnimationEventType.IdleMicro);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry23 = new AnimationAsk(MappedAnimationEventType.UseMagicItem);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry24 = new AnimationAsk(MappedAnimationEventType.CastTouch);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry25 = new AnimationAsk(MappedAnimationEventType.Omnicast);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry26 = new AnimationAsk(MappedAnimationEventType.OutOfSleep);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry27 = new AnimationAsk(MappedAnimationEventType.Move);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry28 = new AnimationAsk(MappedAnimationEventType.Run);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry29 = new AnimationAsk(MappedAnimationEventType.DeathFall);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry30 = new AnimationAsk(MappedAnimationEventType.DeathLying);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry31 = new AnimationAsk(MappedAnimationEventType.CastDirect);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry32 = new AnimationAsk(MappedAnimationEventType.CastYourself);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry33 = new AnimationAsk(MappedAnimationEventType.CoupDeGrace);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry34 = new AnimationAsk(MappedAnimationEventType.PainfulMoan);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry35 = new AnimationAsk(MappedAnimationEventType.VariantIdle1);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry36 = new AnimationAsk(MappedAnimationEventType.VariantIdle2);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry37 = new AnimationAsk(MappedAnimationEventType.VariantIdle3);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry38 = new AnimationAsk(MappedAnimationEventType.AttackPre1);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry39 = new AnimationAsk(MappedAnimationEventType.AttackPre2);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry40 = new AnimationAsk(MappedAnimationEventType.AttackPre3);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry41 = new AnimationAsk(MappedAnimationEventType.Idle2);

	[ProvideNameWithProperty("AnimationEvent")]
	public AnimationAsk AskEntry42 = new AnimationAsk(MappedAnimationEventType.Act);

	public int FirstEmptyIndex => All.IndexOf(All.ToList().FirstOrDefault((AnimationAsk x) => x.Entries.All((AskEntry y) => y.IsEmpty)));

	public int Count => Length;

	public int Length => All.Count();

	[NotNull]
	public AnimationAsk this[int index]
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
				12 => AskEntry12, 
				13 => AskEntry13, 
				14 => AskEntry14, 
				15 => AskEntry15, 
				16 => AskEntry16, 
				17 => AskEntry17, 
				18 => AskEntry18, 
				19 => AskEntry19, 
				20 => AskEntry20, 
				21 => AskEntry21, 
				22 => AskEntry22, 
				23 => AskEntry23, 
				24 => AskEntry24, 
				25 => AskEntry25, 
				26 => AskEntry26, 
				27 => AskEntry27, 
				28 => AskEntry28, 
				29 => AskEntry29, 
				30 => AskEntry30, 
				31 => AskEntry31, 
				32 => AskEntry32, 
				33 => AskEntry33, 
				34 => AskEntry34, 
				35 => AskEntry35, 
				36 => AskEntry36, 
				37 => AskEntry37, 
				38 => AskEntry38, 
				39 => AskEntry39, 
				40 => AskEntry40, 
				41 => AskEntry41, 
				42 => AskEntry42, 
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
			case 12:
				AskEntry12 = value;
				break;
			case 13:
				AskEntry13 = value;
				break;
			case 14:
				AskEntry14 = value;
				break;
			case 15:
				AskEntry15 = value;
				break;
			case 16:
				AskEntry16 = value;
				break;
			case 17:
				AskEntry17 = value;
				break;
			case 18:
				AskEntry18 = value;
				break;
			case 19:
				AskEntry19 = value;
				break;
			case 20:
				AskEntry20 = value;
				break;
			case 21:
				AskEntry21 = value;
				break;
			case 22:
				AskEntry22 = value;
				break;
			case 23:
				AskEntry23 = value;
				break;
			case 24:
				AskEntry24 = value;
				break;
			case 25:
				AskEntry25 = value;
				break;
			case 26:
				AskEntry9 = value;
				break;
			case 27:
				AskEntry10 = value;
				break;
			case 28:
				AskEntry11 = value;
				break;
			case 29:
				AskEntry12 = value;
				break;
			case 30:
				AskEntry13 = value;
				break;
			case 31:
				AskEntry14 = value;
				break;
			case 32:
				AskEntry15 = value;
				break;
			case 33:
				AskEntry16 = value;
				break;
			case 34:
				AskEntry17 = value;
				break;
			case 35:
				AskEntry18 = value;
				break;
			case 36:
				AskEntry19 = value;
				break;
			case 37:
				AskEntry20 = value;
				break;
			case 38:
				AskEntry21 = value;
				break;
			case 39:
				AskEntry22 = value;
				break;
			case 40:
				AskEntry23 = value;
				break;
			case 41:
				AskEntry24 = value;
				break;
			case 42:
				AskEntry25 = value;
				break;
			}
		}
	}

	public IEnumerable<AnimationAsk> All
	{
		get
		{
			yield return AskEntry0;
			yield return AskEntry1;
			yield return AskEntry2;
			yield return AskEntry3;
			yield return AskEntry4;
			yield return AskEntry5;
			yield return AskEntry6;
			yield return AskEntry7;
			yield return AskEntry8;
			yield return AskEntry9;
			yield return AskEntry10;
			yield return AskEntry11;
			yield return AskEntry12;
			yield return AskEntry13;
			yield return AskEntry14;
			yield return AskEntry15;
			yield return AskEntry16;
			yield return AskEntry17;
			yield return AskEntry18;
			yield return AskEntry19;
			yield return AskEntry20;
			yield return AskEntry21;
			yield return AskEntry22;
			yield return AskEntry23;
			yield return AskEntry24;
			yield return AskEntry25;
			yield return AskEntry26;
			yield return AskEntry27;
			yield return AskEntry28;
			yield return AskEntry29;
			yield return AskEntry30;
			yield return AskEntry31;
			yield return AskEntry32;
			yield return AskEntry33;
			yield return AskEntry34;
			yield return AskEntry35;
			yield return AskEntry36;
			yield return AskEntry37;
			yield return AskEntry38;
			yield return AskEntry39;
			yield return AskEntry40;
			yield return AskEntry41;
			yield return AskEntry42;
		}
	}

	public IEnumerable<(int Index, AnimationAsk ask)> AllWithIndex => Enumerable.Range(0, Count).Zip(All, (int i, AnimationAsk a) => (i: i, a: a));

	public IEnumerator<AnimationAsk> GetEnumerator()
	{
		return All.Where((AnimationAsk i) => i != null).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
