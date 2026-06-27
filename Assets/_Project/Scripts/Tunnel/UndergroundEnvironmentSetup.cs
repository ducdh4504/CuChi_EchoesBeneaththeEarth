using UnityEngine;

public class UndergroundEnvironmentSetup : MonoBehaviour
{
    [Header("Environment Overlays")]
    [SerializeField] private bool addWallOverlays = true;
    [SerializeField] private bool addFloorOverlay = true;
    [SerializeField] private bool addCeilingOverlay = true;
    [SerializeField] private bool addWoodenSupports = true;

    [Header("Materials")]
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material ceilingMaterial;
    [SerializeField] private Material woodMaterial;

    [Header("Wall Overlay Settings")]
    [SerializeField] private float wallThickness = 0.3f;
    [SerializeField] private float wallOffset = 0.15f;

    [Header("Wooden Support Settings")]
    [SerializeField] private int horizontalBeams = 2;
    [SerializeField] private int verticalPostsPerRow = 4;
    [SerializeField] private float beamWidth = 0.4f;
    [SerializeField] private float beamDepth = 0.4f;
    [SerializeField] private float verticalSpacing = 1.5f;

    [Header("Lighting")]
    [SerializeField] private bool addDimLighting = true;
    [SerializeField] private Color lightColor = new Color(1f, 0.9f, 0.7f);
    [SerializeField] private float lightIntensity = 0.5f;

    private GameObject wallOverlayParent;
    private GameObject floorOverlayParent;
    private GameObject ceilingOverlayParent;
    private GameObject woodSupportParent;
    private GameObject lightingParent;

    void Start()
    {
        SetupEnvironment();
    }

    public void SetupEnvironment()
    {
        if (wallMaterial == null || floorMaterial == null || ceilingMaterial == null)
        {
            Debug.LogWarning("UndergroundEnvironmentSetup: Missing materials. Please assign all materials.");
            return;
        }

        if (addWallOverlays)
            CreateWallOverlays();

        if (addFloorOverlay)
            CreateFloorOverlay();

        if (addCeilingOverlay)
            CreateCeilingOverlay();

        if (addWoodenSupports)
            CreateWoodenSupports();

        if (addDimLighting)
            SetupUndergroundLighting();
    }

    private void CreateWallOverlays()
    {
        wallOverlayParent = new GameObject("WallOverlays");
        wallOverlayParent.transform.SetParent(transform);

        // Create wall overlays for each side
        CreateWallPanel(wallOverlayParent.transform, Vector3.zero, new Vector3(10, 4, wallThickness),
            new Vector3(0, 2, 5 + wallOffset), "BackWall", wallMaterial);
        CreateWallPanel(wallOverlayParent.transform, Vector3.zero, new Vector3(10, 4, wallThickness),
            new Vector3(0, 2, -5 - wallOffset), "FrontWall", wallMaterial);
        CreateWallPanel(wallOverlayParent.transform, Vector3.zero, new Vector3(wallThickness, 4, 10),
            new Vector3(5 + wallOffset, 2, 0), "LeftWall", wallMaterial);
        CreateWallPanel(wallOverlayParent.transform, Vector3.zero, new Vector3(wallThickness, 4, 10),
            new Vector3(-5 - wallOffset, 2, 0), "RightWall", wallMaterial);
    }

    private void CreateWallPanel(Transform parent, Vector3 localPos, Vector3 size, Vector3 panelPos,
        string name, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = panelPos;
        wall.transform.localScale = size;
        wall.transform.localRotation = Quaternion.identity;

        Renderer renderer = wall.GetComponent<Renderer>();
        renderer.material = new Material(mat);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        renderer.receiveShadows = true;
    }

    private void CreateFloorOverlay()
    {
        floorOverlayParent = new GameObject("FloorOverlay");
        floorOverlayParent.transform.SetParent(transform);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "UndergroundFloor";
        floor.transform.SetParent(floorOverlayParent.transform);
        floor.transform.localPosition = new Vector3(0, -0.15f, 0);
        floor.transform.localScale = new Vector3(10, 0.3f, 10);
        floor.transform.localRotation = Quaternion.identity;

        Renderer renderer = floor.GetComponent<Renderer>();
        renderer.material = new Material(floorMaterial);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ReceiveOnly;
        renderer.receiveShadows = true;

        // Add slight irregularity to floor surface
        AddFloorDetails(floorOverlayParent.transform);
    }

    private void AddFloorDetails(Transform parent)
    {
        // Add scattered small rocks/bumps
        for (int i = 0; i < 20; i++)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "FloorRock_" + i;
            rock.transform.SetParent(parent);
            rock.transform.localPosition = new Vector3(
                Random.Range(-4.5f, 4.5f),
                -0.01f,
                Random.Range(-4.5f, 4.5f)
            );
            rock.transform.localScale = new Vector3(
                Random.Range(0.05f, 0.2f),
                Random.Range(0.02f, 0.08f),
                Random.Range(0.05f, 0.2f)
            );
            rock.transform.localRotation = Random.rotation;

            Renderer renderer = rock.GetComponent<Renderer>();
            renderer.material = new Material(floorMaterial);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ReceiveOnly;
        }
    }

    private void CreateCeilingOverlay()
    {
        ceilingOverlayParent = new GameObject("CeilingOverlay");
        ceilingOverlayParent.transform.SetParent(transform);

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "UndergroundCeiling";
        ceiling.transform.SetParent(ceilingOverlayParent.transform);
        ceiling.transform.localPosition = new Vector3(0, 4.15f, 0);
        ceiling.transform.localScale = new Vector3(10, 0.3f, 10);
        ceiling.transform.localRotation = Quaternion.identity;

        Renderer renderer = ceiling.GetComponent<Renderer>();
        renderer.material = new Material(ceilingMaterial);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        renderer.receiveShadows = true;

        // Add stalactite-like details
        AddCeilingDetails(ceilingOverlayParent.transform);
    }

    private void AddCeilingDetails(Transform parent)
    {
        for (int i = 0; i < 15; i++)
        {
            GameObject stalactite = GameObject.CreatePrimitive(PrimitiveType.Cone);
            stalactite.name = "Stalactite_" + i;
            stalactite.transform.SetParent(parent);
            stalactite.transform.localPosition = new Vector3(
                Random.Range(-4.5f, 4.5f),
                4.0f,
                Random.Range(-4.5f, 4.5f)
            );
            stalactite.transform.localScale = new Vector3(
                Random.Range(0.05f, 0.15f),
                Random.Range(0.1f, 0.4f),
                Random.Range(0.05f, 0.15f)
            );
            stalactite.transform.localRotation = Quaternion.Euler(180, 0, 0);

            Renderer renderer = stalactite.GetComponent<Renderer>();
            renderer.material = new Material(ceilingMaterial);
        }
    }

    private void CreateWoodenSupports()
    {
        if (woodMaterial == null)
        {
            Debug.LogWarning("Wood material not assigned for supports.");
            return;
        }

        woodSupportParent = new GameObject("WoodenSupports");
        woodSupportParent.transform.SetParent(transform);

        float roomWidth = 10f;
        float roomDepth = 10f;
        float roomHeight = 4f;

        // Create horizontal beam along the ceiling (Z direction)
        for (int i = 0; i < horizontalBeams; i++)
        {
            float zPos = -roomDepth / 2 + (roomDepth / (horizontalBeams + 1)) * (i + 1);
            CreateHorizontalBeam(woodSupportParent.transform, new Vector3(0, roomHeight - 0.2f, zPos),
                new Vector3(roomWidth - 0.8f, beamWidth, beamDepth), "CeilingBeam_" + i);
        }

        // Create vertical posts along each side
        float xSpacing = roomWidth / (verticalPostsPerRow + 1);

        // Front row of posts
        for (int i = 0; i < verticalPostsPerRow; i++)
        {
            float xPos = -roomWidth / 2 + xSpacing * (i + 1);
            CreateVerticalPost(woodSupportParent.transform, new Vector3(xPos, roomHeight / 2, roomDepth / 2 - 0.3f),
                new Vector3(beamWidth, roomHeight, beamDepth), "FrontPost_" + i);
        }

        // Back row of posts
        for (int i = 0; i < verticalPostsPerRow; i++)
        {
            float xPos = -roomWidth / 2 + xSpacing * (i + 1);
            CreateVerticalPost(woodSupportParent.transform, new Vector3(xPos, roomHeight / 2, -roomDepth / 2 + 0.3f),
                new Vector3(beamWidth, roomHeight, beamDepth), "BackPost_" + i);
        }

        // Side cross beams
        CreateHorizontalBeam(woodSupportParent.transform, new Vector3(roomWidth / 2 - 0.3f, roomHeight - 0.5f, 0),
            new Vector3(beamDepth, beamWidth, roomDepth - 0.6f), "RightCrossBeam");
        CreateHorizontalBeam(woodSupportParent.transform, new Vector3(-roomWidth / 2 + 0.3f, roomHeight - 0.5f, 0),
            new Vector3(beamDepth, beamWidth, roomDepth - 0.6f), "LeftCrossBeam");
    }

    private void CreateVerticalPost(Transform parent, Vector3 position, Vector3 size, string name)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = name;
        post.transform.SetParent(parent);
        post.transform.localPosition = position;
        post.transform.localScale = size;
        post.transform.localRotation = Quaternion.identity;

        Renderer renderer = post.GetComponent<Renderer>();
        renderer.material = new Material(woodMaterial);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        // Add dirt accumulation at base
        AddDirtAccumulation(parent, position, "Dirt_" + name);
    }

    private void CreateHorizontalBeam(Transform parent, Vector3 position, Vector3 size, string name)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = name;
        beam.transform.SetParent(parent);
        beam.transform.localPosition = position;
        beam.transform.localScale = size;
        beam.transform.localRotation = Quaternion.identity;

        Renderer renderer = beam.GetComponent<Renderer>();
        renderer.material = new Material(woodMaterial);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }

    private void AddDirtAccumulation(Transform parent, Vector3 basePosition, string name)
    {
        GameObject dirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dirt.name = name;
        dirt.transform.SetParent(parent);
        dirt.transform.localPosition = new Vector3(basePosition.x, 0.05f, basePosition.z);
        dirt.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
        dirt.transform.localRotation = Quaternion.identity;

        Renderer renderer = dirt.GetComponent<Renderer>();
        renderer.material = new Material(floorMaterial);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ReceiveOnly;

        // Remove collider from dirt for performance
        Destroy(dirt.GetComponent<Collider>());
    }

    private void SetupUndergroundLighting()
    {
        lightingParent = new GameObject("UndergroundLighting");
        lightingParent.transform.SetParent(transform);

        // Main dim light
        GameObject mainLight = new GameObject("MainLight");
        mainLight.transform.SetParent(lightingParent.transform);
        mainLight.transform.localPosition = new Vector3(0, 3.5f, 0);
        Light lightComp = mainLight.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.color = lightColor;
        lightComp.intensity = lightIntensity;
        lightComp.range = 12f;
        lightComp.shadows = LightShadows.Soft;

        // Secondary corner lights for atmosphere
        CreateCornerLight(lightingParent.transform, new Vector3(3, 3, 3), "CornerLight1");
        CreateCornerLight(lightingParent.transform, new Vector3(-3, 3, -3), "CornerLight2");

        // Add a subtle warm ambient
        RenderSettings.ambientLight = new Color(0.15f, 0.12f, 0.1f);
    }

    private void CreateCornerLight(Transform parent, Vector3 position, string name)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.SetParent(parent);
        lightObj.transform.localPosition = position;
        Light lightComp = lightObj.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.color = new Color(1f, 0.85f, 0.6f);
        lightComp.intensity = lightIntensity * 0.4f;
        lightComp.range = 6f;
        lightComp.shadows = LightShadows.None; // No shadows for fill lights
    }

    public void CleanupEnvironment()
    {
        if (wallOverlayParent != null) DestroyImmediate(wallOverlayParent);
        if (floorOverlayParent != null) DestroyImmediate(floorOverlayParent);
        if (ceilingOverlayParent != null) DestroyImmediate(ceilingOverlayParent);
        if (woodSupportParent != null) DestroyImmediate(woodSupportParent);
        if (lightingParent != null) DestroyImmediate(lightingParent);
    }

    // Editor helper to rebuild environment
#if UNITY_EDITOR
    [ContextMenu("Rebuild Environment")]
    public void RebuildEnvironment()
    {
        CleanupEnvironment();
        SetupEnvironment();
    }
#endif
}
