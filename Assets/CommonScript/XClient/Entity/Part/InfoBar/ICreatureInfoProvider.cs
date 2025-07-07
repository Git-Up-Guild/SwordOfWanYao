using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Entity.Part
{
    /// <summary>
    /// 生物信息提供者，可以让实体实现这个接口，从而支持实体信息显示
    /// </summary>
    public interface ICreatureInfoProvider
    {
        string GetCreatureName();
        int GetCreatureMaxHp();
        int GetCreatureCurHp();
    }
}
