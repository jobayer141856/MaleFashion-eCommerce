using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Exceptions
{
    public class DuplicateDataException : Exception
    {
        public DuplicateDataException(string message) : base(message)
        {
        }
    }
}
