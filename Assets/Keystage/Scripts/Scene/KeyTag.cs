using UnityEngine;

namespace Keystage.Scene
{
    /// <summary>Marks a key mesh so pointer raycasts know which note it plays.</summary>
    public class KeyTag : MonoBehaviour
    {
        public int midi;
    }
}
