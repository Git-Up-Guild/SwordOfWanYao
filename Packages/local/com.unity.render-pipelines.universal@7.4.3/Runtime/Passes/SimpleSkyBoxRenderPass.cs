/*******************************************************************
** 文件名:    SimpleSkyBoxRenderPass.cs
** 版  权:    (C) 冰川网络网络科技
** 创建人:    许德纪
** 日  期:    2022/10/28
** 版  本:    1.0
** 描  述:    简单的天空盒

**************************** 修改记录 ******************************
** 修改人:    
** 日  期:    
** 描  述:    
********************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    public class SimpleSkyBoxRenderPass : ScriptableRenderPass
    {
        private Material m_material;

        public SimpleSkyBoxRenderPass(RenderPassEvent evt, Material material)
        {
            renderPassEvent = evt;
            m_material = material;

#if UNITY_IOS
        m_material.SetColor("_Color", new Color(0,0,0,1));

#else
        m_material.SetColor("_Color", new Color(1, 1, 1, 1));
#endif

        }


        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            var cmd = CommandBufferPool.Get("SimpleSkyBox");

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_material);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);

        }
    }
}
