#include "../../Includes/FluvioLanguageSupport.cginc" // for mat4x4 and Keyframe

// Effector force type
#define EFFECTOR_FORCE_TYPE_RADIAL 0
#define EFFECTOR_FORCE_TYPE_DIRECTIONAL 1

// ---------------------------------------------------------------------------------------
// Custom plugin properties
// ---------------------------------------------------------------------------------------
typedef struct {
	mat4x4 worldToLocalMatrix;
	float4 position;
	float4 worldPosition;
	float4 extents;
	float4 decayParams; // decayParams: x - decay (or 0), y - decayJitter, z - dt (or 0), w - 1.0f if radial force, 0.0f if directional force
	float4 forceDirection;
	float effectorRange;

	// For alignment
	float unused0;
	float unused1;
	float unused2;
} FluidEffectorData;

#define FLUVIO_PLUGIN_DATA_0 FluidEffectorData // data
#define FLUVIO_PLUGIN_DATA_1 Keyframe // force
#define FLUVIO_PLUGIN_DATA_2 Keyframe // vorticity

// ---------------------------------------------------------------------------------------
// Main include
// ---------------------------------------------------------------------------------------

#include "../../Includes/FluvioCompute.cginc"

// ---------------------------------------------------------------------------------------
// Helper functions
// ---------------------------------------------------------------------------------------

inline float GetDecay(uint seed, float decay, float decayJitter)
{
    return clamp(1.0f - (clamp(decay,0.0f, 1.0f - FLUVIO_EPSILON) + (RandomFloat(seed)*(decayJitter*2.0f) - decayJitter)), 0.0f, 1.0f);
}
inline float4 GetForceNormal(float4 pos, FluidEffectorData effector)
{
    return (normalize(effector.worldPosition - pos) * effector.decayParams.w) + effector.forceDirection;
}
inline float4 GetForce(float4 norm, float dist, FLUVIO_BUFFER(Keyframe) f, float range, uint seed)
{
    return norm * EvaluateMinMaxCurve(f, dist/range, seed);
}

// ---------------------------------------------------------------------------------------
// Point effector
// ---------------------------------------------------------------------------------------

inline bool PointEffector(float4 queryPosition, FluidEffectorData effector, mat4x4 worldToLocalMatrix)
{
    float4 d = queryPosition - effector.worldPosition;
    return dot(d,d) < FLUVIO_EPSILON;
}

// ---------------------------------------------------------------------------------------
// Cube effector
// ---------------------------------------------------------------------------------------

inline bool CubeEffector(float4 queryPosition, FluidEffectorData effector, mat4x4 worldToLocalMatrix)
{
    float4 p = mul3x4(worldToLocalMatrix, queryPosition);
    float4 boundsMin = effector.position - effector.extents;
    float4 boundsMax = effector.position + effector.extents;

    return boundsMin.x < p.x && p.x <= boundsMax.x &&
           boundsMin.y < p.y && p.y <= boundsMax.y &&
           boundsMin.z < p.z && p.z <= boundsMax.z;
}

// ---------------------------------------------------------------------------------------
// Ellipsoid effector
// ---------------------------------------------------------------------------------------

inline bool EllipsoidEffector(float4 queryPosition, FluidEffectorData effector, mat4x4 worldToLocalMatrix)
{
    float4 p = mul3x4(worldToLocalMatrix, queryPosition);
    float4 d = effector.extents;
    // (x/a)^2 + (y/b)^2 + (z/c)^2 = 1
    return ((p.x/d.x)*(p.x/d.x)) + ((p.y/d.y)*(p.y/d.y)) + ((p.z/d.z)*(p.z/d.z)) <= 1;
}

// ---------------------------------------------------------------------------------------
// Main plugin macros
// ---------------------------------------------------------------------------------------
#define UpdateEffector(effectorFunc) \
{ \
    FluidEffectorData effector = FluvioGetPluginValue(0); \
    FLUVIO_BUFFER(Keyframe) force = FluvioGetPluginBuffer(1); \
    FLUVIO_BUFFER(Keyframe) vorticity = FluvioGetPluginBuffer(2); \
    mat4x4 worldToLocalMatrix = effector.worldToLocalMatrix; \
    float4 pos = solverData_GetPosition(particleIndex); \
    float lifetime = solverData_GetLifetime(particleIndex); \
    float dist = length(effector.worldPosition - pos); \
    float4 norm = GetForceNormal(pos, effector); \
    uint seed = solverData_GetRandomSeed(particleIndex); \
    if (effectorFunc(pos, effector, worldToLocalMatrix)) \
    { \
        solverData_SetLifetime(particleIndex, (solverData_GetLifetime(particleIndex) + effector.decayParams.z) * GetDecay(seed, effector.decayParams.x, effector.decayParams.y)); \
    } \
    else if (dist <= effector.effectorRange) \
    { \
        solverData_AddForce(particleIndex, GetForce(norm, dist, force, effector.effectorRange, seed), FLUVIO_FORCE_MODE_FORCE); \
        float4 v = GetForce(norm, dist, vorticity, effector.effectorRange, seed); \
        if (dot(v, v) > FLUVIO_EPSILON) \
        { \
            solverData_SetTurbulence(particleIndex, RandomFloat(seed)); \
            solverData_SetVorticity(particleIndex, v); \
        } \
    } \
}

// ---------------------------------------------------------------------------------------
// Plugin entry points
// ---------------------------------------------------------------------------------------
FLUVIO_KERNEL(OnUpdatePlugin_PointEffector)
{
    int particleIndex = get_global_id(0);

    if (FluvioShouldUpdatePlugin(particleIndex))
    {
        UpdateEffector(PointEffector);
    }
}
FLUVIO_KERNEL(OnUpdatePlugin_CubeEffector)
{
    int particleIndex = get_global_id(0);
    
    if (FluvioShouldUpdatePlugin(particleIndex))
    {
        UpdateEffector(CubeEffector);
    }
}
FLUVIO_KERNEL(OnUpdatePlugin_EllipsoidEffector)
{
    int particleIndex = get_global_id(0);
    
    if (FluvioShouldUpdatePlugin(particleIndex))
    {
        UpdateEffector(EllipsoidEffector);
    }
}
