using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messager
{
    public interface IPublisher
    {
        public void SetTimeout(int milliseconds) { }
        public bool Send(string channel, string message) { return false; }

    }
}
