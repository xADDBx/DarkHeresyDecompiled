using System;

namespace Kingmaker.EntitySystem.Stats.Base;

[AttributeUsage(AttributeTargets.Field)]
public sealed class NonModifiableStatAttribute : Attribute
{
}
