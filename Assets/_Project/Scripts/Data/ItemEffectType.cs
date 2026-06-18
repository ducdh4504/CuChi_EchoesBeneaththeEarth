//using UnityEngine;

//public enum ItemEffectType
//{
//    None,

//    RestoreOxygen,
//    RestoreLanternFuel,
//    //Optional
//    //RestoreHealth,

//    UnlockMap,
//    UnlockMorseCode,
//    UnlockObjective,

//    CollectMapFragment
//}

using UnityEngine;

public enum ItemEffectType
{
    None,

    RestoreOxygen,

    // Deprecated: không dùng cho gameplay mới nữa.
    // Giữ lại để tránh hỏng asset cũ nếu đang có LanternData.
    RestoreLanternFuel,

    RestoreFlashlightBattery,

    UnlockMap,
    UnlockMorseCode,
    UnlockObjective,
    UnlockFlashlight,

    CollectMapFragment
}