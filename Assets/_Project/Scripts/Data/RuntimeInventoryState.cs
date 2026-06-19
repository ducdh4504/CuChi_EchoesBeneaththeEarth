public static class RuntimeInventoryState
{
    public static bool HasSmallMap { get; private set; }
    public static bool HasMorseCode { get; private set; }
    public static bool HasSecretDecree { get; private set; }
    public static bool HasFlashlight { get; private set; }
    public static int MapFragmentCount { get; private set; }

    public static void SetSmallMapUnlocked()
    {
        HasSmallMap = true;
    }

    public static void SetMorseCodeUnlocked()
    {
        HasMorseCode = true;
    }

    public static void SetSecretDecreeUnlocked()
    {
        HasSecretDecree = true;
    }

    public static void SetFlashlightUnlocked()
    {
        HasFlashlight = true;
    }

    public static void AddMapFragment()
    {
        MapFragmentCount++;
    }

    public static void Reset()
    {
        HasSmallMap = false;
        HasMorseCode = false;
        HasSecretDecree = false;
        HasFlashlight = false;
        MapFragmentCount = 0;
    }
}