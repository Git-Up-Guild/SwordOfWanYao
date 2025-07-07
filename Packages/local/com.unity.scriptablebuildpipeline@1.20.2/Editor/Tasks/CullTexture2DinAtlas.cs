using System;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.U2D;

namespace UnityEditor.Build.Pipeline.Tasks
{
    /// <summary>
    /// 修复SBP构建包含图集散图的资源包时，发生散图纹理冗余的Bug
    /// </summary>
    public class CullTexture2DinAtlas : IBuildTask
    {
        /// <inheritdoc />
        public int Version => 1;

        [InjectContext]
        IBundleWriteData writeDataParam;

        /// <inheritdoc />
        public ReturnCode Run()
        {
            BundleWriteData writeData = (BundleWriteData)writeDataParam;

            //所有图集散图的guid集合
            HashSet<GUID> spriteGuids = new HashSet<GUID>();

            //遍历资源包里的资源 记录其中图集的散图guid
            foreach (var pair in writeData.FileToObjects)
            {
                foreach (ObjectIdentifier objectIdentifier in pair.Value)
                {
                    string path = AssetDatabase.GUIDToAssetPath(objectIdentifier.guid);
                    //SpriteAtlas asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                    //if (asset !=null)
                    if(path.IndexOf(".spriteatlas")>=0)
                    {
                        string[] spritePaths = AssetDatabase.GetDependencies(path);
                        foreach (string spritePath in spritePaths)
                        {
                            GUID spriteGuild = AssetDatabase.GUIDFromAssetPath(spritePath);
                            if(spriteGuids.Contains(spriteGuild)==false)
                                spriteGuids.Add(spriteGuild);

                            //Debug.LogWarning(path + " 剔除：" + spritePath);
                        }

                       
                        
                    }

                    /*
                    if(asset!=null)
                    {
                        UnityEngine.Object.DestroyImmediate(asset);
                    }
                    */




                }
            }

            Resources.UnloadUnusedAssets();
            System.GC.Collect();


            HashSet<GUID> spriteObjsGuids = new HashSet<GUID>();


            //将writeData.FileToObjects包含的图集散图的texture删掉 避免冗余
            foreach (var pair in writeData.FileToObjects)
            {
                List<ObjectIdentifier> objectIdentifiers = pair.Value;

                //建立sprite的guid
                /*
                spriteObjsGuids.Clear();
                for (int i = objectIdentifiers.Count - 1; i >= 0; i--)
                {
                    ObjectIdentifier objectIdentifier = objectIdentifiers[i];
                    if (objectIdentifier.localIdentifierInFile == 21300000)
                    {
                        spriteObjsGuids.Add(objectIdentifier.guid);
                    }
                }
                */

                for (int i = objectIdentifiers.Count - 1; i >= 0; i--)
                {
                    ObjectIdentifier objectIdentifier = objectIdentifiers[i];

                    if (objectIdentifier.localIdentifierInFile == 2800000)
                    {
                        if (spriteGuids.Contains(objectIdentifier.guid))
                        {
                            //删除图集散图的冗余texture
                            objectIdentifiers.RemoveAt(i);
                        }
                        /*
                        else if (spriteObjsGuids.Contains(objectIdentifier.guid))
                        {
                            //删除图集散图的冗余texture
                            objectIdentifiers.RemoveAt(i);
                        }*/

                    }

                   
                }
            }

            return ReturnCode.Success;
        }
    }
}
