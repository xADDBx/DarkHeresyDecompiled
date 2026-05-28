using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class OwlTypeConverterAttribute : Attribute
{
}
