using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerCamMechanicCore : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private int maxSortingOrderToCapture = 7;
    [SerializeField] private Camera screenshotCamera;
    [SerializeField] private BoxCollider2D objToScreenshot;
    [SerializeField] private SpriteRenderer _imageToExclude;
    [SerializeField] private Transform instantiateTarget;
    private RenderTexture renderTexture;
    public GameObject copy;
    public PhysicsMaterial2D material;
    public Sprite screenShotSprite { get; private set; }  // Add this property
    public BoxCollider2D ObjToScreenshot => objToScreenshot;  // Add this property

    void Start()
    {
        // Setup screenshot camera if not assigned
        if (screenshotCamera == null)
        {
            GameObject camObj = new GameObject("Screenshot Camera");
            screenshotCamera = camObj.AddComponent<Camera>();
            Camera mainCam = Camera.main;
            screenshotCamera.CopyFrom(mainCam);
            screenshotCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
            screenshotCamera.cullingMask &= ~(1 << _imageToExclude.gameObject.layer);
            screenshotCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Background"));
            screenshotCamera.enabled = true;
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            screenshotCamera.targetTexture = renderTexture;
        }
    }

    public IEnumerator TakeSnapShotAndSave()
    {
        yield return new WaitForEndOfFrame();

        Bounds bounds = objToScreenshot.bounds;
        screenshotCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, screenshotCamera.transform.position.z);
        screenshotCamera.orthographicSize = bounds.size.y / 2f;
        
        screenshotCamera.Render();
        RenderTexture.active = renderTexture;

        Vector3 minScreenPoint = screenshotCamera.WorldToScreenPoint(bounds.min);
        Vector3 maxScreenPoint = screenshotCamera.WorldToScreenPoint(bounds.max);

        int width = Mathf.RoundToInt(maxScreenPoint.x - minScreenPoint.x);
        int height = Mathf.RoundToInt(maxScreenPoint.y - minScreenPoint.y);
        int startX = Mathf.RoundToInt(minScreenPoint.x);
        int startY = Mathf.RoundToInt(minScreenPoint.y);

        // Create texture and capture screenshot
        Texture2D ss = new Texture2D(width, height, TextureFormat.RGBA32, true);
        ss.filterMode = FilterMode.Point;
        ss.wrapMode = TextureWrapMode.Clamp;
        ss.anisoLevel = 0;
        ss.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        ss.Apply(true);

        // Create sprite
        float unitsPerPixel = bounds.size.x / width;
        float calculatedPixelsPerUnit = 1f / unitsPerPixel;
        Sprite newSprite = Sprite.Create(ss, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), calculatedPixelsPerUnit);
        screenShotSprite = newSprite;  // Set the property

        // Create game object in scene
        GameObject newObject = CreateGameObjectWithColliders(newSprite, bounds);

        // Handle instantiation target
        if (instantiateTarget != null)
        {
            newObject.transform.position = instantiateTarget.position;
            newObject.layer = 9;
            newObject.transform.SetParent(instantiateTarget);

            if (copy != null)
            {
                Destroy(copy);
            }
            copy = newObject;
        }

        RenderTexture.active = null;
    }

    private GameObject CreateGameObjectWithColliders(Sprite capturedSprite, Bounds captureBounds)
    {
        GameObject newObject = new GameObject("CapturedScene");
        SpriteRenderer sr = newObject.AddComponent<SpriteRenderer>();
        sr.sprite = capturedSprite;
        sr.sortingOrder = 10;

        // Copy colliders
        Collider2D[] collidersInBounds = Physics2D.OverlapAreaAll(captureBounds.min, captureBounds.max);
        
        foreach (Collider2D col in collidersInBounds)
        {
            if (col == objToScreenshot || !ShouldCaptureCollider(col)) continue;

            GameObject colCopy = new GameObject($"Collider_{col.gameObject.name}");
            colCopy.transform.parent = newObject.transform;
            
            Vector3 relativePos = col.transform.position - objToScreenshot.transform.position;
            colCopy.transform.localPosition = relativePos;
            colCopy.transform.localRotation = col.transform.localRotation;
            colCopy.transform.localScale = col.transform.localScale;

            if (col is PolygonCollider2D polyCol)
            {
                ClipAndCreatePolygonCollider(polyCol, colCopy, captureBounds);
            }

            if (colCopy.GetComponent<Collider2D>() == null)
            {
                DestroyImmediate(colCopy);
            }
        }

        return newObject;
    }

    private bool ShouldCaptureCollider(Collider2D col)
    {
        SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
        if (sr == null) return true; // If no SpriteRenderer, allow capture

        return sr.sortingOrder <= maxSortingOrderToCapture;
    }

    private void ClipAndCreatePolygonCollider(PolygonCollider2D original, GameObject target, Bounds captureBounds)
    {
        List<Vector2> originalVerts = new List<Vector2>();
        List<Vector2> boundsVerts = new List<Vector2>();

        // Get original polygon vertices in world space
        for (int i = 0; i < original.pathCount; i++)
        {
            Vector2[] path = original.GetPath(i);
            for (int j = 0; j < path.Length; j++)
            {
                Vector2 worldPoint = original.transform.TransformPoint(path[j]);
                originalVerts.Add(worldPoint);

#if UNITY_EDITOR
                // Debug visualization for original polygon
                if (j > 0)
                {
                    Vector2 prevPoint = original.transform.TransformPoint(path[j - 1]);
                    Debug.DrawLine(prevPoint, worldPoint, Color.blue, 2f);
                }
#endif
            }
        }

        // Set bounds vertices
        boundsVerts.Add(new Vector2(captureBounds.min.x, captureBounds.min.y));
        boundsVerts.Add(new Vector2(captureBounds.min.x, captureBounds.max.y));
        boundsVerts.Add(new Vector2(captureBounds.max.x, captureBounds.max.y));
        boundsVerts.Add(new Vector2(captureBounds.max.x, captureBounds.min.y));

#if UNITY_EDITOR
        // Debug visualization for bounds
        for (int i = 0; i < boundsVerts.Count; i++)
        {
            Debug.DrawLine(boundsVerts[i], boundsVerts[(i + 1) % boundsVerts.Count], Color.red, 2f);
        }
#endif

        // Get intersection in world space
        Vector2[] intersectedPoints = PolygonIntersection.GetPolygonIntersection(
            originalVerts.ToArray(),
            boundsVerts.ToArray()
        );

        if (intersectedPoints.Length >= 3)
        {
            // Calculate bounds of intersection points (like box collider does with min/max)
            Vector2 min = intersectedPoints[0];
            Vector2 max = intersectedPoints[0];
            foreach (var point in intersectedPoints)
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            // Set target position to objToScreenshot's position
            target.transform.position = objToScreenshot.transform.position;
            PolygonCollider2D newCol = target.AddComponent<PolygonCollider2D>();

            // Convert points to local space relative to objToScreenshot's position
            Vector2[] localPoints = new Vector2[intersectedPoints.Length];
            for (int i = 0; i < intersectedPoints.Length; i++)
            {
                localPoints[i] = target.transform.InverseTransformPoint(intersectedPoints[i]);
            }

            // Adjust the scale of the points to match the target's local scale
            for (int i = 0; i < localPoints.Length; i++)
            {
                localPoints[i] = Vector2.Scale(localPoints[i], target.transform.localScale);
            }

            newCol.points = localPoints;

            // Set other properties
            newCol.isTrigger = original.isTrigger;
            newCol.gameObject.tag = "Environment";
            newCol.gameObject.layer = 9;
            if (material != null)
            {
                newCol.sharedMaterial = material;
            }

#if UNITY_EDITOR
            // Debug visualization
            foreach (var point in intersectedPoints)
            {
                Debug.DrawLine(point + Vector2.up * 0.1f, point - Vector2.up * 0.1f, Color.green, 2f);
                Debug.DrawLine(point + Vector2.right * 0.1f, point - Vector2.right * 0.1f, Color.green, 2f);
            }
#endif

            target.transform.localPosition = Vector3.zero;
            target.transform.localScale = Vector3.one;
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
    }

    private void SetupCollider(Collider2D col)
    {
        col.gameObject.tag = "Environment";
        col.gameObject.layer = 9;
        col.sharedMaterial = material;
    }
}
