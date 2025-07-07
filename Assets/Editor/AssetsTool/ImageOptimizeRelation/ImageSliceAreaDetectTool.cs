using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public class ImageSliceAreaDetectTool : EditorWindow
{

    [MenuItem("XGame/资源工具/图片拉伸检测工具(九宫格)")]
    static void ShowWindow()
    {
        CreateWindow<ImageSliceAreaDetectTool>("图片拉伸检测工具");
    }


    string m_ImageDir;//检查预制体路径
    string m_PrefabPath;//检查预制体路径
    string m_CSVFilesPath;//csv目录
    string[] m_allImageFiles;//所有图片文件目录
    List<ImageSliceData> m_Image2SamePixelAreaList;//图片路径 对应 图片相同像素区域 字典

    List<string> m_ChangeReadableImageList;//更改过可读性的图片列表
    List<string> m_ChangeSourceImageList;//更改过源文件的图片列表
    HashSet<string> m_ChangeImageTypePrefabSet;//更改ImageType的预制体集合


    Dictionary<string, List<GameObject>> m_imageFiles2PrefabListDict;//图片路径 对应 预制体字典
    string m_detectTextureName;

    private void OnEnable()
    {
        m_PrefabPath = $"{Application.dataPath}/Game/ImmortalFamily/GameResources/Prefabs/UI/";
        m_ImageDir = "Assets/Game/ImmortalFamily/GameResources/Artist/UI";
        m_allImageFiles = new string[0];
        m_Image2SamePixelAreaList = new List<ImageSliceData>();
    }

    private void OnGUI()
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            m_PrefabPath = EditorGUILayout.TextField("检测预制体文件夹", m_PrefabPath);
            if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                m_PrefabPath = EditorUtility.OpenFolderPanel(name, m_PrefabPath, "");
            }
        }

        /*
        using (var hor = new GUILayout.HorizontalScope())
        {
            m_ImageDir = EditorGUILayout.TextField("检测图片文件夹", m_ImageDir);
            if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                m_ImageDir = EditorUtility.OpenFolderPanel(name, m_ImageDir, "");
            }
        }
        */

        if (GUILayout.Button("获取图片&设置图片对应预制体字典（1）"))
        {
            SetupImage2PrefabDict(m_PrefabPath);
            SetupImagePathArray();
        }
        if(m_allImageFiles.Length > 0)
        {
            m_detectTextureName = GUILayout.TextField(m_detectTextureName);
        }

        if(m_allImageFiles.Length > 0 && GUILayout.Button("设置图片可读性（2）"))
        {
            SetupImageFileReadable(m_allImageFiles);
        }
        if (m_allImageFiles.Length > 0 && GUILayout.Button("计算图片可拉伸范围 & 修改图片源文件 & 修改预制体组件对应类型（3）"))
        {
            SetupImageFileRectData(m_allImageFiles);
            SetupSourceImage(m_Image2SamePixelAreaList);
            UpdatePrefabImageType(m_ChangeSourceImageList);
        }
        if (m_Image2SamePixelAreaList.Count > 0)
        {

            using (var hor = new GUILayout.HorizontalScope())
            {
                m_CSVFilesPath = EditorGUILayout.TextField("CSV 输出文件目录", m_CSVFilesPath);
                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    m_CSVFilesPath = EditorUtility.OpenFolderPanel(name, m_CSVFilesPath, "");
                }
                if (GUILayout.Button("写入", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    WriteImageSliceInfoIntoCSV(m_CSVFilesPath);
                    WriteChangeSourceImageInfoIntoTXT(m_CSVFilesPath);
                    WriteChangeImageTypePrefabIntoTXT(m_CSVFilesPath);
                }
            }

        }
        if (m_allImageFiles.Length > 0 && GUILayout.Button("重置修改图片的可读性（5）"))
        {
            RevertImageReadable(m_ChangeReadableImageList);
        }
    }


    void SetupImagePathArray()
    {
        m_allImageFiles = new string[m_imageFiles2PrefabListDict.Count];
        int i = -1;
        foreach (var item in m_imageFiles2PrefabListDict)
        {
            if(item.Key.IndexOf(m_ImageDir)<0)
            {
                continue;
            }

            m_allImageFiles[++i] = item.Key;
        }
        

    }

    void SetupImage2PrefabDict(string prefabsPath)
    {
        m_imageFiles2PrefabListDict = ImageOptimizeUtility.GetPrefasImageDependData(prefabsPath);
    }



    /// <summary>
    /// 设置图片可读性
    /// </summary>
    /// <param name="allimageFiles"></param>
    void SetupImageFileReadable(string[] allimageFiles)
    {
        m_ChangeReadableImageList = new List<string>();
        for (int i = 0; i < allimageFiles.Length; i++)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(allimageFiles[i]);
            if(texture && !texture.isReadable)
            {
                
                string[] mataFile = File.ReadAllLines($"{allimageFiles[i]}.meta");
                for (int j = 20; j < mataFile.Length; j++)//为什么要从20开始呢？因为可读性信息一般在20行后面，这样可以减少计算
                {
                    if (mataFile[j] == "  isReadable: 0")
                    {
                        mataFile[j] =  "  isReadable: 1";
                        m_ChangeReadableImageList.Add(allimageFiles[i]);
                        break;
                    }
                }
                File.WriteAllLines($"{allimageFiles[i]}.meta", mataFile);
            }

        }
        EditorUtility.DisplayProgressBar("刷新中...","",0f);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 还原 被修改 可读性 的 图片
    /// </summary>
    /// <param name="changeimageFiles"></param>
    void RevertImageReadable(List<string> changeimageFiles)
    {
        int i = 0;
        foreach (var item in changeimageFiles)
        {
            EditorUtility.DisplayProgressBar("重置图片可读性", item, 1f * i / changeimageFiles.Count);
            string[] mataFile = File.ReadAllLines($"{item}.meta");
            for (int j = 20; j < mataFile.Length; j++)//为什么要从20开始呢？因为可读性信息一般在20行后面，这样可以减少计算
            {
                if (mataFile[j] == "  isReadable: 1")
                {
                    mataFile[j] = "  isReadable: 0";
                    break;
                }
            }
            File.WriteAllLines($"{item}.meta", mataFile);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    /// <summary>
    /// 根据 图片压缩信息 创建新的图片文件
    /// </summary>
    void CreateNewImage(ImageSliceData imageSliceData)
    {
        Color[] newCol = null;
        int width = 0;
        int height = 0;

        int midColumn = imageSliceData.right - imageSliceData.left;
        int midRow = imageSliceData.up - imageSliceData.down;

        if (midColumn <= 0 && midRow > 0)
        {
            newCol = GetColorBlockWithoutHorizontalSlice(imageSliceData, out width, out height);
        }
        else if (midColumn > 0 && midRow <= 0)
        {
            newCol = GetColorBlockWithoutVerticalSlice(imageSliceData, out width, out height);
        }
        else if (midColumn > 0 && midRow > 0)
        {
            newCol = GetColorBlock(imageSliceData , out width ,out height);
        }

        if(newCol != null)
        {
            var newTex = new Texture2D(width, height);
            newTex.SetPixels(newCol);
            newTex.Apply();
            byte[] fileByteArr = newTex.EncodeToPNG();
            


            File.Delete(imageSliceData.path);
            var fs = File.Create(imageSliceData.path);
            fs.Write(fileByteArr, 0, fileByteArr.Length);
            fs.Flush();
            fs.Close();

        }

    }


    Color[,] GetPixelBlock(Texture2D texture , int x , int y , int width , int height)
    {
        //if (texture == null)
        //    return null;

        Color[] source = texture.GetPixels(x, y, width, height);
        Color[,] result = new Color[height, width];



        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                result[i, j] = source[i * width + j];
            }
        }
        return result;
    }

    /// <summary>
    /// 根据 图片拉伸数据 获取 新图片颜色数组（左右上下可拉伸）
    /// </summary>
    /// <param name="imageSliceData">图片拉伸数据</param>
    /// <param name="width">新图片的宽</param>
    /// <param name="height">新图片的高</param>
    /// <returns></returns>
    Color[] GetColorBlock(ImageSliceData imageSliceData , out int width , out int height)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imageSliceData.path);

        if (!texture.isReadable)
        {
            Debug.LogError($"{imageSliceData.path} 图片文件 仍未可读");
            width = 0;
            height = 0;
            return null ;
        }

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int leftIndex = imageSliceData.left;
        int rightIndex = imageSliceData.right;
        int upIndex = imageSliceData.up;
        int downIndex = imageSliceData.down;

        //图片 像素块
        Color[,] leftUp, up, rightUp;
        Color[,] left, mid, right;
        Color[,] leftDown, down, rightDown;


        leftUp = GetPixelBlock(texture, 0, upIndex, leftIndex, textureHeight - upIndex);
        left = GetPixelBlock(texture, 0, downIndex, leftIndex, 1);
        leftDown = GetPixelBlock(texture, 0, 0, leftIndex, downIndex);

        up = GetPixelBlock(texture, leftIndex, upIndex, 1, textureHeight - upIndex);
        mid = GetPixelBlock(texture, leftIndex, downIndex, 1, 1);
        down = GetPixelBlock(texture, leftIndex, 0, 1, downIndex);

        rightUp = GetPixelBlock(texture, rightIndex, upIndex, textureWidth - rightIndex, textureHeight - upIndex);
        right = GetPixelBlock(texture, rightIndex, downIndex, textureWidth - rightIndex, 1);
        rightDown = GetPixelBlock(texture, rightIndex, 0, textureWidth - rightIndex, downIndex);


        //Combine
        width = leftIndex + (textureWidth - rightIndex) + 1;
        height = downIndex + (textureHeight - upIndex) + 1;
        Color[] newCol = new Color[width * height];




        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (i < downIndex)
                {
                    if (j < leftIndex)
                    {
                        newCol[i * width + j] = leftDown[i, j];
                    }
                    else if (j < leftIndex + 1)
                    {
                        newCol[i * width + j] = down[i, j - leftIndex];
                    }
                    else
                    {
                        newCol[i * width + j] = rightDown[i, j - leftIndex - 1];
                    }
                }
                else if (i < downIndex + 1)
                {
                    if (j < leftIndex)
                    {
                        newCol[i * width + j] = left[i - downIndex, j];
                    }
                    else if (j < leftIndex + 1)
                    {
                        newCol[i * width + j] = mid[i - downIndex, j - leftIndex];
                    }
                    else
                    {
                        newCol[i * width + j] = right[i - downIndex, j - leftIndex - 1];
                    }
                }
                else
                {
                    if (j < leftIndex)
                    {
                        newCol[i * width + j] = leftUp[i - downIndex - 1, j];
                    }
                    else if (j < leftIndex + 1)
                    {
                        newCol[i * width + j] = up[i - downIndex - 1, j - leftIndex];
                    }
                    else
                    {
                        newCol[i * width + j] = rightUp[i - downIndex - 1, j - leftIndex - 1];
                    }
                }
            }
        }

        return newCol;
    }

    /// <summary>
    /// 根据 图片拉伸数据 获取 新图片颜色数组（仅上下可拉伸）
    /// </summary>
    /// <param name="imageSliceData">图片拉伸数据</param>
    /// <param name="width">新图片的宽</param>
    /// <param name="height">新图片的高</param>
    /// <returns></returns>
    Color[] GetColorBlockWithoutHorizontalSlice(ImageSliceData imageSliceData, out int width, out int height)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imageSliceData.path);

        if (!texture.isReadable)
        {
            Debug.LogError($"{imageSliceData.path} 图片文件 仍未可读");
            width = 0;
            height = 0;
            return null;
        }

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int upIndex = imageSliceData.up;
        int downIndex = imageSliceData.down;

        //图片 像素块
        Color[,] up;
        Color[,] mid;
        Color[,] down;

        up =        GetPixelBlock(texture, 0,   upIndex,    textureWidth,   textureHeight - upIndex);
        mid =       GetPixelBlock(texture, 0,   downIndex,  textureWidth,   1);
        down =      GetPixelBlock(texture, 0,   0,          textureWidth,   downIndex);


        //Combine
        width = textureWidth;
        height = downIndex + (textureHeight - upIndex) + 1;
        Color[] newCol = new Color[width * height];


       

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (i < downIndex)
                {
                    newCol[i * width + j] = down[i, j];
                }
                else if (i < downIndex + 1)
                {
                    newCol[i * width + j] = mid[i - downIndex, j];
                }
                else
                {
                    newCol[i * width + j] = up[i - downIndex - 1, j];
                }
            }
        }

        return newCol;
    }

    /// <summary>
    /// 根据 图片拉伸数据 获取 新图片颜色数组（仅左右可拉伸）
    /// </summary>
    /// <param name="imageSliceData">图片拉伸数据</param>
    /// <param name="width">新图片的宽</param>
    /// <param name="height">新图片的高</param>
    /// <returns></returns>
    Color[] GetColorBlockWithoutVerticalSlice(ImageSliceData imageSliceData, out int width, out int height)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imageSliceData.path);

        if (!texture.isReadable)
        {
            Debug.LogError($"{imageSliceData.path} 图片文件 仍未可读");
            width = 0;
            height = 0;
            return null;
        }

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int leftIndex = imageSliceData.left;
        int rightIndex = imageSliceData.right;

        //图片 像素块
        Color[,] left, mid, right;

        left =      GetPixelBlock(texture, 0,           0, leftIndex,                       textureHeight);
        mid =       GetPixelBlock(texture, leftIndex,   0, 1,                               textureHeight);
        right =     GetPixelBlock(texture, rightIndex,  0, textureWidth - rightIndex,       textureHeight);


        //Combine
        width = leftIndex + (textureWidth - rightIndex) + 1;
        height = textureHeight;
        Color[] newCol = new Color[width * height];




        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (j < leftIndex)
                {
                    newCol[i * width + j] = left[i, j];
                }
                else if (j < leftIndex + 1)
                {
                    newCol[i * width + j] = mid[i, j - leftIndex];
                }
                else
                {
                    newCol[i * width + j] = right[i, j - leftIndex - 1];
                }
            }
        }

        return newCol;
    }

    Color32 Color2Color32(Color color)
    {

        Color32 result = new Color32()
        {
            r = (byte)(color.r * 255),
            g = (byte)(color.g * 255),
            b = (byte)(color.b * 255),
            a = (byte)(color.a * 255),
        };
        return result;
    }

    /// <summary>
    /// 设置图片的九宫格数据
    /// </summary>
    /// <param name="imageSliceData"></param>
    void SetupImageFileSlice(ImageSliceData imageSliceData)
    {
        //  spriteBorder: {x: (左), y: (下), z: (右), w: (上)} 
        var metaFile = File.ReadAllLines($"{imageSliceData.path}.meta");
        for (int i = 45; i < metaFile.Length; i++)//为什么要从45开始呢？因为可读性信息一般在45行后面，这样可以减少计算
        {
            if(metaFile[i].Contains("spriteBorder"))
            {
                int L = imageSliceData.left;
                int R = imageSliceData.width - imageSliceData.right;
                int T = imageSliceData.height - imageSliceData.up;
                int B = imageSliceData.down;
                metaFile[i] = $"  spriteBorder: {{x: {L}, y: {B}, z: {R}, w: {T}}} ";
                break;
            }
        }
        File.WriteAllLines($"{imageSliceData.path}.meta", metaFile);
    }



    /// <summary>
    /// 设置图片对应拉伸数据
    /// </summary>
    /// <param name="allimageFiles"></param>
    void SetupImageFileRectData(string[] allimageFiles)
    {
        for (int i = 0; i < allimageFiles.Length; i++)
        {
            if (allimageFiles[i]==null)
            {
                continue;
            }

            EditorUtility.DisplayProgressBar("计算图片拉伸性", allimageFiles[i] , 1f * i / allimageFiles.Length);
            try
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(allimageFiles[i]);
                if(texture != null && texture.name == m_detectTextureName)
                {
                    Debug.Log("ForDebug");
                }
                if(texture && texture.isReadable)
                {
                    GetMaxSamePixelRect(texture, out int left, out int right, out int up, out int down , out int width , out int height);
                    ImageSliceData data = new ImageSliceData()
                        {
                            path = allimageFiles[i],
                            width = width,
                            height = height,
                            left = left,
                            right = right,
                            up = up,
                            down = down,
                        };
                    data.UpdateMagnitude();
                    m_Image2SamePixelAreaList.Add(data);
                }
                else
                {
                    Debug.LogError($"{allimageFiles[i]}图片未处理，由于图片不可读");
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        EditorUtility.DisplayProgressBar("排序", "", 0f);
        m_Image2SamePixelAreaList.Sort((ImageSliceData a, ImageSliceData b) =>
        {
            return -(a.magnitude.CompareTo(b.magnitude));
        });
        EditorUtility.ClearProgressBar();

        //Debug.Log(m_Image2SamePixelAreaList[8].path);
        //CreateNewImage(m_Image2SamePixelAreaList[8]);
        //SetupImageFileSlice(m_Image2SamePixelAreaList[8]);

    }
    
    /// <summary>
    /// 根据拉伸数据 更改图片源文件
    /// </summary>
    /// <param name="allimageFiles"></param>
    void SetupSourceImage(List<ImageSliceData> imageSliceDatas)
    {
        float i = 0f;
        m_ChangeSourceImageList = new List<string>();
        foreach (var item in imageSliceDatas)
        {
            EditorUtility.DisplayProgressBar("修改图片源文件", item.path, (++i) / imageSliceDatas.Count);

            int sliceHorizontalCount = item.right - item.left;
            int sliceVerticalCount = item.up - item.down;
            if(sliceHorizontalCount > 1 || sliceVerticalCount > 1)//可拉伸区域如果为1x1 就不压缩图片
            {
                CreateNewImage(item);
                SetupImageFileSlice(item);
                m_ChangeSourceImageList.Add(item.path);
            }

        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 修改预制体组件对应类型
    /// </summary>
    void UpdatePrefabImageType(List<string> imagePaths)
    {
        float i = 0f;
        m_ChangeImageTypePrefabSet = new HashSet<string>();
        foreach (var item in imagePaths)
        {
            EditorUtility.DisplayProgressBar("修改图片对应预制体Image组件属性", item,  i / imagePaths.Count);
            m_imageFiles2PrefabListDict.TryGetValue(item, out var allPrefabList);
            if (allPrefabList != null)
            {
                string GUID = AssetDatabase.AssetPathToGUID(item);
                foreach (var prefab in allPrefabList)
                {
                    string prefabPath = AssetDatabase.GetAssetPath(prefab);
                    string[] allLines = File.ReadAllLines(prefabPath);

                    for (int j = 0; j < allLines.Length; j++)
                    {
                        if (allLines[j].Contains(GUID) && allLines[j].Contains("m_Sprite"))
                        {
                            //下一行为 Image 的 ImageType (0:Simple,1:Sliced:2:Tiled,3:Filled)
                            allLines[j + 1] = allLines[j + 1].Replace('0', '1');//Simple --> Sliced

                            string goName = FindGameObjectName(allLines, j);
                            m_ChangeImageTypePrefabSet.Add($"{prefabPath}\t中被更改过的物体名\t{goName}");

                            ++j;
                        }

                    }
                    File.WriteAllLines(prefabPath, allLines);
                }


            }
        }

        EditorUtility.ClearProgressBar();
    }


    string FindGameObjectName(string[] allLines , int index)
    {
        for (int nameSearchIndex = index; nameSearchIndex > 0; nameSearchIndex--)
        {
            if (allLines[nameSearchIndex].Contains("m_Name:"))
            {
                int searchIndex = allLines[nameSearchIndex].IndexOf("m_Name:");
                if (searchIndex >= 0)
                {
                    string goName = allLines[nameSearchIndex].Substring(searchIndex + 7);
                    if (goName != "" && goName != " ")
                    {
                        return goName;
                    }
                }
            }
        }
        return "";
    }

    


    /// <summary>
    /// 获取图片 中心 延伸 相同颜色像素 最大 区域
    /// </summary>
    /// <param name="texture">处理Texture2D</param>
    /// <param name="left">可拉伸范围左像素坐标</param>
    /// <param name="right">可拉伸范围右像素坐标</param>
    /// <param name="up">可拉伸范围上像素坐标</param>
    /// <param name="down">可拉伸范围下像素坐标</param>
    /// <param name="width">Texture的宽</param>
    /// <param name="height">Texture的高</param>
    void GetMaxSamePixelRect(Texture2D texture , out int left , out int right, out int up, out int down, out int width, out int height)
    {

        float thresholdPerPixel = 0.004f;

        width = texture.width;
        height = texture.height;

        //水平 判断
        int horizontalCenter = width / 2;
        int horizontalLeft = horizontalCenter - 1;//水平区间
        int horizontalRight = horizontalCenter + 1;//水平区间



        bool isLeftBoundary = false, isRightBoundary = false;
        Color[] centerColumnPixel = texture.GetPixels(horizontalCenter, 0, 1, height);
        Color[] leftColumnPixel, rightColumnPixel;


        while (!isLeftBoundary || !isRightBoundary)
        {
            if (!isLeftBoundary)
            {
                if(horizontalLeft >= 0)
                {
                    leftColumnPixel = texture.GetPixels(horizontalLeft, 0, 1, height);
                    if (IfPixelsDifferent(centerColumnPixel, leftColumnPixel, thresholdPerPixel))
                    {
                        horizontalLeft += 2;
                        isLeftBoundary = true;
                    }
                    else
                    {
                        --horizontalLeft;
                    }
                }
                else
                {
                    ++horizontalLeft;
                    isLeftBoundary = true;
                }
                
            }

            if (!isRightBoundary)
            {
                if(horizontalRight < width)
                {
                    rightColumnPixel = texture.GetPixels(horizontalRight, 0, 1, height);
                    if (IfPixelsDifferent(centerColumnPixel, rightColumnPixel, thresholdPerPixel))
                    {
                        horizontalRight -= 1;
                        isRightBoundary = true;
                    }
                    else
                    {
                        ++horizontalRight;
                    }
                }
                else
                {
                    //--horizontalRight;
                    isRightBoundary = true;
                }

            }
        }

        int verticalCenter = height / 2;
        int verticalDown = verticalCenter - 1;//垂直区间
        int verticalUp = verticalCenter + 1;//垂直区间

        bool isDownBoundary = false, isUpBoundary = false;
        Color[] centerRowPixels = texture.GetPixels(0, verticalCenter, width, 1);
        Color[] downRowPixels, upRowPixels;





        while (!isDownBoundary || !isUpBoundary)
        {
            if (!isDownBoundary)
            {
                if (verticalDown >= 0)
                {
                    downRowPixels = texture.GetPixels(0, verticalDown, width, 1);
                    if (IfPixelsDifferent(centerRowPixels, downRowPixels, thresholdPerPixel))
                    {
                        verticalDown += 2;
                        isDownBoundary = true;
                    }
                    else
                    {
                        --verticalDown;
                    }
                }
                else
                {
                    ++verticalDown;
                    isDownBoundary = true;
                }

            }

            if (!isUpBoundary)
            {
                if (verticalUp < height)
                {
                    upRowPixels = texture.GetPixels(0, verticalUp, width, 1);
                    if (IfPixelsDifferent(centerRowPixels, upRowPixels, thresholdPerPixel))
                    {
                        horizontalRight -= 1;
                        isUpBoundary = true;
                    }
                    else
                    {
                        ++verticalUp;
                    }
                }
                else
                {
                    //--horizontalRight;
                    isUpBoundary = true;
                }


            }
        }
        horizontalLeft = Mathf.Max(horizontalLeft, 0);
        horizontalRight = Mathf.Min(horizontalRight, width);
        verticalDown = Mathf.Max(verticalDown, 0);
        verticalUp = Mathf.Min(verticalUp, height);

        left = horizontalLeft;
        right = horizontalRight;
        up = verticalUp;
        down = verticalDown;
    }

    /// <summary>
    /// 判断两个Color数组是否相差无几
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    bool IfPixelsDifferent(Color[] a , Color[] b , float threshold)
    {
        if(a == null || b == null)
        {
            return true;
        }

        if(a.Length != b.Length)
        {
            return true;
        }


        for (int i = 0; i < a.Length; i++)
        {
            float distance = Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b) + Mathf.Abs(a[i].a - b[i].a);
            if (distance > threshold)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 将图片拉伸信息列表的数据写入CSV
    /// </summary>
    /// <param name="path"></param>
    void WriteImageSliceInfoIntoCSV(string path)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("图片路径,图片可拉伸宽高平方根,可压缩宽,可压缩高,左边界,右边界,上边界,下边界,leftIndex,rightIndex,upIndex,downIndex\n");
        foreach (var item in m_Image2SamePixelAreaList)
        {
            //spriteBorder: { x: 1(上), y: 4(右), z: 2(下), w: 3(左)}
            float width = item.right - item.left;
            float height = item.up - item.down;
            float size = Mathf.Sqrt(width * width + height * height);
            sb.Append($" {item.path} , {item.magnitude} , {width} , {height} , {item.left} , {item.width - item.right} , {item.height - item.up}, {item.down},{item.left} , {item.right} , {item.up}, {item.down} \n");
        }

        if(File.Exists($"{path}/图片拉伸性列表.csv"))
        {
            File.Delete($"{path}/图片拉伸性列表.csv");
        }

        using (FileStream fs = new FileStream($"{path}/图片拉伸性列表.csv", FileMode.Create,FileAccess.Write))
        {
            StreamWriter sw = new StreamWriter(fs);
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }
    }

    /// <summary>
    /// 将改变源文件的图片写入TXT
    /// </summary>
    /// <param name="path"></param>
    void WriteChangeSourceImageInfoIntoTXT(string path)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("图片路径\n");
        foreach (var item in m_ChangeSourceImageList)
        {
            sb.Append($"{item}\n");
        }

        if (File.Exists($"{path}/更改图片源文件列表.txt"))
        {
            File.Delete($"{path}/更改图片源文件列表.txt");
        }

        using (FileStream fs = new FileStream($"{path}/更改图片源文件列表.txt", FileMode.Create, FileAccess.Write))
        {
            StreamWriter sw = new StreamWriter(fs);
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }
    }

    /// <summary>
    /// 将更改ImageType的预制体写入TXT
    /// </summary>
    /// <param name="path"></param>
    void WriteChangeImageTypePrefabIntoTXT(string path)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("预制体路径\n");
        foreach (var item in m_ChangeImageTypePrefabSet)
        {
            sb.Append($"{item}\n");
        }

        if (File.Exists($"{path}/更改ImageType预制体文件列表.txt"))
        {
            File.Delete($"{path}/更改ImageType预制体文件列表.txt");
        }

        using (FileStream fs = new FileStream($"{path}/更改ImageType预制体文件列表.txt", FileMode.Create, FileAccess.Write))
        {
            StreamWriter sw = new StreamWriter(fs);
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }
    }

    //EditorCoroutine.Start(TakeScreenshots(TakeCameraShot));
    

    /// <summary>
    /// 图片拉伸性数据
    /// </summary>
    class ImageSliceData
    {
        public string path;
        public int width, height;
        public int left, right, up, down;

        public float magnitude;
        public void UpdateMagnitude()
        {
            float horizontal = right - left;
            float vertical = up - down;
            this.magnitude = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
        }
    }
}
