using UnityEngine;
using System;

namespace KianCommons.Math {
    [Serializable]
    public struct Vector3Serializable {
        public float x, y, z;
        public static implicit operator Vector3(Vector3Serializable v)
            => new Vector3(v.x, v.y, v.z);
        public static implicit operator Vector3Serializable(Vector3 v)
            => new Vector3Serializable {x=v.x, y=v.y, z=v.z};
    }
}
