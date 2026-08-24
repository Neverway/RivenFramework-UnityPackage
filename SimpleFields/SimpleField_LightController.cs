using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleField_LightController : MonoBehaviour
{
#if UNITY_EDITOR
    public Light lightComponent;
    private Light previousLightComponent;

    public LightType type;
    //[Slid] public Vector2 spotlightAngles;
    public Color color;
    public float intensity;
    public float range;

    public LightRenderMode renderMode;

    public void OnValidate()
    {
        if (lightComponent == null)
            return;

        if (previousLightComponent != lightComponent)
        {
            color = lightComponent.color;
            type = lightComponent.type;
            //spotlightAngles.x = lightComponent.innerSpotAngle;
            //spotlightAngles.y = lightComponent.spotAngle;
            renderMode = lightComponent.renderMode;
            intensity = lightComponent.intensity;
            range = lightComponent.range;

            previousLightComponent = lightComponent;
            return;
        }

        bool lightChanged = false;

        if (lightComponent.type != type)
        {
            lightComponent.type = type;
            lightChanged |= true;
        }

        if (lightComponent.color != color)
        {
            lightComponent.color = color;
            lightChanged |= true;
        }
        //if (lightComponent.innerSpotAngle != spotlightAngles.x)
        //{
        //    lightComponent.innerSpotAngle = spotlightAngles.x;
        //    lightChanged |= true;
        //}
        //
        //if (lightComponent.spotAngle != spotlightAngles.y)
        //{
        //    lightComponent.spotAngle = spotlightAngles.y;
        //    lightChanged |= true;
        //}

        if (lightComponent.renderMode != renderMode)
        {
            lightComponent.renderMode = renderMode;
            lightChanged |= true;
        }

        if (lightComponent.intensity != intensity)
        {
            lightComponent.intensity = intensity;
            lightChanged |= true;
        }

        if (lightComponent.range != range)
        {
            lightComponent.range = range;
            lightChanged |= true;
        }

        if (lightChanged)
        {
            UnityEditor.Undo.RecordObject(lightComponent, "Updated Light values from SimpleFields");
        }
    }
#endif
}
