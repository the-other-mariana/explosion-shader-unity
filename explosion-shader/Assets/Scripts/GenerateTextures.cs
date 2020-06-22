using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTextures : MonoBehaviour
{
    // global params
    [Header("Color Gradient")]
    public int width;
    public int height;
    public Color[] colors;

    [Header("Layered Noise")]
    public Layer[] layers;

    [Header("Explosion Scaling")]
    public float scaleGrowth;
    public float initScale;
    public float maxScale;
    
    [Header("Explosion Depth")]
    public float initialDepth;
    public float minXDepthDelta;
    public float maxXDepthDelta;
    public float minYDepthDelta;
    public float maxYDepthDelta;

    [Header("Smoke Effect")]
    public float SmokeNoiseScale;
    public float smokeGrowth;

    [Header("Explosion Light")]
    public Color lightColor;
    public float lightRange;
    public float lightGrowth;

    [Header("Explosion Waves")]
    public float frequency;
    public float WaveLength;
    public float WaveDisplacement;
    public Vector3 timeOffset;

    [HideInInspector]
    public ApplyBlink ab;
    [HideInInspector]
    public GameObject smoke;

    // private variables
    private Texture2D finalDisplacement;
    private Texture2D smokeTexture;
    private Material material;
    private Vector2 rangeDelta;
    private Vector3 range;
    private bool smokeOut;
    private GameObject mySmoke;
    private Light pointLight;
    private float lightFactor;
    private GameObject lightGameObject;
    private bool blinked;
    private float timeCreated;
    private float globalTimer;

    // the following object is used for noise layering
    [System.Serializable]
    public struct Layer
    {
        public float scale;
        public float strength;

        [HideInInspector]
        public float offsetX;
        [HideInInspector]
        public float offsetY;
    }

    // Start is called before the first frame update
    void Start()
    {
        // variable set up
        globalTimer = 0f;
        timeCreated = 0f;
        material = this.gameObject.GetComponent<Renderer>().material;

        lightFactor = 1f;
        range = new Vector3(0f, initialDepth, 0f);
        material.SetVector("_Depth", range);

        for(int i = 0; i < layers.Length; i++)
        {
            layers[i].offsetX = Random.Range(0f, 9999f);
            layers[i].offsetY = Random.Range(0f, 9999f);
        }

        // generate the gradient texture with n colors
        Texture2D texture = GenerateGradient();
        material.SetTexture("_ColorGradTex", texture);

        // generate the n layered noise texture
        finalDisplacement = GenerateLayeredNoise();
        material.SetTexture("_DispTex", finalDisplacement);
        this.transform.localScale = new Vector3(initScale, initScale, initScale);

        // generate
        smokeTexture = GeneratePerlin();
        lightGameObject = new GameObject("The Light");
        pointLight = lightGameObject.AddComponent<Light>();
        pointLight.color = lightColor;
        pointLight.range = lightRange;
        pointLight.intensity = 0.01f;
        lightGameObject.transform.position = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        globalTimer += Time.deltaTime;
        SineWaveMove();
        // scaling the explosion upwards
        this.transform.localScale += new Vector3(scaleGrowth, scaleGrowth, scaleGrowth) * Time.deltaTime;

        // scaling up the light with time
        if(pointLight != null)
        {
            pointLight.intensity += Time.deltaTime * lightGrowth * lightFactor;
        }

        // scaling and managing the smoke globe
        if (mySmoke != null)
        {
            timeCreated += Time.deltaTime;
            mySmoke.transform.localScale += new Vector3(scaleGrowth * smokeGrowth, scaleGrowth * smokeGrowth, scaleGrowth * smokeGrowth) * Time.deltaTime;
            Material smokeMaterial = mySmoke.GetComponent<Renderer>().material;
            smokeMaterial.SetFloat("_TimeCreated", timeCreated);
            Color color = smokeMaterial.GetColor("_Color");
            color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 0.1f);
            smokeMaterial.SetColor("_Color", color);
            smokeMaterial.SetTexture("_RimTex", smokeTexture);

        }

        // the scale grows slower when in the first half of growth
        if (this.transform.localScale.x <= (maxScale / 2f))
        {
            if (!blinked)
            {
                ab.doIt = true;
                blinked = true;
            }
            rangeDelta.y = minYDepthDelta;
            rangeDelta.x = minXDepthDelta;
            range += new Vector3(rangeDelta.x, rangeDelta.y, 0f) * Time.deltaTime;
            material.SetVector("_Depth", range);
        }

        // the scale grows faster when in the second half of growth
        if (this.transform.localScale.x >= (maxScale * 0.5f))
        {
            
            if (!smokeOut)
            {
                mySmoke = Instantiate(smoke, this.transform.position, Quaternion.identity);
                Material smokeMaterial = mySmoke.GetComponent<Renderer>().material;
                mySmoke.transform.localScale = this.transform.localScale;
                smokeOut = true;
                lightFactor *= -1;
            }
            
            rangeDelta.y = maxYDepthDelta;
            rangeDelta.x = maxXDepthDelta;
            range += new Vector3(rangeDelta.x, rangeDelta.y, 0f) * Time.deltaTime;
            material.SetVector("_Depth", range);
        }

        // kill explosion after reaching the max scale specified
        if(material.GetVector("_Depth").y >= maxScale - (maxScale * 0.1f))
        {
            Destroy(this.gameObject);
            Destroy(mySmoke);
            Destroy(lightGameObject);
        }

    }

    // function that creates a gradient texture with n colors;
    private Texture2D GenerateGradient()
    {
        Texture2D texture = new Texture2D(width, height);
        
        int numColors = colors.Length;
        int step = width / (numColors - 1);

        // makes the gradient by dividing the image horizontally with n sections
        for (int i = 0; i < height; i++)
        {
            for (int s = 0; s < (numColors - 1); s++)
            {
                for (int j = s * step; j < (s + 1) * step; j++)
                {
                    Color c1 = colors[s];
                    Color c2 = colors[s + 1];
                    float top = (float)(j - (s * step));
                    float bottom = ((s + 1) * step) - (s * step);

                    float r = (top / bottom) * (c2.r - c1.r);
                    float g = (top / bottom) * (c2.g - c1.g);
                    float b = (top / bottom) * (c2.b - c1.b);

                    Color color = new Color(c1.r + r, c1.g + g, c1.b + b);
                    texture.SetPixel(j, i, color);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    // generate simple perlin noise texture
    private Texture2D GeneratePerlin()
    {
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float r = 0f, g = 0f, b = 0f;
                float xCoord = (float)x / width * SmokeNoiseScale;
                float yCoord = (float)y / height * SmokeNoiseScale;
                r += Mathf.PerlinNoise(xCoord, yCoord);
                g += Mathf.PerlinNoise(xCoord, yCoord);
                b += Mathf.PerlinNoise(xCoord, yCoord);


                Color color = new Color(r, g, b);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    // generate layered noise texture
    private Texture2D GenerateLayeredNoise()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // by accumulating the noises multiplied by its strengths and scales
                float r = 0f, g = 0f, b = 0f;
                for(int index = 0; index < layers.Length; index++)
                {
                    float xCoord = (float)x / width * layers[index].scale + layers[index].offsetX;
                    float yCoord = (float)y / height * layers[index].scale + layers[index].offsetY;
                    r += Mathf.PerlinNoise(xCoord, yCoord) * layers[index].strength;
                    g += Mathf.PerlinNoise(xCoord + 10, yCoord + 10) * layers[index].strength;
                    b += Mathf.PerlinNoise(xCoord - 20, yCoord - 20) * layers[index].strength;
                }

                Color color = new Color(r / layers.Length, g / layers.Length, b / layers.Length);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    // moving the noise textures of explosion by a sine wave
    private void SineWaveMove()
    {
        float x = WaveLength * Mathf.Sin((globalTimer / frequency * 1.0f + timeOffset.x) * 2 * Mathf.PI) + WaveDisplacement;
        float y = WaveLength * Mathf.Sin((globalTimer / frequency * 1.0f + timeOffset.y) * 2 * Mathf.PI) + WaveDisplacement;
        float z = WaveLength * Mathf.Sin((globalTimer / frequency * 1.0f + timeOffset.z) * 2 * Mathf.PI) + WaveDisplacement;
        material.SetVector("_NoiseMove", new Vector3(x / (x + y + z), y / (x + y + z), z / (x + y + z)));
    }
}
