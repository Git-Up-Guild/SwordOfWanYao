using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Entity.Net
{
    public class TestNetEntity : NetEntity
    {
        //数据部件
        public TestNetEntityDataPart data => GetPart<TestNetEntityDataPart>();
    }
}
