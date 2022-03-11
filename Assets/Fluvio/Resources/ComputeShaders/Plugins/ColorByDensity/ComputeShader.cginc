#include "../../Includes/FluvioLanguageSupport.cginc" // for GradientColorKey, GradientAlphaKey

// ---------------------------------------------------------------------------------------
// Custom plugin properties
// ---------------------------------------------------------------------------------------

#define FLUVIO_PLUGIN_DATA_0 GradientColorKey // gradient (color keys)
#define FLUVIO_PLUGIN_DATA_1 GradientAlphaKey // gradient (alpha keys)
#define FLUVIO_PLUGIN_DATA_2 float4 // xy - range, z - smoothing, w - unused

// ---------------------------------------------------------------------------------------
// Main include
// ---------------------------------------------------------------------------------------

// If the location of this shader or FluvioCompute.cginc is changed,
// change this to the RELATIVE path to the new include.
#include "../../Includes/FluvioCompute.cginc"

// ---------------------------------------------------------------------------------------
// Main plugin
// ---------------------------------------------------------------------------------------

FLUVIO_KERNEL(OnUpdatePlugin)
{  
    int particleIndex = get_global_id(0);

	if (FluvioShouldUpdatePlugin(particleIndex))
    {
        // Main plugin code goes here.
        FLUVIO_BUFFER(GradientColorKey) colorKeys = FluvioGetPluginBuffer(0);
        FLUVIO_BUFFER(GradientAlphaKey) alphaKeys = FluvioGetPluginBuffer(1);
        float4 rangeSmoothing = FluvioGetPluginValue(2);
        uint seed = particleIndex;

        float density = solverData_GetDensity(particleIndex);
            
        float d = invlerp(rangeSmoothing.x, rangeSmoothing.y, density);
        
        solverData_SetColor(particleIndex, lerp4(solverData_GetColor(particleIndex), EvaluateMinMaxGradient(colorKeys, alphaKeys, d, seed), rangeSmoothing.z));
    }
}
