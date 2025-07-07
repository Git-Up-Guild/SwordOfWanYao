using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WXB
{
    // 缓存渲染元素
    public partial class RenderCache
    {
        IOwner mOwner;

        public RenderCache(IOwner st)
        {
            mOwner = st;
            materials = new List<Texture>();
        }

        //渲染数据列表
        private List<BaseRenderData> m_RenderDatas = new List<BaseRenderData>();

        public List<Texture> materials { get; protected set; }

        public void Release()
        {
            materials.Clear();

            BaseRenderData bd = null;
            for (int i = 0; i < m_RenderDatas.Count; ++i)
            {
                bd = m_RenderDatas[i];
                bd.Release();
                if (bd is TextRenderData)
                {
                    PoolData<TextRenderData>.Free((TextRenderData)bd);
                }
                else if (bd is SpriteRenderData)
                {
                    PoolData<SpriteRenderData>.Free((SpriteRenderData)bd);
                }
            }

            m_RenderDatas.Clear();
        }

        public void cacheText(Line l, TextNode n, string text, Rect rect)
        {
            TextRenderData td = PoolData<TextRenderData>.Get();
            td.Reset(n, text, rect, l);

            m_RenderDatas.Add(td);

            td.subMaterial = materials.IndexOf(n.d_font.material.mainTexture);
            if (td.subMaterial == -1)
            {
                td.subMaterial = materials.Count;
                materials.Add(n.d_font.material.mainTexture);
            }
        }

        public void cacheSprite(Line l, NodeBase n, Sprite sprite, Rect rect)
        {
            if (sprite != null)
            {
                SpriteRenderData sd = PoolData<SpriteRenderData>.Get();
                sd.Reset(n, sprite, rect, l);
                m_RenderDatas.Add(sd);

                sd.subMaterial = materials.IndexOf(sprite.texture);
                if (sd.subMaterial == -1)
                {
                    sd.subMaterial = materials.Count;
                    materials.Add(sprite.texture);
                }
            }
        }

        public void cacheCartoon(Line l, NodeBase n, Cartoon cartoon, Rect rect)
        {
            if (cartoon != null)
            {
                CartoonRenderData cd = PoolData<CartoonRenderData>.Get();
                cd.Reset(n, cartoon, rect, l);
                m_RenderDatas.Add(cd);
            }
        }

        public bool isEmpty
        {
            get { return m_RenderDatas.Count == 0; }
        }

        struct Key
        {
            public int subMaterial;
            public bool isBlink;
            public bool isOffset;
            public Rect offsetRect;

            public bool IsEquals(BaseRenderData bd)
            {
                return subMaterial == bd.subMaterial &&
                       isBlink == bd.node.d_bBlink && ((isOffset == false && bd.node.d_bOffset == false) ||
                       (isOffset == bd.node.d_bOffset && offsetRect == bd.node.d_rectOffset));
            }

            public List<BaseRenderData> nodes;

            public DrawType drawType
            {
                get
                {
                    if (isBlink)
                    {
                        if (isOffset)
                            return DrawType.OffsetAndAlpha;

                        return DrawType.Alpha;
                    }

                    if (isOffset)
                    {
                        return DrawType.Offset;
                    }

                    return DrawType.Default;
                }
            }

            public IDraw Get(IOwner owner, Texture texture)
            {
                long key = nodes[0].node.keyPrefix;
                key += texture.GetInstanceID();
                IDraw draw = owner.GetDraw(drawType, key,
                    (IDraw d, object p) =>
                    {
                        d.texture = texture;
                        if (d is OffsetDraw)
                        {
                            OffsetDraw od = d as OffsetDraw;
                            od.Set((Rect)p);
                        }
                    }, offsetRect);

                return draw;
            }
        }

        static List<Key> s_keys = new List<Key>();

        Vector2 DrawOffset;
        public void OnAlignByGeometry(ref Vector2 offset, float pixelsPerUnit, float firstHeight)
        {
            for (int m = 0; m < m_RenderDatas.Count; ++m)
            {
                if (m_RenderDatas[m].rect.y > firstHeight)
                    continue;

                if (!m_RenderDatas[m].isAlignByGeometry)
                {
                    offset = Vector2.zero;
                    return;
                }

                m_RenderDatas[m].OnAlignByGeometry(ref offset, pixelsPerUnit);
            }
        }

        // 行修正
        public void OnCheckLineY(float pixelsPerUnit)
        {
            for (int m = 0; m < m_RenderDatas.Count; ++m)
            {
                m_RenderDatas[m].OnLineYCheck(pixelsPerUnit);
            }
        }

        public void Render(VertexHelper vh, Rect rect, Vector2 offset, float pixelsPerUnit, Mesh workerMesh, Material defaultMaterial)
        {
            DrawOffset = offset /** pixelsPerUnit*/;
            s_keys.Clear();

            //对渲染数据进行分批，同样属性的放在一批，提高绘制性能
            for (int m = 0; m < m_RenderDatas.Count; ++m)
            {
                BaseRenderData bd = m_RenderDatas[m];
                int index = s_keys.FindIndex((Key k) =>
                {
                    return k.IsEquals(bd);
                });

                if (index == -1)
                {
                    Key k = new Key();
                    k.subMaterial = bd.subMaterial;
                    k.isOffset = bd.node.d_bOffset;
                    k.isBlink = bd.node.d_bBlink;
                    k.offsetRect = bd.node.d_rectOffset;
                    k.nodes = ListPool<BaseRenderData>.Get();
                    s_keys.Add(k);
                    k.nodes.Add(bd);
                }
                else
                {
                    s_keys[index].nodes.Add(bd);
                }
            }

            vh.Clear();

            for (int i = 0; i < s_keys.Count; ++i)
            {
                Key key = s_keys[i];
                for (int m = 0; m < key.nodes.Count; ++m)
                {
                    key.nodes[m].Render(vh, rect, offset, pixelsPerUnit);
                }

                if (vh.currentVertCount != 0)
                {
                    IDraw draw = key.Get(mOwner, materials[key.subMaterial]);
                    vh.FillMesh(workerMesh);
                    draw.FillMesh(workerMesh);
                    vh.Clear();
                }

                ListPool<BaseRenderData>.Release(key.nodes);
            }

            s_keys.Clear();
        }

        public BaseRenderData Get(Vector2 pos)
        {
            BaseRenderData bd = null;
            pos -= DrawOffset;
            for (int i = 0; i < m_RenderDatas.Count; ++i)
            {
                bd = m_RenderDatas[i];
                if (bd.isContain(pos))
                    return bd;
            }

            return null;
        }

        public void Get(List<BaseRenderData> bds, NodeBase nb)
        {
            BaseRenderData bd = null;
            for (int i = 0; i < m_RenderDatas.Count; ++i)
            {
                bd = m_RenderDatas[i];
                if (bd.node == nb)
                    bds.Add(bd);
            }
        }
    }
}