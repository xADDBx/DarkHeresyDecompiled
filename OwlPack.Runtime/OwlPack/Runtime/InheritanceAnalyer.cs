using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OwlPack.Runtime;

public static class InheritanceAnalyer
{
	public readonly struct Error
	{
		public readonly string ParentClassName;

		public readonly string DerivedClassName;

		public Error(string derived, string parent)
		{
			ParentClassName = parent;
			DerivedClassName = derived;
		}
	}

	public static List<Error> Analyze(Assembly assembly = null)
	{
		List<Assembly> list = new List<Assembly>();
		if (assembly != null)
		{
			list.Add(assembly);
		}
		else
		{
			string currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly2 in assemblies)
			{
				if (!assembly2.FullName.Contains("-Editor") && assembly2.GetReferencedAssemblies().Any((AssemblyName n) => n.Name == currentAssemblyName))
				{
					list.Add(assembly2);
				}
			}
		}
		List<Error> list2 = new List<Error>();
		foreach (Assembly item in list)
		{
			foreach (Type item2 in (from t in item.GetTypes()
				where t.CustomAttributes == null || !t.CustomAttributes.Any((CustomAttributeData ca) => ca.AttributeType == typeof(OwlPackableAttribute))
				select t).ToList())
			{
				if (item2.BaseType.CustomAttributes.Any((CustomAttributeData ca) => ca.AttributeType == typeof(OwlPackableAttribute)))
				{
					list2.Add(new Error(item2.FullName, item2.BaseType.FullName));
					continue;
				}
				Type type = item2.GetInterfaces().FirstOrDefault((Type iface) => iface.GetCustomAttributes<OwlPackableAttribute>().FirstOrDefault() != null);
				if (type != null)
				{
					list2.Add(new Error(item2.FullName, type.FullName));
				}
			}
		}
		return list2;
	}
}
