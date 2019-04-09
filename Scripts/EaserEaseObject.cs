using UnityEngine;
using System.Collections;

namespace Liangddyy
{
    [System.Serializable]
    public class EaserEaseObject
    {
        public string name = string.Empty;
        public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        public EaserEaseObject(string name)
        {
            this.name = name;
        }

        public EaserEaseObject()
        {
            
        }
    }
}