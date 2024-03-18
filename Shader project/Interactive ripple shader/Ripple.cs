using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ripple : MonoBehaviour {
    public Camera mainCamera;
    public RenderTexture PrevRT;    // Previous frame's RT
    public RenderTexture CurrentRT; // Current frame's RT
    public RenderTexture TempRT;    // Temporary RT
    public Shader DrawShader;       // draw shader
    public Shader RippleShader;     // Ripple calc shader

    private Material RippleMat;
    private Material DrawMat;
    public int TextureSize = 512;
    [Range(0, 1.0f)]
    [SerializeField]public float DrawRadius = 0.15f;

    void Start()
    {
        // Get main camera object
        mainCamera = Camera.main.GetComponent<Camera>();

        CurrentRT = CreateRT();
        PrevRT = CreateRT();
        TempRT = CreateRT();

        DrawMat = new Material(DrawShader);
        RippleMat = new Material(RippleShader);

        GetComponent<Renderer>().material.mainTexture = CurrentRT;
    }

    public RenderTexture CreateRT()
    {
        RenderTexture rt = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.RFloat);
        rt.Create();

        return rt;
    }

    private void DrawAt(float x, float y, float radius)
    {
        // original texture
        DrawMat.SetTexture("_SourceTex", CurrentRT);
        DrawMat.SetVector("_Pos", new Vector4(x, y, radius));
        // pass to tempRT
        Graphics.Blit(null, TempRT, DrawMat);

        RenderTexture rt = TempRT;
        TempRT = CurrentRT;
        CurrentRT= rt;
    }

    private Vector3 lastPos;

    void Update()
    {
        // raycast detect
        if (Input.GetMouseButton(0)) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // raycast hit info
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if ((hit.point - lastPos).sqrMagnitude > 0.1f) {
                    // reset lastPos
                    lastPos = hit.point;
                    // draw
                    DrawAt(hit.textureCoord.x, hit.textureCoord.y, DrawRadius);
                }

                
            }
        }

        // calc ripple
        RippleMat.SetTexture("_PrevRT", PrevRT);
        RippleMat.SetTexture("_CurrentRT", CurrentRT);
        Graphics.Blit(null, TempRT, RippleMat);

        // pass to curr
        Graphics.Blit(TempRT, PrevRT);
        RenderTexture rt = PrevRT;
        PrevRT = CurrentRT;
        CurrentRT = rt;
    }
}
