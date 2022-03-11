#include "../../Includes/FluvioLanguageSupport.cginc" // for Keyframe

// ---------------------------------------------------------------------------------------
// Custom plugin properties
// ---------------------------------------------------------------------------------------

#define FLUVIO_PLUGIN_DATA_0 Keyframe // size
#define FLUVIO_PLUGIN_DATA_1 float4 // xy - range, z - smoothing, w - unused

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
        FLUVIO_BUFFER(Keyframe) size = FluvioGetPluginBuffer(0);
        float4 rangeSmoothing = FluvioGetPluginValue(1);
        
        float density = solverData_GetDensity(particleIndex);
        float d = invlerp(rangeSmoothing.x, rangeSmoothing.y, density);
        uint seed = particleIndex;

        solverData_SetSize(particleIndex, max(0.0f, lerp(solverData_GetSize(particleIndex), EvaluateMinMaxCurve(size, d, seed), rangeSmoothing.z)));
    }
}
