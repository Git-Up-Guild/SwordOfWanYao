using gamepol;
using System.ComponentModel;
using XGame.Entity;

namespace XClient.Entity
{
    /// <summary>
    /// 玩家实体对象
    /// </summary>
    public class PersonCreateShareContext
    {
        public TRolePublicContext publicContext;
        public TRolePartPublicContext partContext;

        public void Reset()
        {
            publicContext = null;
            partContext = null;
        }

        private PersonCreateShareContext() { }
        public static PersonCreateShareContext instance = new PersonCreateShareContext();
    }

    /// <summary>
    /// 其他玩家实体对象
    /// </summary>
    public class PersonEntity : VisibleEntity
    {
        public override string GetResPath()
        {
            return "G_Resources/Artist/Prefabs/Role/Role001.prefab";
        }
    }

}
