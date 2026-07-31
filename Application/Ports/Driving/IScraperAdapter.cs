using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Ports.Driving
{
    public interface IScraperAdapter
    {
        void SendProduct(string productUrl);
    }
}
