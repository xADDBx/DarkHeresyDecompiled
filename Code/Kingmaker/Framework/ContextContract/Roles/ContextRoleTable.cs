using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Framework.ContextContract.Roles;

public readonly struct ContextRoleTable
{
	private static readonly int _FieldCount = EnumUtils.GetMaxValuePlusOne<ContextField>();

	[CanBeNull]
	private readonly ContextRoleHint[] _hints;

	[CanBeNull]
	private readonly Dictionary<string, ContextRoleHint[]> _byFieldName;

	public ContextRoleHint this[ContextField field]
	{
		get
		{
			if (_hints == null)
			{
				return ContextRoleHint.Empty;
			}
			return _hints[(int)field];
		}
	}

	public static ContextRoleTable Empty => new ContextRoleTable(null, null);

	public IEnumerable<ContextField> NonEmptyFields
	{
		get
		{
			if (_hints == null)
			{
				yield break;
			}
			for (int i = 0; i < _FieldCount; i++)
			{
				if (_hints[i].HasContent)
				{
					yield return (ContextField)i;
				}
			}
		}
	}

	private ContextRoleTable([CanBeNull] ContextRoleHint[] hints, [CanBeNull] Dictionary<string, ContextRoleHint[]> byFieldName)
	{
		_hints = hints;
		_byFieldName = byFieldName;
	}

	public ContextRoleTable With(ContextField field, ContextRoleHint hint)
	{
		ContextRoleHint[] array = CopyHints();
		array[(int)field] = hint;
		return new ContextRoleTable(array, CopyByFieldName());
	}

	public ContextRoleTable WithForField([NotNull] string fieldName, ContextField field, ContextRoleHint hint)
	{
		Dictionary<string, ContextRoleHint[]> dictionary = ((_byFieldName == null) ? new Dictionary<string, ContextRoleHint[]>() : new Dictionary<string, ContextRoleHint[]>(_byFieldName));
		if (!dictionary.TryGetValue(fieldName, out var value) || value == null)
		{
			value = (dictionary[fieldName] = new ContextRoleHint[_FieldCount]);
		}
		else
		{
			ContextRoleHint[] array2 = new ContextRoleHint[_FieldCount];
			for (int i = 0; i < _FieldCount; i++)
			{
				array2[i] = value[i];
			}
			value = (dictionary[fieldName] = array2);
		}
		value[(int)field] = hint;
		return new ContextRoleTable(CopyHints(), dictionary);
	}

	public ContextRoleTable EnterField([NotNull] string fieldName)
	{
		if (_byFieldName == null || !_byFieldName.TryGetValue(fieldName, out var value) || value == null)
		{
			return new ContextRoleTable(CopyHints(), null);
		}
		ContextRoleHint[] array = CopyHints();
		for (int i = 0; i < _FieldCount; i++)
		{
			if (value[i].HasContent)
			{
				array[i] = value[i];
			}
		}
		return new ContextRoleTable(array, null);
	}

	[NotNull]
	private ContextRoleHint[] CopyHints()
	{
		ContextRoleHint[] array = new ContextRoleHint[_FieldCount];
		if (_hints != null)
		{
			for (int i = 0; i < _FieldCount; i++)
			{
				array[i] = _hints[i];
			}
		}
		return array;
	}

	[CanBeNull]
	private Dictionary<string, ContextRoleHint[]> CopyByFieldName()
	{
		if (_byFieldName == null)
		{
			return null;
		}
		return new Dictionary<string, ContextRoleHint[]>(_byFieldName);
	}
}
