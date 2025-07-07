using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XClient.UI
{
    public class UIRotateAnim : MonoBehaviour
    {
        public RectTransform target;
        public Vector3 euler;
        public float speed = 10;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            if (target == null)
            {
                enabled = false;
                return;
            }
            euler = target.localEulerAngles;
        }

        private void Update()
        {
            if (target == null)
            {
                enabled = false;
                return;
            }
            euler.z = euler.z + speed;
            target.LocalRotationEx(Quaternion.Euler(euler));
        }
    }
}
