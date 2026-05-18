using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public abstract class AbstractModifiersManager : IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	protected delegate void Visitor(Modifier modifier, Status status);

	protected enum Status : byte
	{
		Enabled,
		Filtered,
		StackSkipped
	}

	private const int DefaultCapacity = 16;

	private List<Modifier>? _list;

	public ReadonlyList<Modifier> List => _list;

	protected virtual bool KeepNonStackingModifiers => false;

	protected IEnumerable<Modifier> GetList(ModifierType type)
	{
		return _list?.Where((Modifier i) => i.Type == type) ?? Enumerable.Empty<Modifier>();
	}

	public bool Contains(Func<Modifier, bool> pred)
	{
		if (_list != null)
		{
			return _list.Contains(pred);
		}
		return false;
	}

	public Modifier? GetModifier(Func<Modifier, bool> pred)
	{
		int? num = _list?.FindIndex(pred);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault >= 0)
			{
				return _list[valueOrDefault];
			}
		}
		return null;
	}

	protected bool TryAdd(Modifier newModifier)
	{
		if (KeepNonStackingModifiers || newModifier.Stackable)
		{
			if (_list == null)
			{
				_list = new List<Modifier>(16);
			}
			_list.Add(newModifier);
			OnAdded(newModifier);
			return true;
		}
		for (int i = 0; i < List.Count; i++)
		{
			Modifier modifier = List[i];
			if (modifier.SameStack(newModifier))
			{
				if (!(newModifier.Positive ? (newModifier.Value > modifier.Value) : (newModifier.Value < modifier.Value)))
				{
					return false;
				}
				if (_list == null)
				{
					_list = new List<Modifier>(16);
				}
				_list[i] = newModifier;
				OnAdded(newModifier);
				return true;
			}
		}
		if (_list == null)
		{
			_list = new List<Modifier>(16);
		}
		_list.Add(newModifier);
		OnAdded(newModifier);
		return true;
	}

	public bool RemoveAll(Predicate<Modifier> match)
	{
		bool result = false;
		if (_list == null || _list.Empty())
		{
			return result;
		}
		for (int num = _list.Count - 1; num >= 0; num--)
		{
			Modifier modifier = _list[num];
			if (match(modifier))
			{
				_list.RemoveAt(num);
				OnRemoved(modifier);
				result = true;
			}
		}
		return result;
	}

	protected void Visit(out int valAdd, out float pctAdd, out float pctMul, out int valAddExtra, out float pctMulExtra, Visitor? visitor = null, Func<Modifier, bool>? filter = null)
	{
		valAdd = 0;
		pctAdd = 0f;
		pctMul = 1f;
		valAddExtra = 0;
		pctMulExtra = 1f;
		if (_list == null)
		{
			return;
		}
		Span<Status?> status = stackalloc Status?[_list.Count];
		for (int i = 0; i < status.Length; i++)
		{
			Modifier arg = _list[i];
			status[i] = ((filter != null && !filter(arg)) ? new Status?(Status.Filtered) : ((!KeepNonStackingModifiers || arg.Stackable) ? new Status?(Status.Enabled) : null));
		}
		if (KeepNonStackingModifiers)
		{
			ResolveStacks(status);
		}
		for (int j = 0; j < _list.Count; j++)
		{
			Status value = status[j].Value;
			visitor?.Invoke(_list[j], value);
			if (value == Status.Enabled)
			{
				Modifier modifier = _list[j];
				switch (modifier.Type)
				{
				case ModifierType.ValAdd:
					valAdd += modifier.Value;
					break;
				case ModifierType.PctAdd:
					pctAdd += (float)modifier.Value / 100f;
					break;
				case ModifierType.PctMul:
					pctMul *= (float)modifier.Value / 100f;
					break;
				case ModifierType.ValAdd_Extra:
					valAddExtra += modifier.Value;
					break;
				case ModifierType.PctMul_Extra:
					pctMulExtra *= (float)modifier.Value / 100f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	private void ResolveStacks(Span<Status?> status)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			if (status[i].HasValue)
			{
				continue;
			}
			status[i] = Status.Enabled;
			Modifier modifier = _list[i];
			int index = i;
			int value = modifier.Value;
			bool positive = modifier.Positive;
			for (int j = i + 1; j < _list.Count; j++)
			{
				if (status[j].HasValue)
				{
					continue;
				}
				Modifier other = _list[j];
				if (!modifier.StacksWith(other))
				{
					if (positive ? (other.Value > value) : (other.Value < value))
					{
						status[index] = Status.StackSkipped;
						status[j] = Status.Enabled;
						index = j;
						value = other.Value;
					}
					else
					{
						status[j] = Status.StackSkipped;
					}
				}
			}
		}
	}

	protected void Visit(Visitor visitor, Func<Modifier, bool>? filter = null)
	{
		Visit(out var _, out var _, out var _, out var _, out var _, visitor, filter);
	}

	public void GetValues(out int valAdd, out float pctAdd, out float pctMul, out int valAddExtra, out float pctMulExtra, Func<Modifier, bool>? filter = null)
	{
		Visit(out valAdd, out pctAdd, out pctMul, out valAddExtra, out pctMulExtra, null, filter);
	}

	public void Clear()
	{
		_list?.Clear();
	}

	protected void Sort()
	{
		_list?.Sort(Modifier.ValueComparer);
	}

	public void CopyFrom(IReadonlyModifiers? other, Func<Modifier, bool>? pred = null)
	{
		if (other == null || other.List.Count < 1)
		{
			return;
		}
		foreach (Modifier item in other.List)
		{
			if (pred == null || pred(item))
			{
				TryAdd(item);
			}
		}
	}

	protected virtual void OnAdded(Modifier modifier)
	{
	}

	protected virtual void OnRemoved(Modifier modifier)
	{
	}

	public IEnumerator<Modifier> GetEnumerator()
	{
		List<Modifier>.Enumerator? enumerator = _list?.GetEnumerator();
		if (!enumerator.HasValue)
		{
			return Enumerable.Empty<Modifier>().GetEnumerator();
		}
		return enumerator.GetValueOrDefault();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
