using System;

namespace Framework.Utility.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
public sealed class InspectorExpandedByDefaultAttribute : Attribute
{
}
