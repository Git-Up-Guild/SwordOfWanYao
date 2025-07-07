
namespace XClient.Network
{
    /// <summary>
    /// 网络数据对象序列化接口
    /// </summary>
    public interface INetableSerializer
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isDirtyOnly"></param>
        /// <returns></returns>
        bool Serializer(INetable obj, bool isDirtyOnly = true);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Unserializer(INetable obj);

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isDirtyOnly"></param>
        /// <returns></returns>
        bool RemoteValueDeltaSerializer(INetable obj, bool isDirtyOnly = true);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool RemoteValueDeltaUnserializer(INetable obj);
    }
}
