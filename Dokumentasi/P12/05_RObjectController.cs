using UnityEngine;

public class ARObjectController : MonoBehaviour
{
    [Header("Target AR")]
    public Transform targetObject;
    public string targetTag = "ARObject";

    [Header("Rotation")]
    public float rotateStep = 15f;
    [Tooltip("Putar child pertama (model) agar tidak tertimpa tracking pada root.")]
    public bool rotateChild = true;

    [Header("Scale (relatif terhadap ukuran awal)")]
    public float scaleFactor = 1.15f;
    public float minScale = 0.2f;   // 0.2x ukuran awal
    public float maxScale = 3.0f;   // 3x ukuran awal

    [Header("Optional Feedback")]
    public AudioSource audioSource;

    // Menyimpan ukuran awal target untuk batas skala relatif
    private Transform scaleOwner;
    private Vector3 initialScale = Vector3.one;

    void Start()
    {
        FindTargetIfNeeded();
    }

    public void FindTargetIfNeeded()
    {
        if (targetObject != null) return;

        GameObject found = GameObject.FindGameObjectWithTag(targetTag);
        if (found != null)
        {
            targetObject = found.transform;
        }
    }

    private bool TargetReady()
    {
        if (targetObject == null)
        {
            FindTargetIfNeeded();
        }

        if (targetObject == null)
        {
            Debug.LogWarning("Target AR belum ditemukan. Scan marker terlebih dahulu.");
            return false;
        }

        return true;
    }

    // Menentukan objek yang diputar
    private Transform GetRotateTarget()
    {
        if (rotateChild && targetObject.childCount > 0)
        {
            return targetObject.GetChild(0);
        }
        return targetObject;
    }

    public void RotateLeft()
    {
        if (!TargetReady()) return;
        GetRotateTarget().Rotate(0f, -rotateStep, 0f, Space.Self);
        PlayFeedback();
    }

    public void RotateRight()
    {
        if (!TargetReady()) return;
        GetRotateTarget().Rotate(0f, rotateStep, 0f, Space.Self);
        PlayFeedback();
    }

    public void ScaleUp()
    {
        if (!TargetReady()) return;
        ApplyScale(scaleFactor);
        PlayFeedback();
    }

    public void ScaleDown()
    {
        if (!TargetReady()) return;
        ApplyScale(1f / scaleFactor);
        PlayFeedback();
    }

    private void ApplyScale(float factor)
    {
        // Catat ukuran awal saat target baru pertama kali diskalakan
        if (scaleOwner != targetObject)
        {
            scaleOwner = targetObject;
            initialScale = targetObject.localScale;

            if (initialScale.x < 0.0001f)
            {
                initialScale = Vector3.one * 0.0001f;
            }
        }

        // Hitung rasio saat ini terhadap ukuran awal, lalu batasi
        float currentRatio = targetObject.localScale.x / initialScale.x;
        float desiredRatio = Mathf.Clamp(currentRatio * factor, minScale, maxScale);

        targetObject.localScale = initialScale * desiredRatio;
    }

    private void PlayFeedback()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}