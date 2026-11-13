
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARMarkerPrefabSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MarkerPrefab
    {
        public string markerName;
        public GameObject prefab;
    }

    [Header("AR Tracked Image Manager")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Daftar Prefab Marker")]
    public List<MarkerPrefab> markerPrefabs =
        new List<MarkerPrefab>();

    // Menyimpan pasangan nama marker dan prefab
    private Dictionary<string, GameObject> prefabDictionary =
        new Dictionary<string, GameObject>(
            System.StringComparer.OrdinalIgnoreCase);

    // Menyimpan objek yang sudah dibuat
    private Dictionary<TrackableId, GameObject> spawnedObjects =
        new Dictionary<TrackableId, GameObject>();

    // Mencegah Debug Log berulang
    private HashSet<string> warnedMarkers =
        new HashSet<string>();

    private void Awake()
    {
        prefabDictionary.Clear();

        foreach (MarkerPrefab item in markerPrefabs)
        {
            if (item.prefab != null &&
                !string.IsNullOrWhiteSpace(item.markerName))
            {
                string key = item.markerName.Trim();

                if (prefabDictionary.ContainsKey(key))
                {
                    Debug.LogWarning(
                        "Nama marker duplikat: " + key);
                    continue;
                }

                prefabDictionary.Add(key, item.prefab);

                Debug.Log(
                    "Prefab berhasil didaftarkan: " + key);
            }
            else
            {
                Debug.LogError(
                    "Nama marker atau prefab belum diisi!");
            }
        }

        Debug.Log(
            "Total prefab terdaftar: " +
            prefabDictionary.Count);
    }

    private void Start()
    {
        if (trackedImageManager == null)
        {
            Debug.LogError(
                "AR Tracked Image Manager belum dihubungkan!");
            return;
        }

        Debug.Log(
            "AR Marker Prefab Spawner berhasil dijalankan.");
    }

    private void Update()
    {
        if (trackedImageManager == null)
            return;

        HashSet<TrackableId> currentIds =
            new HashSet<TrackableId>();

        foreach (ARTrackedImage image in
                 trackedImageManager.trackables)
        {
            currentIds.Add(image.trackableId);

            string imageName =
                image.referenceImage.name.Trim();

            // Periksa status tracking
            if (image.trackingState != TrackingState.Tracking)
            {
                if (spawnedObjects.TryGetValue(
                    image.trackableId,
                    out GameObject existing))
                {
                    existing.SetActive(false);
                }

                continue;
            }

            Debug.Log(
                "MARKER TERDETEKSI: " + imageName);

            // Cari prefab berdasarkan nama marker
            if (!prefabDictionary.TryGetValue(
                imageName, out GameObject prefab))
            {
                if (warnedMarkers.Add(imageName))
                {
                    Debug.LogWarning(
                        "Prefab tidak ditemukan untuk marker: " +
                        imageName);
                }

                continue;
            }

            GameObject spawned;

            // Buat objek jika belum pernah dibuat
            if (!spawnedObjects.TryGetValue(
                image.trackableId, out spawned))
            {
                spawned = Instantiate(
                    prefab,
                    image.transform);

                spawned.name = prefab.name;

                // Atur posisi relatif terhadap marker
                spawned.transform.localPosition =
                    Vector3.zero;

                spawned.transform.localRotation =
                    Quaternion.identity;

                spawnedObjects.Add(
                    image.trackableId, spawned);

                Debug.Log(
                    "MODEL BERHASIL DIMUNCULKAN: " +
                    prefab.name);
            }

            // Tampilkan model selama marker terlacak
            if (!spawned.activeSelf)
            {
                spawned.SetActive(true);
            }

            // Ikuti posisi dan rotasi marker
            spawned.transform.localPosition =
                Vector3.zero;

            spawned.transform.localRotation =
                Quaternion.identity;
        }

        // Periksa marker yang sudah tidak terdeteksi
        List<TrackableId> removedIds =
            new List<TrackableId>();

        foreach (TrackableId id in spawnedObjects.Keys)
        {
            if (!currentIds.Contains(id))
            {
                removedIds.Add(id);
            }
        }

        // Hapus model yang marker-nya sudah dihapus
        foreach (TrackableId id in removedIds)
        {
            if (spawnedObjects.TryGetValue(
                id, out GameObject obj))
            {
                Destroy(obj);
            }

            spawnedObjects.Remove(id);

            Debug.Log(
                "Marker sudah tidak terdeteksi. Model dihapus.");
        }
    }
}
