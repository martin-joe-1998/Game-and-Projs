using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkyboxController : MonoBehaviour {
    [SerializeField] private List<LightingSettings> newLightingSettings;
    [SerializeField] private List<Material> newSkyboxMaterial; // 这里用来引用新的Skybox材质
    [SerializeField] private Mode mode;
    private enum Mode {
        CartoonBaseBlueSky,
        CartoonBaseNightSky,
        ColdNight,
        ColdSunset,
        DeepDusk,
        EpicBlueSunset,
        EpicGloriousPink,
        NightMoonBurst,
        OvercastLow,
        SpaceAnotherPlanet,
        Default,
    }

    // 在需要改变Skybox的地方调用这个方法
    public void ChangeToNewSkybox(int index)
    {
        if (newSkyboxMaterial[index] != null) {
            RenderSettings.skybox = newSkyboxMaterial[index]; // 将RenderSettings的skybox属性设置为新的材质
        } else {
              Debug.LogError("Didn't Find certain skybox materials!");
        }
    }

    public void ChangeToNewLighting(int index)
    {
        if (newLightingSettings[index] != null) {
#if UNITY_EDITOR
            Lightmapping.lightingSettings = newLightingSettings[index];
#endif
        } else {
            Debug.LogError("Didn't Find certain .lighting file!");
        }
    }

    void Update()
    {
        switch (mode) {
            case Mode.CartoonBaseBlueSky:
                ChangeToNewSkybox(0);
                ChangeToNewLighting(0);
                break;
            case Mode.CartoonBaseNightSky:
                ChangeToNewSkybox(1);
                ChangeToNewLighting(1);
                break;
            case Mode.ColdNight:
                ChangeToNewSkybox(2);
                ChangeToNewLighting(2);
                break;
            case Mode.ColdSunset:
                ChangeToNewSkybox(3);
                ChangeToNewLighting(3);
                break;
            case Mode.DeepDusk:
                ChangeToNewSkybox(4);
                ChangeToNewLighting(4);
                break;
            case Mode.EpicBlueSunset:
                ChangeToNewSkybox(5);
                ChangeToNewLighting(5);
                break;
            case Mode.EpicGloriousPink:
                ChangeToNewSkybox(6);
                ChangeToNewLighting(6);
                break;
            case Mode.NightMoonBurst:
                ChangeToNewSkybox(7);
                ChangeToNewLighting(7);
                break;
            case Mode.OvercastLow:
                ChangeToNewSkybox(8);
                ChangeToNewLighting(8);
                break;
            case Mode.SpaceAnotherPlanet:
                ChangeToNewSkybox(9);
                ChangeToNewLighting(9);
                break;
            case Mode.Default:
                ChangeToNewSkybox(10);
                ChangeToNewLighting(10);
                break;
        }
    }

    public void SwitchMode()
    {
        // Get int of mode
        int currentMode = (int)mode;

        // Switch mode to next preset
        currentMode = ((currentMode % 11) + 1) % 11; 

        // translate mode from int to Mode
        mode = (Mode)currentMode;
    }
}