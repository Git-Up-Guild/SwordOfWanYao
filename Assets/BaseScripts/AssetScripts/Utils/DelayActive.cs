using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XGame.Utils
{
    public class DelayActive : MonoBehaviour
    {
        public float delayTime = 0.3f;

        private void OnEnable()
        {
            transform.localScale = Vector3.zero;

            Invoke("RestoreScale", 0.3f);
        }

        private void RestoreScale()
        {
            transform.localScale = Vector3.one;
        }
    }
}
