using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResNetRealtimeClassifier : MonoBehaviour
{
    public Unity.InferenceEngine.ModelAsset modelAsset;   
    public TextAsset labelsCsv;                     
    public RawImage previewImage;    
    public RawImage modelInputPreview; 

    [Tooltip("Skor minimal biar prediksi dianggap valid, di bawah ini dianggap 'tidak yakin'")]
    public float confidenceThreshold = 60f;

    [System.Serializable]
    public class LabelPrefabPair
    {
        public string label;     
        public GameObject prefab;
    }

    [Header("Spawn Setup")]
    public LabelPrefabPair[] labelPrefabs;   
    public int maxObjects = 6;               

    [Header("Off-screen Spawn")]
    public Camera spawnCamera;               
    public float groundY = 0f;            
    [Range(0f, 0.3f)] public float offscreenMargin = 0.15f; 

    private Dictionary<string, GameObject> prefabLookup;
    private Queue<GameObject> spawnedQueue = new Queue<GameObject>();
    private string lastSpawnedLabel = null; 

    private Unity.InferenceEngine.Worker worker;
    private string[] labels;
    private WebCamTexture webcamTex;
    private Texture2D frameTex;

    [SerializeField] private float inferenceInterval = 0.5f;
    
    private const int InputW = 240;
    private const int InputH = 320;

    private const int NumClasses = 11;

    void Start()
    {
        var model = Unity.InferenceEngine.ModelLoader.Load(modelAsset);
        worker = new Unity.InferenceEngine.Worker(model, Unity.InferenceEngine.BackendType.GPUCompute);

        labels = labelsCsv.text.Split('\n')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();

        prefabLookup = new Dictionary<string, GameObject>();
        foreach (var pair in labelPrefabs)
        {
            if (pair != null && !string.IsNullOrEmpty(pair.label) && pair.prefab != null)
                prefabLookup[pair.label] = pair.prefab;
        }

        if (spawnCamera == null) spawnCamera = Camera.main;

        webcamTex = new WebCamTexture();
        webcamTex.Play();

        if (previewImage != null)
            previewImage.texture = webcamTex;

        frameTex = new Texture2D(InputW, InputH, TextureFormat.RGB24, false);

        if (modelInputPreview != null)
            modelInputPreview.texture = frameTex;

        StartCoroutine(InferenceLoop());
    }

    IEnumerator InferenceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(inferenceInterval);
            if (webcamTex.width < 100) continue; 
            CaptureAndClassify();
        }
    }

    void CaptureAndClassify()
    {
        RenderTexture rt = RenderTexture.GetTemporary(InputW, InputH);
        Graphics.Blit(webcamTex, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        frameTex.ReadPixels(new Rect(0, 0, InputW, InputH), 0, 0);
        frameTex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        var input = TextureToTensor(frameTex);
        worker.Schedule(input);
        var output = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;

        float[] scores = output.DownloadToArray();
        var top3 = Enumerable.Range(0, NumClasses)
            .OrderByDescending(i => scores[i])
            .Take(3)
            .ToList();

        string top3Log = string.Join(" | ", top3.Select(i => $"{labels[i]} ({scores[i]:F1})"));

        if (scores[top3[0]] < confidenceThreshold)
        {
            Debug.Log($"(tidak yakin, skor rendah) {top3Log}");
        }
        else
        {
            string predictedLabel = labels[top3[0]];
            Debug.Log($"Prediksi: {top3Log}");
            TrySpawnObject(predictedLabel);
        }

        input.Dispose();
    }

    void TrySpawnObject(string label)
    {
        if (label == lastSpawnedLabel)
            return;

        if (!prefabLookup.TryGetValue(label, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning($"Tidak ada prefab terdaftar untuk label '{label}' di labelPrefabs.");
            return;
        }

        Vector3 spawnPos = GetOffscreenSpawnPosition();
        Vector3 targetInScreen = GetInScreenTarget();

        float prefabY = prefab.transform.position.y;
        spawnPos.y = prefabY;
        targetInScreen.y = prefabY;

        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
        spawnedQueue.Enqueue(obj);
        lastSpawnedLabel = label;

        var behaviour = obj.GetComponent<AnimalBehaviourBase>();
        if (behaviour != null)
            behaviour.Initialize(targetInScreen, prefab.transform.eulerAngles);
        else
            Debug.LogWarning($"Prefab '{label}' gak punya komponen AnimalBehaviourBase, dia bakal diam di titik spawn.");

        while (spawnedQueue.Count > maxObjects)
        {
            GameObject oldest = spawnedQueue.Dequeue();
            if (oldest != null)
                Destroy(oldest);
        }
    }

    Vector3 GetOffscreenSpawnPosition()
    {
        int side = Random.Range(0, 4);
        float vx = 0.5f, vy = 0.5f;
        switch (side)
        {
            case 0: vx = -offscreenMargin; vy = Random.Range(0.1f, 0.9f); break;
            case 1: vx = 1f + offscreenMargin; vy = Random.Range(0.1f, 0.9f); break;
            case 2: vx = Random.Range(0.1f, 0.9f); vy = 1f + offscreenMargin; break;
            case 3: vx = Random.Range(0.1f, 0.9f); vy = -offscreenMargin; break;
        }
        return GetPointOnGround(vx, vy);
    }

    Vector3 GetInScreenTarget()
    {
        float vx = Random.Range(0.25f, 0.75f);
        float vy = Random.Range(0.25f, 0.75f);
        return GetPointOnGround(vx, vy);
    }

    Vector3 GetPointOnGround(float viewportX, float viewportY)
    {
        Ray ray = spawnCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0f));
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (groundPlane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        Debug.LogWarning("Ray gak nyentuh bidang tanah, fallback ke titik kamera + 10 unit ke depan.");
        return ray.GetPoint(10f);
    }

    Unity.InferenceEngine.Tensor<float> TextureToTensor(Texture2D tex)
    {
        float[] mean = { 0.485f, 0.456f, 0.406f };
        float[] std  = { 0.229f, 0.224f, 0.225f };
        var data = new float[1 * 3 * InputH * InputW];
        Color[] pixels = tex.GetPixels();

        for (int y = 0; y < InputH; y++)
        for (int x = 0; x < InputW; x++)
        {
            int srcY = InputH - 1 - y;
            Color c = pixels[srcY * InputW + x];

            int idx = y * InputW + x;
            data[0 * InputH * InputW + idx] = (c.r - mean[0]) / std[0];
            data[1 * InputH * InputW + idx] = (c.g - mean[1]) / std[1];
            data[2 * InputH * InputW + idx] = (c.b - mean[2]) / std[2];
        }

        return new Unity.InferenceEngine.Tensor<float>(
            new Unity.InferenceEngine.TensorShape(1, 3, InputH, InputW), data);
    }

    void OnDestroy()
    {
        webcamTex?.Stop();
        worker?.Dispose();
    }
}