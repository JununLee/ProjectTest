// Defined in order to enable writing to solver buffers
#define FLUVIO_SOLVER

#include "../Includes/FluvioLanguageSupport.cginc" // For FLUVIO_INITIALIZE

// ---------------------------------------------------------------------------------------
// Main include
// ---------------------------------------------------------------------------------------

// If the location of this shader or FluvioCompute.cginc is changed,
// change this to the RELATIVE path to the new include.
#include "../Includes/FluvioCompute.cginc"

// Special kernel function
#ifdef FLUVIO_API_OPENCL
    #define FLUVIO_KERNEL_PARTICLES(kernelName) \
        __kernel \
        void kernelName( \
            int4 fluvio_Count, \
			int fluvio_Stride, \
            float4 fluvio_KernelSize, \
            float4 fluvio_KernelFactors, \
            float4 fluvio_Time, \
            FLUVIO_BUFFER_RW(FluidParticle) fluvio_Particle, \
            FLUVIO_BUFFER_SOLVER_RW(int) fluvio_Neighbors, \
            FLUVIO_BUFFER(FluidData) fluvio_Fluid, \
            int fluvio_ParticleSystem_FluidID, \
            int fluvio_ParticleSystem_ParticleCount, \
            float4 fluvio_ParticleSystem_Dimensions, \
            mat4x4 fluvio_ParticleSystem_ParticleToWorldMatrix, \
            mat4x4 fluvio_ParticleSystem_WorldToParticleMatrix)
#else
    int fluvio_ParticleSystem_FluidID;
    int fluvio_ParticleSystem_ParticleCount;
    float4 fluvio_ParticleSystem_Dimensions;
    mat4x4 fluvio_ParticleSystem_ParticleToWorldMatrix;
    mat4x4 fluvio_ParticleSystem_WorldToParticleMatrix;
    #define FLUVIO_KERNEL_PARTICLES(kernelName) FLUVIO_KERNEL(kernelName)
#endif

// ---------------------------------------------------------------------------------------
// Main plugin
// ---------------------------------------------------------------------------------------

FLUVIO_KERNEL_PARTICLES(ParticleSystemToFluid)
{
    int particleIndex = get_global_id(0);	
	if (particleIndex >= fluvio_Count.y || fluvio_Particle[particleIndex].lifetime.x <= 0) return;

	if (fluvio_Particle[particleIndex].id.x != fluvio_ParticleSystem_FluidID)
		return;

	// Get particle ID
	int id = fluvio_Particle[particleIndex].id.y;

	// Skip despawned particles
	if (id >= fluvio_ParticleSystem_ParticleCount)
		return;

	// Get local position and velocity
	float4 position = fluvio_Particle[particleIndex].position;
	float4 velocity = fluvio_Particle[particleIndex].velocity;
	float size = velocity.w;

	// Apply transformation matrix
	float4 worldPosition = mul3x4(fluvio_ParticleSystem_ParticleToWorldMatrix, position);
	float4 worldVelocity = mulv(fluvio_ParticleSystem_ParticleToWorldMatrix, velocity);

	// Assign transformed position and velocity
	fluvio_Particle[particleIndex].position = worldPosition;
	worldVelocity.w = size;
	fluvio_Particle[particleIndex].velocity = worldVelocity;
}

#define INTEGRATE_PARTICLES \
if (particleIndex < fluvio_Count.z) \
{ \
    float4 worldForce = fluvio_Particle[particleIndex].force; \
    worldForce.z *= fluvio_ParticleSystem_Dimensions.x; \
    float invMass = 1.0f/fluvio_Fluid[fluvio_Particle[particleIndex].id.x].particleMass; \
    float4 acceleration = worldForce*invMass; \
    int solverIterations = (int) fluvio_Time.w; \
    float dtIter = fluvio_Time.y; \
    for (int iter = 0; iter < solverIterations; ++iter) \
    { \
        float4 t = dtIter*acceleration; \
        if (dot(t, t) > (FLUVIO_MAX_SQR_VELOCITY_CHANGE*fluvio_KernelSize.w*fluvio_KernelSize.w)) \
        { \
            t *= 0; \
        } \
        worldVelocity += t; \
    } \
}

FLUVIO_KERNEL_PARTICLES(FluidToParticleSystem)
{
	int particleIndex = get_global_id(0);
	if (particleIndex >= fluvio_Count.y || fluvio_Particle[particleIndex].lifetime.x <= 0) return;

    if (fluvio_Particle[particleIndex].id.x != fluvio_ParticleSystem_FluidID)
        return;

    // Get particle ID
    int id = fluvio_Particle[particleIndex].id.y;

    // Skip despawned particles
    if (id >= fluvio_ParticleSystem_ParticleCount)
        return;

    // Get world position, velocity, and normal
    float4 worldPosition = fluvio_Particle[particleIndex].position;
    float4 worldVelocity = fluvio_Particle[particleIndex].velocity;
    float4 worldNormal = fluvio_Particle[particleIndex].normal;

    // Axis constraint
    worldPosition.z *= fluvio_ParticleSystem_Dimensions.x;
    worldPosition.z += fluvio_ParticleSystem_Dimensions.y;
    worldVelocity.z *= fluvio_ParticleSystem_Dimensions.x;

    // Force integration: dynamic particles only
    INTEGRATE_PARTICLES;

    // Apply transformation matrix
    float4 position = mul3x4(fluvio_ParticleSystem_WorldToParticleMatrix, worldPosition);
    float4 velocity = mulv(fluvio_ParticleSystem_WorldToParticleMatrix, worldVelocity);
    float4 normal = mulv(fluvio_ParticleSystem_WorldToParticleMatrix, worldNormal);

    // Assign transformed position, velocity and normal
    fluvio_Particle[particleIndex].position = position;
    fluvio_Particle[particleIndex].velocity = velocity;
    fluvio_Particle[particleIndex].normal = normal;
}

FLUVIO_KERNEL_PARTICLES(FluidToParticleSystemFast)
{
    int particleIndex = get_global_id(0);
	if (particleIndex >= fluvio_Count.y || fluvio_Particle[particleIndex].lifetime.x <= 0) return;

    if (fluvio_Particle[particleIndex].id.x != fluvio_ParticleSystem_FluidID)
        return;

    // Get particle ID
    int id = fluvio_Particle[particleIndex].id.y;

    // Skip despawned particles
    if (id >= fluvio_ParticleSystem_ParticleCount)
        return;

    // Get world position, velocity, and normal
	float4 worldPosition = fluvio_Particle[particleIndex].position;
	float4 worldVelocity = fluvio_Particle[particleIndex].velocity;
	float4 worldNormal = fluvio_Particle[particleIndex].normal;

    // Axis constraint
    worldPosition.z *= fluvio_ParticleSystem_Dimensions.x;
    worldPosition.z += fluvio_ParticleSystem_Dimensions.y;
    worldVelocity.z *= fluvio_ParticleSystem_Dimensions.x;

    // Force integration: dynamic particles only
	INTEGRATE_PARTICLES;

    // Assign world position, velocity and normal
	fluvio_Particle[particleIndex].position = worldPosition;
	fluvio_Particle[particleIndex].velocity = worldVelocity;
	fluvio_Particle[particleIndex].normal = worldNormal;
}
