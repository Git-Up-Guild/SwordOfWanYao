using gamepol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Client
{
    class RoomPropertySet
    {
        public long Version
        {
            get;set;
        }

        private Dictionary<int, long> m_dicProperty = new Dictionary<int, long>();

        public RoomPropertySet()
        {
            Version = 0;
        }

        public void BatchUpdate(TPropertySet newData)
        {
            int number = newData.get_iNum();
            int [] arrID = newData.get_arrID();
            TAbsValue[] arrVal = newData.get_arrVal();
            for (int i = 0; i < number; ++i)
            {
                int propID = arrID[i];
                TAbsValue absValue = arrVal[i];
                if (!m_dicProperty.TryAdd(propID, absValue.get_iVal()))
                {
                    m_dicProperty[propID] = absValue.get_iVal();
                }
            }
        }

        public Dictionary<int, long> GetAllProperty()
        {
            return m_dicProperty;
        }

        public long GetProperty(int id)
        {
            long val;
            if (m_dicProperty.TryGetValue(id, out val))
            {
                return val;
            }
            return 0;
        }

        public void SetProperty(int id, long val)
        {
            
            if (!m_dicProperty.TryAdd(id, val))
            {
                m_dicProperty[id] = val;
            }
            
        }

        public void SetProperty(int[] arrID, long[] arrVal, int iLen)
        {
            for (int i = 0; i < iLen; ++i)
            {
                if (!m_dicProperty.TryAdd(arrID[i], arrVal[i]))
                {
                    m_dicProperty[arrID[i]] = arrVal[i];
                }
            }

            

        }
    }
    
}
