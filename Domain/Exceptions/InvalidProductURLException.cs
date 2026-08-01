using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class InvalidProductURLException : Exception
    {
        public InvalidProductURLException(string message): base(message) { }
    }
}
