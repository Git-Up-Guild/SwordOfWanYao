using System.Collections.Generic;
using UnityEngine;

public class EEE
{
}

namespace XClient.Scripts.Test
{
    public struct AAA
    {
        public struct CCC
        {

        }

    }

    public class BBB
    {
        public struct DDD
        {

        }
    }
    namespace Monitor
    {
        public class TestMonitorClassA
        {
            int B = 0;
        }
    }
    public class MonoTestClass
    {
        public class SubMonoTestClassA
        {

        }
        public class SubMonoTestClassB
        {

        }
        SubMonoTestClassA a = new SubMonoTestClassA();
        SubMonoTestClassB b = new SubMonoTestClassB();
    }

    public enum TestEnum
    {
        AAA,
        BBB,
    }

    public class ThreadTest
    {
        public class SubThreadTestA
        {
            private List<int> intArr = new List<int>(1024);
            //private int[] intArr;
            private int intValue;
            private float floatValue;
            public SubThreadTestA()
            {
                for (int i = 0; i < 1024; i++)
                {
                    intArr.Add(i);
                }
                Debug.Log($"构造 SubThreadTestA ");
            }
        }
        public class SubThreadTestB
        {
            public SubThreadTestB()
            {
                Debug.Log("构造 SubThreadTestB");
            }
        }

        public delegate void Func(ThreadTest s);
        static public void TestAction(Func a)
        {
            //a?.Invoke(null);
            //a?.Invoke(new ThreadTest());
        }
        
    }
}
