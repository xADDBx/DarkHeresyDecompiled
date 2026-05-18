using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Framework.ContextContract;

public readonly struct ContextContract : IEquatable<ContextContract>
{
	public ref struct Builder
	{
		private readonly Availability[] _fields;

		private bool _isCold;

		public static Builder New()
		{
			return new Builder(new Availability[_FieldCount], isCold: false);
		}

		private Builder(Availability[] fields, bool isCold)
		{
			_fields = fields;
			_isCold = isCold;
		}

		public Builder Set(ContextField field, Availability availability)
		{
			_fields[(int)field] = availability;
			return this;
		}

		public Builder Cold()
		{
			_isCold = true;
			return this;
		}

		public ContextContract Freeze()
		{
			return new ContextContract(_fields, _isCold);
		}
	}

	private static readonly int _FieldCount = EnumUtils.GetMaxValuePlusOne<ContextField>();

	private static readonly IReadOnlyDictionary<ContextField, ContextField[]> _DerivedFieldSources = new Dictionary<ContextField, ContextField[]>
	{
		{
			ContextField.AbilityWeapon,
			new ContextField[1] { ContextField.Ability }
		},
		{
			ContextField.AbilityBlueprint,
			new ContextField[1] { ContextField.Ability }
		},
		{
			ContextField.RuleInitiator,
			new ContextField[1] { ContextField.Rule }
		},
		{
			ContextField.RuleTarget,
			new ContextField[1] { ContextField.Rule }
		},
		{
			ContextField.CurrentTargetEntity,
			new ContextField[1] { ContextField.Target }
		},
		{
			ContextField.ContextMainTarget,
			new ContextField[1] { ContextField.ClickedTarget }
		},
		{
			ContextField.ContextCaster,
			new ContextField[1]
		},
		{
			ContextField.ContextOwner,
			new ContextField[1] { ContextField.Owner }
		},
		{
			ContextField.LosToClickedTarget,
			new ContextField[2]
			{
				ContextField.Caster,
				ContextField.ClickedTarget
			}
		},
		{
			ContextField.SourcePatternNodes,
			new ContextField[1] { ContextField.Pattern }
		}
	};

	private readonly Availability[] _fields;

	public bool IsColdContext { get; }

	public Availability this[ContextField field]
	{
		get
		{
			if (_fields == null)
			{
				return Availability.Never;
			}
			return _fields[(int)field];
		}
	}

	private ContextContract(Availability[] fields, bool isCold)
	{
		_fields = fields;
		IsColdContext = isCold;
	}

	public static ContextContract AllNever()
	{
		return new ContextContract(new Availability[_FieldCount], isCold: false);
	}

	public static ContextContract Cold()
	{
		return new ContextContract(new Availability[_FieldCount], isCold: true);
	}

	public ContextContract With(ContextField field, Availability availability)
	{
		Availability[] array = CopyFields();
		array[(int)field] = availability;
		return new ContextContract(array, IsColdContext);
	}

	public ContextContract Merge(ContextContract other)
	{
		Availability[] array = CopyFields();
		Availability[] fields = other._fields;
		if (fields != null)
		{
			for (int i = 0; i < _FieldCount; i++)
			{
				if ((int)fields[i] > (int)array[i])
				{
					array[i] = fields[i];
				}
			}
		}
		return new ContextContract(array, IsColdContext || other.IsColdContext);
	}

	public ContextContract Downgrade(ContextField field, Availability availability)
	{
		Availability[] array = CopyFields();
		if ((int)array[(int)field] > (int)availability)
		{
			array[(int)field] = availability;
		}
		return new ContextContract(array, IsColdContext);
	}

	public static IEnumerable<ContextField> ExpandDerived(ContextField field)
	{
		if (!_DerivedFieldSources.TryGetValue(field, out var value))
		{
			yield return field;
			yield break;
		}
		ContextField[] array = value;
		foreach (ContextField field2 in array)
		{
			foreach (ContextField item in ExpandDerived(field2))
			{
				yield return item;
			}
		}
	}

	private Availability[] CopyFields()
	{
		Availability[] array = new Availability[_FieldCount];
		if (_fields != null)
		{
			Array.Copy(_fields, array, _FieldCount);
		}
		return array;
	}

	public bool Equals(ContextContract other)
	{
		if (IsColdContext != other.IsColdContext)
		{
			return false;
		}
		Availability[] fields = _fields;
		Availability[] fields2 = other._fields;
		if (fields == null && fields2 == null)
		{
			return true;
		}
		if (fields == null || fields2 == null)
		{
			Availability[] array = fields ?? fields2;
			for (int i = 0; i < _FieldCount; i++)
			{
				if (array[i] != 0)
				{
					return false;
				}
			}
			return true;
		}
		for (int j = 0; j < _FieldCount; j++)
		{
			if (fields[j] != fields2[j])
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is ContextContract other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = (IsColdContext ? 1 : 0);
		if (_fields != null)
		{
			for (int i = 0; i < _FieldCount; i++)
			{
				num = (num * 397) ^ (int)_fields[i];
			}
		}
		return num;
	}

	public static bool operator ==(ContextContract a, ContextContract b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(ContextContract a, ContextContract b)
	{
		return !a.Equals(b);
	}
}
