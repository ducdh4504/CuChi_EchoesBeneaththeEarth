using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndScenePhotoSlideshow : MonoBehaviour
{
    [Header("Background Images")]
    [SerializeField] private Image photoA;
    [SerializeField] private Image photoB;

    [Header("Real Cu Chi Photos")]
    [SerializeField] private Sprite[] photos;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Screen Fit")]
    [SerializeField] private bool cropToFillScreen = true;

    private int currentIndex;
    private bool isPhotoAActive = true;

    private void Start()
    {
        if (photoA == null || photoB == null)
        {
            Debug.LogWarning("EndScenePhotoSlideshow: Chưa gán Photo A hoặc Photo B.");
            return;
        }

        if (photos == null || photos.Length == 0)
        {
            Debug.LogWarning("EndScenePhotoSlideshow: Chưa gán ảnh Địa đạo Củ Chi.");
            return;
        }

        PrepareImage(photoA);
        PrepareImage(photoB);

        currentIndex = 0;

        SetPhoto(photoA, photos[currentIndex]);
        SetAlpha(photoA, 1f);
        SetAlpha(photoB, 0f);

        if (photos.Length > 1)
        {
            StartCoroutine(SlideshowRoutine());
        }
    }

    private void LateUpdate()
    {
        // Đảm bảo ảnh vẫn full màn hình khi Game View đổi tỉ lệ.
        if (cropToFillScreen)
        {
            ApplyCoverFit(photoA);
            ApplyCoverFit(photoB);
        }
    }

    private IEnumerator SlideshowRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(displayDuration);

            int nextIndex = (currentIndex + 1) % photos.Length;

            Image currentPhoto = isPhotoAActive ? photoA : photoB;
            Image nextPhoto = isPhotoAActive ? photoB : photoA;

            SetPhoto(nextPhoto, photos[nextIndex]);
            SetAlpha(nextPhoto, 0f);

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / fadeDuration);

                SetAlpha(currentPhoto, 1f - t);
                SetAlpha(nextPhoto, t);

                yield return null;
            }

            SetAlpha(currentPhoto, 0f);
            SetAlpha(nextPhoto, 1f);

            currentIndex = nextIndex;
            isPhotoAActive = !isPhotoAActive;
        }
    }

    private void PrepareImage(Image image)
    {
        if (image == null) return;

        image.raycastTarget = false;
        image.type = Image.Type.Simple;

        // Tắt preserveAspect để script tự crop-fill, tránh bị hở hai bên.
        image.preserveAspect = false;
    }

    private void SetPhoto(Image image, Sprite sprite)
    {
        if (image == null || sprite == null) return;

        image.sprite = sprite;
        ApplyCoverFit(image);
    }

    private void ApplyCoverFit(Image image)
    {
        if (!cropToFillScreen) return;
        if (image == null || image.sprite == null) return;

        RectTransform imageRect = image.rectTransform;
        RectTransform parentRect = imageRect.parent as RectTransform;

        if (parentRect == null) return;

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        if (parentWidth <= 0f || parentHeight <= 0f) return;

        float spriteWidth = image.sprite.rect.width;
        float spriteHeight = image.sprite.rect.height;

        if (spriteWidth <= 0f || spriteHeight <= 0f) return;

        float parentRatio = parentWidth / parentHeight;
        float spriteRatio = spriteWidth / spriteHeight;

        float finalWidth;
        float finalHeight;

        if (spriteRatio > parentRatio)
        {
            // Ảnh rộng hơn màn hình: lấy chiều cao làm chuẩn, crop trái/phải.
            finalHeight = parentHeight;
            finalWidth = finalHeight * spriteRatio;
        }
        else
        {
            // Ảnh cao hơn màn hình: lấy chiều rộng làm chuẩn, crop trên/dưới.
            finalWidth = parentWidth;
            finalHeight = finalWidth / spriteRatio;
        }

        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = new Vector2(finalWidth, finalHeight);
        imageRect.localScale = Vector3.one;
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}