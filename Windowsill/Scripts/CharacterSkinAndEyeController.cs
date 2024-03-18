using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkinAndEyeController : MonoBehaviour
{
    Renderer[] characterMaterials;

    public Texture2D[] albedoList;
    [ColorUsage(true,true)]
    public Color[] eyeColors;
    public enum EyePosition { normal, happy, angry, dead}
    public EyePosition eyeState;

    // Start is called before the first frame update
    void Start()
    {
        characterMaterials = GetComponentsInChildren<Renderer>();

        // Initialize with the normal mode at the beginning
        eyeState = EyePosition.normal;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            eyeState = EyePosition.normal;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            eyeState = EyePosition.angry;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            eyeState = EyePosition.happy;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            eyeState = EyePosition.dead;
        }

        switch (eyeState) {
            case EyePosition.normal:
                ChangeMaterialSettings(0);
                ChangeEyeOffset(EyePosition.normal);
                break;
            case EyePosition.angry:
                ChangeMaterialSettings(1);
                ChangeEyeOffset(EyePosition.angry);
                break;
            case EyePosition.happy:
                ChangeMaterialSettings(2);
                ChangeEyeOffset(EyePosition.happy);
                break;
            case EyePosition.dead:
                ChangeMaterialSettings(3);
                ChangeEyeOffset(EyePosition.dead);
                break;
        }
    }

    public void ChangeMaterialSettings(int index)
    {
        for (int i = 0; i < characterMaterials.Length; i++)
        {
            if (characterMaterials[i].transform.CompareTag("PlayerEyes")) {
                characterMaterials[i].material.SetColor("_EmissionColor", eyeColors[index]);
            }
            else {
                //characterMaterials[i].material.SetTexture("_MainTex",albedoList[index]);
                characterMaterials[i].material.mainTexture = albedoList[index];
            }              
        }
    }

    public void ChangeEyeOffset(EyePosition pos)
    {
        Vector2 offset = Vector2.zero;

        switch (pos)
        {
            case EyePosition.normal:
                offset = new Vector2(0, 0);
                break;
            case EyePosition.happy:
                offset = new Vector2(.33f, 0);
                break;
            case EyePosition.angry:
                offset = new Vector2(.66f, 0);
                break;
            case EyePosition.dead:
                offset = new Vector2(.33f, .66f);
                break;
            default:
                break;
        }

        for (int i = 0; i < characterMaterials.Length; i++)
        {
            if (characterMaterials[i].transform.CompareTag("PlayerEyes")) {
                //Debug.Log("offset = "+ offset + ", eyestate = " + pos);
                //characterMaterials[i].material.SetTextureOffset("_MainTex", offset);
                characterMaterials[i].material.mainTextureOffset = offset;
            }
                
        }
    }

    public void ToggleSkin()
    {
        // Get int of mode
        int currentMode = (int)eyeState;

        // Switch mode to next preset
        currentMode = ((currentMode % 4) + 1) % 4;

        // translate mode from int to Mode
        eyeState = (EyePosition)currentMode;
    }
}
