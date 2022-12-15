using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpDemo7.GenericAttribute
{

    //https://learn.microsoft.com/es-es/dotnet/csharp/whats-new/csharp-11#generic-attributes
    public class DefaultValueAttribute<T> : Attribute
    {
        public T? GetValue() => default;
    }
}
