using UnityEngine;

namespace FGJ26
{


    public enum MaskType
    {
        Basic = 0,
        Advanced = 1,
        Elite = 2
    }

    public interface IWearableMask
    {
        MaskType MaskType { get; }
    }
}
