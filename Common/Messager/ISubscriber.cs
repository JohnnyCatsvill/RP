using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messager
{
    public interface ISubscriber
    {
        public void SetTimeout(int milliseconds) { }
        public void AddSubscription(string channel) { }
        public bool Recieve(ref string recievingString, ref string recivingChannel) { return false; }
    }
}
