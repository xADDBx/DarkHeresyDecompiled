using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Owlcat.UI.Lookup;

public interface ILookupTable : IComparer<Type>
{
	Task<MonoBehaviour> FirstOrDefault(Type type);
}
