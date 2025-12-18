using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

public class DelayUnityObjectDeserializationContractResolver : FieldsContractResolver
{
	private class ObjectValueProvider : IValueProvider
	{
		private readonly FieldOrPropertyInfo m_Field;

		public ObjectValueProvider(Type t, string field)
		{
			m_Field = new FieldOrPropertyInfo(t, field);
		}

		public object GetValue(object target)
		{
			return m_Field.GetValue(target);
		}

		public void SetValue(object target, object value)
		{
			(string, long)? tuple = value as (string, long)?;
			if (tuple.HasValue)
			{
				lock (s_DelayedFields)
				{
					s_DelayedFields.Add((target, m_Field), (tuple.Value.Item1, tuple.Value.Item2));
				}
				value = null;
			}
			m_Field.SetValue(target, value);
		}
	}

	private readonly struct FieldOrPropertyInfo
	{
		private readonly FieldInfo m_FieldInfo;

		private readonly PropertyInfo m_PropertyInfo;

		private void FindInfo(Type t, string name, out FieldInfo fieldInfo, out PropertyInfo propertyInfo)
		{
			fieldInfo = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			propertyInfo = ((m_FieldInfo == null) ? t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : null);
			if (!(fieldInfo != null) && !(propertyInfo != null) && !(t.BaseType == null))
			{
				FindInfo(t.BaseType, name, out fieldInfo, out propertyInfo);
			}
		}

		public FieldOrPropertyInfo(Type t, string name)
		{
			m_FieldInfo = null;
			m_PropertyInfo = null;
			FindInfo(t, name, out m_FieldInfo, out m_PropertyInfo);
			if (m_FieldInfo == null && m_PropertyInfo == null)
			{
				throw new Exception("Cannot find property or field " + name + " in type " + t.FullName);
			}
		}

		public object GetValue(object obj)
		{
			if (m_FieldInfo != null)
			{
				return m_FieldInfo.GetValue(obj);
			}
			if (m_PropertyInfo != null)
			{
				m_PropertyInfo.GetValue(obj);
			}
			return null;
		}

		public void SetValue(object obj, object value)
		{
			if (m_FieldInfo != null)
			{
				m_FieldInfo.SetValue(obj, value);
			}
			else if (m_PropertyInfo != null)
			{
				m_PropertyInfo.SetValue(obj, value);
			}
		}
	}

	private static Dictionary<(object obj, FieldOrPropertyInfo field), (string guid, long fileId)?> s_DelayedFields = new Dictionary<(object, FieldOrPropertyInfo), (string, long)?>();

	private static Dictionary<(Type type, string), ObjectValueProvider> s_ProviderCache = new Dictionary<(Type, string), ObjectValueProvider>();

	protected override JsonContract CreateContract(Type objectType)
	{
		return base.CreateContract(objectType);
	}
}
