using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TextureFMT
{
    [CreateAssetMenu]
    public class TextureFMTConfig : ScriptableObject
    {
       // public Dictionary<string, string> m_dicASTC5X5 = new Dictionary<string, string>();


        [SerializeField]
        public List<string> m_listASTC4X4 = new List<string>();

        [SerializeField]
        public List<string> m_listASTC5X5 = new List<string>();

        [SerializeField]
        public List<string> m_listASTC6X6 = new List<string>();

        [SerializeField]
        public List<string> m_listASTC8X8 = new List<string>();

        [SerializeField]
        public List<string> m_listCheckFolder = new List<string>();

        [SerializeField]
        public List<string> m_textureReadWriteFolder = new List<string>();


    }


}

