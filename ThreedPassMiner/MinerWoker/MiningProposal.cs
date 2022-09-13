using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreedPassMiner
{
    internal class MiningProposal
    {
        public int id;
        public string pre_obj;
        public string raw;

        public MiningProposal(int _id, string _pre_obj, string _raw)
        {
            id      = _id;
            pre_obj = _pre_obj;
            raw     = _raw;
        }
    }
}
