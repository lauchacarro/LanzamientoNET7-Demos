using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpDemo7
{
    public class Person
    {
        public Person() { }

        public required string FirstName { get; init; }
        public required string LastName { get; init; }

        public int? Age { get; set; }
    }

}
