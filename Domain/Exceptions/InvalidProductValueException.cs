using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class InvalidProductValueException : Exception
    {
        public InvalidProductValueException(string message)
        {
            throw new Exception($"Valor inválido: {message}");
        }
    }
}
