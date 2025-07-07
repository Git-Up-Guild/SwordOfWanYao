using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Entry
{

    public static void Start()
    {
        Debug.Log("[Entry::Start] 看到这个日志表示你成功运行了热更新代码");
        Debug.Log("[Entry::Start] 测试是否成功热更新");
        Run_AOTGeneric();
    }

    public static void UnLoadTest()
    {
        Debug.Log("热更新测试 [Entry::UnLoadTest]");
    }


    struct MyVec3
    {
        public int x;
        public int y;
        public int z;
    }
    
    struct MyVec2
    {
        public float x;
        public float y;
    }
    
    private static void Run_AOTGeneric()
    {
        // 泛型实例化
        var arr = new List<MyVec3>();
        arr.Add(new MyVec3 {x = 1});
        Debug.Log("热更新测试[Demos.Run_AOTGeneric] [MyVec3]");
    
        var arr2 = new List<MyVec2>();
        arr2.Add(new MyVec2 {x = 1, y = 2});
        Debug.Log("热更新测试[Demos.Run_AOTGeneric] [MyVec2]");
    }
}