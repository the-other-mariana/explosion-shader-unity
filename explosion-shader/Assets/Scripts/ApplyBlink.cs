using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyBlink : MonoBehaviour
{
    // global params
    public Material BlinkMaterial;
    public  bool doIt;

    private float factor;
    private float blinkTimer;
    private bool resetValue;
    private int counter;

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // every frame we pass the img through a material that lightens or darkens the view
        int width = source.width;
        int height = source.height;

        RenderTexture rt = RenderTexture.GetTemporary(width, height); 

        Graphics.Blit(source, rt); 

        RenderTexture tempRenderTexture = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(rt, tempRenderTexture, BlinkMaterial); 
        RenderTexture.ReleaseTemporary(rt);
        rt = tempRenderTexture;

        Graphics.Blit(rt, destination);
        RenderTexture.ReleaseTemporary(rt);
    }
    private void Start()
    {
        // variable set up
        doIt = false;
        resetValue = false;
        factor = 0.6f;
        blinkTimer = 0f;
        counter = 0;
        BlinkMaterial.SetFloat("_DarkFactor", 0.6f);
    }

    private void Update()
    {
        // when there is a collision, lighten the view image
        if (doIt)
        {
            counter++;
            if (!resetValue)
            {
                factor = 0.6f;
                resetValue = true;
            }
            if (factor < 1)
            {
                factor += Time.deltaTime *0.2f;
            }
            else
            {
                doIt = false;
            }

            BlinkMaterial.SetFloat("_DarkFactor", factor);
            blinkTimer += Time.deltaTime;
        }
        else
        {
            resetValue = false;
        }
        if(!doIt && counter > 0)
        {
            if(factor > 0.6f)
            {
                factor -= Time.deltaTime * 0.2f;
                BlinkMaterial.SetFloat("_DarkFactor", factor);
            }
            
        }
    }
}
