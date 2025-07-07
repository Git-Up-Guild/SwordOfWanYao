using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UGUIBatchDepthAnalyzeBase;

public class UGUIBatchDepthAnalyzeResultReader
{
    /// <summary>
    /// 这两个数据相等代表应该合批
    /// </summary>
    public class BatchData
    {
        public int materialID;
        public int textureID;

        public bool IsEqual(BatchData other)
        {
            if (other != null)
            {
                return this.materialID == other.materialID
                    && this.textureID == other.textureID;
            }
            return false;
        }
        public BatchData(int matID, int texID)
        {
            this.materialID = matID;
            this.textureID = texID;
        }
    }
    /// <summary>
    /// BatchData 是MaterialID,TextureID相同的Key
    /// List 最终结果数量=1，不输出
    /// 添加时候：>1 直接加到后面
    /// = 1 与第一个判断相同，不加，不同，加
    /// < 1 直接加
    /// </summary>
    private Dictionary<BatchData, List<GameObjectData>> m_cacheData = new Dictionary<BatchData, List<GameObjectData>>();
    private StringBuilder outPut = new StringBuilder();
    public BatchData GetCacheDataKey(int materialID, int textureID)
    {
        foreach (var kv in m_cacheData)
        {
            if (kv.Key.materialID == materialID && kv.Key.textureID == textureID)
            {
                return kv.Key;
            }
        }
        return null;
    }
    public void Clear()
    {
        foreach (var kv in m_cacheData)
        {
            kv.Value.Clear();
        }
    }
    public void PrintUnBatchData(Dictionary<Transform, GameObjectData> dict)
    {
        Clear();
        foreach (var kv in dict)
        {
            BatchData key = GetCacheDataKey(kv.Value.materialID, kv.Value.textureID);
            if (key == null)
            {
                key = new BatchData(kv.Value.materialID, kv.Value.textureID);
                m_cacheData.Add(key, new List<GameObjectData>());
            }
            List<GameObjectData> output = m_cacheData[key];
            if (output.Count == 0)
                output.Add(kv.Value);
            else if (output.Count > 1)
                output.Add(kv.Value);
            else if (output.Count == 1)
            {
                //批次不等，相加
                if (output[0].batchID != kv.Value.batchID)
                    output.Add(kv.Value);
            }
        }
        outPut.Clear();
        //打印
        foreach (var kv in m_cacheData)
        {
            //一个的无冲突不用打印
            if (kv.Value.Count <= 1) continue; //上面有缓存数据
            outPut.Append($"材质ID:{kv.Key.materialID},贴图ID:{kv.Key.textureID}有如下合批不对:");
            for (int i = 0; i < kv.Value.Count; i++)
            {
                outPut.Append($"<color=yellow>{kv.Value[i].go.name},BatchID:{kv.Value[i].batchID}</color>");
                if (i < kv.Value.Count - 1)
                {
                    outPut.Append("-");
                }
            }
            outPut.AppendLine();
        }
        string result = outPut.ToString();
        if (result != string.Empty)
        {
            Debug.LogWarning(result);
        }
        outPut.Clear();
        Clear();
    }
}
