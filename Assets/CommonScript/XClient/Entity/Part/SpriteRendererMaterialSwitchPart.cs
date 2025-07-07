using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.MonoState;

namespace XClient.Entity
{
    public class SpriteRendererMaterialSwitchPart : BasePart
    {
        public SpriteRenderMaterialSwitcher materialSwitcher { get; private set; }
        private int m_materialIndex = 0;

        protected override void OnInit(object context)
        {
            base.OnInit(context);
            m_materialIndex = 0;
        }

        public void ChangeMaterial(int matIndex)
        {
            m_materialIndex = matIndex;

            if (materialSwitcher != null)
                materialSwitcher.Switch(m_materialIndex);
        }

        protected override void OnReset()
        {
            base.OnReset();

            m_materialIndex = 0;
            materialSwitcher?.SwitchDefault();
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);

            IPrefabResource res = data as IPrefabResource;

            switch (id)
            {
                case EntityMessageID.ResLoaded:
                    {
                        materialSwitcher = res?.gameObject.GetComponentInChildren<SpriteRenderMaterialSwitcher>();
                        materialSwitcher?.Switch(m_materialIndex);
                    }
                    break;
                case EntityMessageID.ResUnloaded:
                    {
                        materialSwitcher?.SwitchDefault();
                        materialSwitcher = null;
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
