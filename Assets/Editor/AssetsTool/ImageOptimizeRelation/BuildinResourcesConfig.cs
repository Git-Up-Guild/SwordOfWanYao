using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XGame
{
    [CreateAssetMenu]
    public class BuildinResourcesConfig : ScriptableObject
    {
        [SerializeField]
        public List<Texture2D> m_listBuildinResources = new List<Texture2D>();
    }


}

