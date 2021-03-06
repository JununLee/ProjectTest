// Defined in order to enable writing to solver buffers
#define FLUVIO_SOLVER

#include "../../Includes/FluvioCompute.cginc"

// ---------------------------------------------------------------------------------------
// SolverUtility
// ---------------------------------------------------------------------------------------
#define SolverUtility_GetParticleArrayIndex(particleIndex, particleCount) ((particleIndex) >= (particleCount) ? (particleIndex) - (particleCount) : (particleIndex));

// ---------------------------------------------------------------------------------------
// Poly6 SPH Kernel (general)
// ---------------------------------------------------------------------------------------
inline float Poly6Calculate(float4 dist, float poly6Factor, float kernelSizeSq)
{
    float lenSq = dot(dist, dist);
    float diffSq = kernelSizeSq - lenSq;
    return poly6Factor*diffSq*diffSq*diffSq;
}
inline float4 Poly6CalculateGradient(float4 dist, float poly6Factor, float kernelSizeSq)
{
    float lenSq = dot(dist, dist);    
    float diffSq = kernelSizeSq - lenSq;
    float f = -poly6Factor*6.0f*diffSq*diffSq; // 6.0 to convert 315/64 to 945/32
    return dist*f;
}
inline float Poly6CalculateLaplacian(float4 dist, float poly6Factor, float kernelSizeSq)
{
    float lenSq = dot(dist, dist);    
    float diffSq = kernelSizeSq - lenSq;
    float f = lenSq - (0.75f*diffSq);
    return poly6Factor*24.0f*diffSq*diffSq*f; // 24.0 to convert 315/64 to 945/8
}

// ---------------------------------------------------------------------------------------
// Spiky SPH Kernel (pressure)
// ---------------------------------------------------------------------------------------
inline float SpikyCalculate(float4 dist, float spikyFactor, float kernelSize)
{
    float lenSq = dot(dist, dist);   
    float f = kernelSize - sqrt(lenSq);
    return spikyFactor*f*f*f;
}
inline float4 SpikyCalculateGradient(float4 dist, float spikyFactor, float kernelSize)
{
    float lenSq = dot(dist, dist);    
    float len = sqrt(lenSq);
    float f = -spikyFactor*3.0f*(kernelSize - len)*(kernelSize - len)/len; // 3.0 to convert 15/1 to 45/1
    return dist*f;
}

// ---------------------------------------------------------------------------------------
// Viscosity SPH Kernel (viscosity)
// ---------------------------------------------------------------------------------------
inline float ViscosityCalculate(float4 dist, float viscosityFactor, float kernelSize3, float kernelSizeSq, float kernelSize)
{
    float lenSq = dot(dist, dist);
    float len = sqrt(lenSq);
    float len3 = len*len*len;
    return viscosityFactor*(((-len3/(2.0f*kernelSize3)) + (lenSq/kernelSizeSq) + (kernelSize/(2.0f*len))) - 1.0f);
}
inline float4 ViscosityCalculateGradient(float4 dist, float viscosityFactor, float kernelSize3, float kernelSizeSq, float kernelSize)
{
    float lenSq = dot(dist, dist);
    float len = sqrt(lenSq);
    float len3 = len*len*len;
    float f = viscosityFactor*((-3.0f*len/(2.0f*kernelSize3)) + (2.0f/kernelSizeSq) + (kernelSize/(2.0f*len3)));
    return dist*f;
}
inline float ViscosityCalculateLaplacian(float4 dist, float viscosityFactor, float kernelSize3, float kernelSize)
{
    float lenSq = dot(dist, dist);
    float len = sqrt(lenSq);
    return viscosityFactor*(6.0f/kernelSize3)*(kernelSize - len);
}

// ---------------------------------------------------------------------------------------
// Index Grid
// ---------------------------------------------------------------------------------------
inline int mod_pos(int a, int b)
{
	return (a % b + b) % b;
}
inline float fluvio_IndexGrid_GetCellSpace(float kernelSize)
{
	return kernelSize;
}
inline int4 fluvio_IndexGrid_GetIndexVector(float4 position, float cellSpace)
{
	// TODO: FLUVIO_CONVERT macro
#if FLUVIO_API_OPENCL
	return convert_int4(position / cellSpace);
#else
	return (int4)(position / cellSpace);
#endif
}
inline int fluvio_IndexGrid_GetIndex(int4 indexVector)
{
	return mod_pos(indexVector.x + FLUVIO_MAX_GRID_SIZE * (indexVector.y + FLUVIO_MAX_GRID_SIZE * indexVector.z), FLUVIO_MAX_GRID_SIZE * FLUVIO_MAX_GRID_SIZE * FLUVIO_MAX_GRID_SIZE);
}
inline int fluvio_IndexGrid_GetBucketIndex(int gridIndex, int i)
{
	return gridIndex * FLUVIO_GRID_BUCKET_SIZE + i;
}
inline int fluvio_IndexGrid_GetIndexFromPosition(float4 position, float cellSpace)
{
	return fluvio_IndexGrid_GetIndex(fluvio_IndexGrid_GetIndexVector(position, cellSpace));
}
inline void fluvio_IndexGrid_Add(volatile FLUVIO_BUFFER_SOLVER_RW(uint) grid, int particleIndex, float4 position, float kernelSize)
{
	float cellSpace = fluvio_IndexGrid_GetCellSpace(kernelSize);
	int gridIndex = fluvio_IndexGrid_GetIndexFromPosition(position, cellSpace);

	uint gridAddIndex = particleIndex + 1; // +1 for GPU grid ids
#if !FLUVIO_API_OPENCL
	[allow_uav_condition]
#endif
	for (int i = 0; i < FLUVIO_GRID_BUCKET_SIZE; ++i)
	{
		// Get grid array index
		int gi = fluvio_IndexGrid_GetBucketIndex(gridIndex, i);

		// Try to set the bucket item
		uint original;
#if FLUVIO_API_OPENCL
		original = atomic_cmpxchg(grid + gi, 0, gridAddIndex);
#else
		InterlockedCompareExchange(grid[gi], 0, gridAddIndex, original);
#endif	
		// If the bucket item was not already set or set on another thread during this loop, we're done
		if (original == 0) return;
	}
}
inline bool fluvio_IndexGrid_DoQuery(
	volatile FLUVIO_BUFFER_SOLVER_RW(uint) grid,
	int particleIndex,
	int particleCount,
	int stride,
	float kernelSizeSq,
	FLUVIO_BUFFER_SOLVER_RW(FluidParticle) particle,
	FLUVIO_BUFFER_SOLVER_RW(FluidParticle) boundaryParticle,
	FLUVIO_BUFFER_SOLVER_RW(int) neighbors,
	int4 indexVector, 
	int4 indexVectorOff,
	float4 position,
#if FLUVIO_API_OPENCL
	int* neighborCount)
#else
	inout int neighborCount)
#endif
{
	int4 indexVectorOffset = indexVector + indexVectorOff;

	int gridIndex = fluvio_IndexGrid_GetIndex(indexVectorOffset);

	for (int i = 0; i < FLUVIO_GRID_BUCKET_SIZE; ++i)
	{
		int neighborIndex = (int)grid[fluvio_IndexGrid_GetBucketIndex(gridIndex, i)] - 1;

		if (neighborIndex < 0) break;
		if (particleIndex == neighborIndex) continue;

		int neighborInd = SolverUtility_GetParticleArrayIndex(neighborIndex, particleCount);

		float4 dist;
		if (neighborInd == neighborIndex)
		{
			dist = particle[neighborInd].position - position;
		}
		else
		{
			dist = boundaryParticle[neighborInd].position - position;
		}

		dist.w = 0;
		float d = dot(dist, dist);

		if (d < kernelSizeSq)
		{
#if FLUVIO_API_OPENCL
			neighbors[particleIndex * stride + (*neighborCount)++] = neighborIndex;

			if (*neighborCount >= stride)
				return false;
#else
			neighbors[particleIndex * stride + neighborCount++] = neighborIndex;

			if (neighborCount >= stride)
				return false;
#endif
		}
	}
	return true;
}
inline int fluvio_IndexGrid_Query3D(
	volatile FLUVIO_BUFFER_SOLVER_RW(uint) grid,
	int particleIndex,
	int particleCount,
	int stride,
	float kernelSize,
	float kernelSizeSq,
	FLUVIO_BUFFER_SOLVER_RW(FluidParticle) particle,
	FLUVIO_BUFFER_SOLVER_RW(FluidParticle) boundaryParticle,
	FLUVIO_BUFFER_SOLVER_RW(int) neighbors)
{
	int neighborCount = 0;

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, particleCount);
	float4 position = particleInd == particleIndex ? particle[particleInd].position : boundaryParticle[particleInd].position;

	float cellSpace = fluvio_IndexGrid_GetCellSpace(kernelSize);
	int4 indexVector = fluvio_IndexGrid_GetIndexVector(position, cellSpace);
	
	int4 indexVectorOff;
	indexVectorOff.w = 0;

	for (indexVectorOff.x = -1; indexVectorOff.x <= 1; indexVectorOff.x++)
	{
		for (indexVectorOff.y = -1; indexVectorOff.y <= 1; indexVectorOff.y++)
		{
			for (indexVectorOff.z = -1; indexVectorOff.z <= 1; indexVectorOff.z++)
			{
				if (!fluvio_IndexGrid_DoQuery(
					grid, particleIndex, particleCount,
					stride, kernelSizeSq, particle, boundaryParticle,
					neighbors, indexVector, indexVectorOff, position,
#if FLUVIO_API_OPENCL
					&neighborCount))
#else
					neighborCount))
#endif
					return stride;
			}
		}
	}
	return neighborCount;
}
inline int fluvio_IndexGrid_Query2D(
	volatile FLUVIO_BUFFER_SOLVER_RW(uint) grid,
	int particleIndex,
	int particleCount,
	int stride,
	float kernelSize,
	float kernelSizeSq,
	FLUVIO_BUFFER_SOLVER_RW(FluidParticle) particle,
	FLUVIO_BUFFER_SOLVER_RW(FluidParticle) boundaryParticle,
	FLUVIO_BUFFER_SOLVER_RW(int) neighbors)
{
	int neighborCount = 0;

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, particleCount);
	float4 position = particleInd == particleIndex ? particle[particleInd].position : boundaryParticle[particleInd].position;

	float cellSpace = fluvio_IndexGrid_GetCellSpace(kernelSize);
	int4 indexVector = fluvio_IndexGrid_GetIndexVector(position, cellSpace);

	int4 indexVectorOff;
	indexVectorOff.z = 0;
	indexVectorOff.w = 0;

	for (indexVectorOff.x = -1; indexVectorOff.x <= 1; indexVectorOff.x++)
	{
		for (indexVectorOff.y = -1; indexVectorOff.y <= 1; indexVectorOff.y++)
		{
			if (!fluvio_IndexGrid_DoQuery(
				grid, particleIndex, particleCount,
				stride, kernelSizeSq, particle, boundaryParticle,
				neighbors, indexVector, indexVectorOff, position,
#if FLUVIO_API_OPENCL
				&neighborCount))
#else
				neighborCount))
#endif
				return stride;
		}
	}
	return neighborCount;
}
// ---------------------------------------------------------------------------------------
// Neighbor Search Helpers
// ---------------------------------------------------------------------------------------
inline int QueryIndexGrid2D(volatile FLUVIO_BUFFER_SOLVER_RW(uint) grid, int particleIndex, int particleCount, int stride, float kernelSize, float kernelSizeSq, FLUVIO_BUFFER_SOLVER_RW(FluidParticle) particle, FLUVIO_BUFFER_SOLVER_RW(FluidParticle) boundaryParticle, FLUVIO_BUFFER_SOLVER_RW(int) neighbors)
{
    return fluvio_IndexGrid_Query2D(grid,
                                    particleIndex,
                                    particleCount,
                                    stride,
								    kernelSize,
                                    kernelSizeSq,
								    particle,
		                            boundaryParticle,
                                    neighbors);
}
inline int QueryIndexGrid3D(volatile FLUVIO_BUFFER_SOLVER_RW(uint) grid, int particleIndex, int particleCount, int stride, float kernelSize, float kernelSizeSq, FLUVIO_BUFFER_SOLVER_RW(FluidParticle) particle, FLUVIO_BUFFER_SOLVER_RW(FluidParticle) boundaryParticle, FLUVIO_BUFFER_SOLVER_RW(int) neighbors)
{
    return fluvio_IndexGrid_Query3D(grid,
                                    particleIndex,
                                    particleCount,
                                    stride,
								    kernelSize,
                                    kernelSizeSq,
								    particle,
		                            boundaryParticle,
                                    neighbors);
}
inline int QueryBruteForce(int particleIndex, int particleCount, int boundaryParticleCount, int stride, float kernelSizeSq, FLUVIO_BUFFER_SOLVER_RW(FluidParticle) particle, FLUVIO_BUFFER_SOLVER_RW(FluidParticle) boundaryParticle, FLUVIO_BUFFER_SOLVER_RW(int) neighbors)
{
    int neighborCount = 0;
    float4 dist;
    float d;
	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, particleCount);
	int neighborInd;
	int totalCount = particleCount + boundaryParticleCount;
	float4 particlePosition = particleInd == particleIndex ? particle[particleInd].position : boundaryParticle[particleInd].position;
	float4 neighborPosition;
	float neighborLifetime;

    for (int neighborIndex = 0; neighborIndex < totalCount; ++neighborIndex)
    {
		neighborInd = SolverUtility_GetParticleArrayIndex(neighborIndex, particleCount);

		if (neighborInd == neighborIndex)
		{
			neighborPosition = particle[neighborInd].position;
			neighborLifetime = particle[neighborInd].lifetime.x;
		}
		else
		{
			neighborPosition = boundaryParticle[neighborInd].position;
			neighborLifetime = boundaryParticle[neighborInd].lifetime.x;
		}

    	dist = neighborPosition - particlePosition;
        dist.w = 0;
        d = dot(dist, dist);

        if (particleIndex != neighborIndex && // Not the same
            neighborLifetime > 0.0f && // Neighbor is alive
            d < kernelSizeSq) // Within the correct distance
        {
            neighbors[particleIndex * stride + neighborCount++] = neighborIndex;
            
			if (neighborCount >= stride)
                return stride;
        }
    }

    return neighborCount;
}

// ---------------------------------------------------------------------------------------
// Main solver kernels
// ---------------------------------------------------------------------------------------
FLUVIO_KERNEL_INDEX_GRID(Solver_IndexGridClear)
{
	#ifdef FLUVIO_USE_INDEX_GRID_ON
	int gridIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(gridIndex, FLUVIO_GRID_LENGTH);

	fluvio_IndexGrid[gridIndex] = 0;
	#else
	// This is never run; just here to stop the kernel from being optimized away
	fluvio_Particle[get_global_id(0)].force = 0;
	#endif
}
FLUVIO_KERNEL_INDEX_GRID(Solver_IndexGridAdd)
{
	#ifdef FLUVIO_USE_INDEX_GRID_ON
	int totalCount = fluvio_Count.y + fluvio_Count.w;
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, totalCount);

	// Reverse particle index.
	// This lets us count backwards,
	// prioritizing boundaries and static/kinematic particles for spatial partitioning.
	particleIndex = totalCount - particleIndex - 1;
	
	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);	
	float lifetime;
	float4 position;
	
	if (particleIndex == particleInd)
	{
		lifetime = fluvio_Particle[particleInd].lifetime.x;
		position = fluvio_Particle[particleInd].position;
		
		// Clear neighbor count
		fluvio_Particle[particleInd].id.z = 0;
	}
	else
	{
		lifetime = fluvio_IndexGridBoundaryParticle[particleInd].lifetime.x;
		position = fluvio_IndexGridBoundaryParticle[particleInd].position;

		// Clear neighbor count
		fluvio_IndexGridBoundaryParticle[particleInd].id.z = 0;
	}

	if (lifetime > 0.0f)
	{
		fluvio_IndexGrid_Add(fluvio_IndexGrid, particleIndex, position, fluvio_KernelSize.x);
	}	
	#else
	// This is never run; just here to stop the kernel from being optimized away
	fluvio_Particle[get_global_id(0)].force = 0;
	#endif
}
FLUVIO_KERNEL_INDEX_GRID(Solver_NeighborSearchGrid2D)
{
	#ifdef FLUVIO_USE_INDEX_GRID_ON
	int gridIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(gridIndex, FLUVIO_GRID_LENGTH);

	int particleIndex = (int)fluvio_IndexGrid[gridIndex];
    if (particleIndex < 0) return;

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);

	float lifetime;
	int neighborCount = 0;

	// ------------------------------
	// Zero forces
	// ------------------------------
	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].force = 0;
		lifetime = fluvio_Particle[particleInd].lifetime.x;
	}
	else
	{
		lifetime = fluvio_IndexGridBoundaryParticle[particleInd].lifetime.x;
	}

	// ------------------------------
	// Neighbor Search
	// ------------------------------        
	if (lifetime > 0.0f)
	{
		neighborCount = QueryIndexGrid2D(fluvio_IndexGrid,
									     particleIndex,
									     fluvio_Count.y,
									     fluvio_Stride, 
									     fluvio_KernelSize.x, 
									     fluvio_KernelSize.y,
									     fluvio_Particle, 
									     fluvio_IndexGridBoundaryParticle,
									     fluvio_Neighbors);
	}

	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].id.z = neighborCount;
	}
	else
	{
		fluvio_IndexGridBoundaryParticle[particleInd].id.z = neighborCount;
	}
	#else
	// This is never run; just here to stop the kernel from being optimized away
	fluvio_Particle[get_global_id(0)].force = 0;
	#endif
}
FLUVIO_KERNEL_INDEX_GRID(Solver_NeighborSearchGrid3D)
{
	#ifdef FLUVIO_USE_INDEX_GRID_ON
	int gridIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(gridIndex, FLUVIO_GRID_LENGTH);

	int particleIndex = (int)fluvio_IndexGrid[gridIndex];
    if (particleIndex < 0) return;

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);

	float lifetime;
	int neighborCount = 0;

	// ------------------------------
	// Zero forces
	// ------------------------------
	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].force = 0;
		lifetime = fluvio_Particle[particleInd].lifetime.x;
	}
	else
	{
		lifetime = fluvio_IndexGridBoundaryParticle[particleInd].lifetime.x;
	}

	// ------------------------------
	// Neighbor Search
	// ------------------------------        
	if (lifetime > 0.0f)
	{
		neighborCount = QueryIndexGrid3D(fluvio_IndexGrid,
									     particleIndex,
									     fluvio_Count.y,
									     fluvio_Stride, 
									     fluvio_KernelSize.x, 
									     fluvio_KernelSize.y,
									     fluvio_Particle, 
									     fluvio_IndexGridBoundaryParticle,
									     fluvio_Neighbors);
	}

	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].id.z = neighborCount;
	}
	else
	{
		fluvio_IndexGridBoundaryParticle[particleInd].id.z = neighborCount;
	}
	#else
	// This is never run; just here to stop the kernel from being optimized away
	fluvio_Particle[get_global_id(0)].force = 0;
	#endif
}
FLUVIO_KERNEL_INDEX_GRID(Solver_NeighborSearch2D)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.y + fluvio_Count.w);

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);

	float lifetime;
	int neighborCount = 0;

	// ------------------------------
	// Zero forces
	// ------------------------------
	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].force = 0;
		lifetime = fluvio_Particle[particleInd].lifetime.x;
	}
	else
	{
		lifetime = fluvio_IndexGridBoundaryParticle[particleInd].lifetime.x;
	}

	// ------------------------------
	// Neighbor Search
	// ------------------------------        
	if (lifetime > 0.0f)
	{
		#ifdef FLUVIO_USE_INDEX_GRID_ON
		neighborCount = QueryIndexGrid2D(fluvio_IndexGrid,
									     particleIndex,
									     fluvio_Count.y,
									     fluvio_Stride, 
									     fluvio_KernelSize.x, 
									     fluvio_KernelSize.y,
									     fluvio_Particle, 
									     fluvio_IndexGridBoundaryParticle,
									     fluvio_Neighbors);
		#else
		neighborCount = QueryBruteForce(particleIndex,
										fluvio_Count.y,
										fluvio_Count.w,
										fluvio_Stride,
										fluvio_KernelSize.y,
										fluvio_Particle,
										fluvio_IndexGridBoundaryParticle,
										fluvio_Neighbors);		
		#endif
	}

	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].id.z = neighborCount;
	}
	else
	{
		fluvio_IndexGridBoundaryParticle[particleInd].id.z = neighborCount;
	}
}
FLUVIO_KERNEL_INDEX_GRID(Solver_NeighborSearch3D)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.y + fluvio_Count.w);

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);

	float lifetime;
	int neighborCount = 0;

	// ------------------------------
	// Zero forces
	// ------------------------------
	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].force = 0;
		lifetime = fluvio_Particle[particleInd].lifetime.x;
	}
	else
	{
		lifetime = fluvio_IndexGridBoundaryParticle[particleInd].lifetime.x;
	}

	// ------------------------------
	// Neighbor Search
	// ------------------------------        
	if (lifetime > 0.0f)
	{
		#ifdef FLUVIO_USE_INDEX_GRID_ON
		neighborCount = QueryIndexGrid3D(fluvio_IndexGrid,
									     particleIndex,
									     fluvio_Count.y,
									     fluvio_Stride, 
									     fluvio_KernelSize.x, 
									     fluvio_KernelSize.y,
									     fluvio_Particle, 
									     fluvio_IndexGridBoundaryParticle,
									     fluvio_Neighbors);
		#else
		neighborCount = QueryBruteForce(particleIndex,
										fluvio_Count.y,
										fluvio_Count.w,
										fluvio_Stride,
										fluvio_KernelSize.y,
										fluvio_Particle,
										fluvio_IndexGridBoundaryParticle,
										fluvio_Neighbors);		
		#endif
	}

	if (particleIndex == particleInd)
	{
		fluvio_Particle[particleInd].id.z = neighborCount;
	}
	else
	{
		fluvio_IndexGridBoundaryParticle[particleInd].id.z = neighborCount;
	}
}
// ------------------------------
// Synchronize
// ------------------------------

FLUVIO_KERNEL(Solver_DensityPressure)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.y + fluvio_Count.w);

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);
	
	int fluidInd;
	FluidData fluid;
	int neighborCount;
	float4 particlePosition;
	float particleLifetime;

	if (particleIndex == particleInd)
	{
		fluidInd = fluvio_Particle[particleInd].id.x;
		fluid = fluvio_Fluid[fluidInd];
		neighborCount = fluvio_Particle[particleInd].id.z;
		particlePosition = fluvio_Particle[particleInd].position;
		particleLifetime = fluvio_Particle[particleInd].lifetime.x;
	}
	else
	{
		fluidInd = fluvio_BoundaryParticle[particleInd].id.x;
		fluid = fluvio_Fluid[fluidInd];
		neighborCount = fluvio_BoundaryParticle[particleInd].id.z;
		particlePosition = fluvio_BoundaryParticle[particleInd].position;
		particleLifetime = fluvio_BoundaryParticle[particleInd].lifetime.x;
	}

    int neighborIndex, neighborInd, neighborFluidInd;
    float density = 0, minDensity = 0, d = 0, selfDensity = 0;
    float neighborMass = 0;
	float4 dist;
    
    // ------------------------------
    // Density/Pressure calculation
    // ------------------------------  
	if (particleLifetime > 0.0f)
	{
		for (int j = 0; j < neighborCount; ++j)
		{
			neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);
			neighborInd = SolverUtility_GetParticleArrayIndex(neighborIndex, fluvio_Count.y);

			if (neighborIndex == neighborInd)
			{
				neighborFluidInd = fluvio_Particle[neighborInd].id.x;
				neighborMass = fluvio_Fluid[neighborFluidInd].particleMass;
				dist = particlePosition - fluvio_Particle[neighborInd].position;
			}
			else
			{
				neighborFluidInd = fluvio_BoundaryParticle[neighborInd].id.x;
				neighborMass = fluvio_Fluid[neighborFluidInd].particleMass;
				dist = particlePosition - fluvio_BoundaryParticle[neighborInd].position;

				d = neighborMass * Poly6Calculate(dist, fluvio_KernelFactors.x, fluvio_KernelSize.y);
				density += d;
			}

			d = neighborMass * Poly6Calculate(dist, fluvio_KernelFactors.x, fluvio_KernelSize.y);

			if (fluidInd == neighborFluidInd) selfDensity += d;
			density += d;
		}

		if (particleIndex == particleInd)
		{
			minDensity = fluvio_Fluid[fluvio_Particle[particleInd].id.x].minimumDensity;
			density = max(density, minDensity);
			selfDensity = max(selfDensity, minDensity);
			fluvio_Particle[particleInd].densityPressure.x = density;
			fluvio_Particle[particleInd].densityPressure.y = selfDensity;
			fluvio_Particle[particleInd].densityPressure.z = fluvio_Fluid[fluvio_Particle[particleInd].id.x].gasConstant * (density - fluvio_Fluid[fluvio_Particle[particleInd].id.x].initialDensity);
		}
		else
		{
			minDensity = fluvio_Fluid[fluvio_BoundaryParticle[particleInd].id.x].minimumDensity;
			density = max(density, minDensity);
			selfDensity = max(selfDensity, minDensity);
			fluvio_BoundaryParticle[particleInd].densityPressure.x = density;
			fluvio_BoundaryParticle[particleInd].densityPressure.y = selfDensity;
			fluvio_BoundaryParticle[particleInd].densityPressure.z = fluvio_Fluid[fluvio_BoundaryParticle[particleInd].id.x].gasConstant * (density - fluvio_Fluid[fluvio_BoundaryParticle[particleInd].id.x].initialDensity);
		}
	}
}

FLUVIO_KERNEL(Solver_Normal)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.y + fluvio_Count.w);

	int particleInd = SolverUtility_GetParticleArrayIndex(particleIndex, fluvio_Count.y);

	int neighborCount;
	float4 particlePosition;
	float particleLifetime;

	if (particleIndex == particleInd)
	{
		neighborCount = fluvio_Particle[particleInd].id.z;
		particlePosition = fluvio_Particle[particleInd].position;
		particleLifetime = fluvio_Particle[particleInd].lifetime.x;
	}
	else
	{
		neighborCount = fluvio_BoundaryParticle[particleInd].id.z;
		particlePosition = fluvio_BoundaryParticle[particleInd].position;
		particleLifetime = fluvio_BoundaryParticle[particleInd].lifetime.x;
	}

	int neighborIndex, neighborInd, neighborFluidInd;
	float neighborDensity;
	float normalLen;
	float neighborMass = 0;
	float4 dist, normal = 0;

	// ------------------------------
	// Normal calculation
	// ------------------------------  
	
	if (particleLifetime > 0.0f)
	{
		for (int j = 0; j < neighborCount; ++j)
		{
			neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);
			neighborInd = SolverUtility_GetParticleArrayIndex(neighborIndex, fluvio_Count.y);

			if (neighborIndex == neighborInd)
			{
				neighborFluidInd = fluvio_Particle[neighborInd].id.x;
				neighborMass = fluvio_Fluid[neighborFluidInd].particleMass;
				neighborDensity = fluvio_Particle[neighborInd].densityPressure.y;
				dist = particlePosition - fluvio_Particle[neighborInd].position;
			}
			else
			{
				neighborFluidInd = fluvio_BoundaryParticle[neighborInd].id.x;
				neighborMass = fluvio_Fluid[neighborFluidInd].particleMass;
				neighborDensity = fluvio_BoundaryParticle[neighborInd].densityPressure.y;
				dist = particlePosition - fluvio_BoundaryParticle[neighborInd].position;
			}

			neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);
			neighborInd = SolverUtility_GetParticleArrayIndex(neighborIndex, fluvio_Count.y);
			normal += (neighborMass / neighborDensity) * Poly6CalculateGradient(dist, fluvio_KernelFactors.x, fluvio_KernelSize.y);
		}

		normalLen = length(normal);

		if (particleIndex == particleInd)
		{
			fluvio_Particle[particleInd].normal = normal / normalLen;
			fluvio_Particle[particleInd].normal.w = normalLen;
		}
		else
		{
			fluvio_BoundaryParticle[particleInd].normal = normal / normalLen;
			fluvio_BoundaryParticle[particleInd].normal.w = normalLen;
		}
	}
}

// ------------------------------
// Synchronize
// ------------------------------

FLUVIO_KERNEL(Solver_Forces)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.z);

	int neighborCount = fluvio_Particle[particleIndex].id.z;
    int neighborIndex;
    float neighborMass, scalar;
    float4 dist, force;

    // ------------------------------
    // Force calculation
    // ------------------------------
    if (fluvio_Particle[particleIndex].lifetime.x > 0.0f)
    {
		for (int j = 0; j < neighborCount; ++j)
        {
			neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);
            
            if (/*neighborIndex > particleIndex && */neighborIndex < fluvio_Count.y)
            {
				neighborMass = fluvio_Fluid[fluvio_Particle[neighborIndex].id.x].particleMass;
                
                dist = fluvio_Particle[particleIndex].position - fluvio_Particle[neighborIndex].position;
				dist.w = 0;

				// Pressure Term
				scalar = neighborMass * (fluvio_Particle[particleIndex].densityPressure.z + fluvio_Particle[neighborIndex].densityPressure.z) / (fluvio_Particle[neighborIndex].densityPressure.x * 2.0f);

				force = SpikyCalculateGradient(dist, fluvio_KernelFactors.y, fluvio_KernelSize.x);
				force *= scalar;

				fluvio_Particle[particleIndex].force -= force;
				//fluvio_Particle[neighborIndex].force += force;

				// Viscosity Term
				scalar = neighborMass * ViscosityCalculateLaplacian(dist, fluvio_KernelFactors.z, fluvio_KernelSize.z, fluvio_KernelSize.x) * (1.0f / fluvio_Particle[neighborIndex].densityPressure.x);

				force = (fluvio_Particle[neighborIndex].velocity - fluvio_Particle[particleIndex].velocity) / fluvio_KernelSize.w;
				force *= scalar * fluvio_Fluid[fluvio_Particle[particleIndex].id.x].viscosity;
				force.w = 0;

				fluvio_Particle[particleIndex].force += force;
				//fluvio_Particle[neighborIndex].force -= force;
            }
        }
    }
}

// ------------------------------
// Synchronize
// ------------------------------

FLUVIO_KERNEL(Solver_BoundaryForces)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.z);

	int neighborCount = fluvio_Particle[particleIndex].id.z;
	int neighborIndex;
	float neighborMass, scalar;
	float4 dist, force;

	// ------------------------------
	// Force calculation
	// ------------------------------
	if (fluvio_Particle[particleIndex].lifetime.x > 0.0f)
	{
		for (int j = 0; j < neighborCount; ++j)
		{
			neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);
			
			if (/*neighborIndex > particleIndex && */neighborIndex >= fluvio_Count.y)
			{
				neighborIndex -= fluvio_Count.y;
				neighborMass = fluvio_Fluid[fluvio_BoundaryParticle[neighborIndex].id.x].particleMass;

				dist = fluvio_Particle[particleIndex].position - fluvio_BoundaryParticle[neighborIndex].position;
				dist.w = 0;

				// Pressure Term
				scalar = neighborMass * (fluvio_Particle[particleIndex].densityPressure.z + fluvio_BoundaryParticle[neighborIndex].densityPressure.z) / (fluvio_BoundaryParticle[neighborIndex].densityPressure.x * 2.0f);

				force = SpikyCalculateGradient(dist, fluvio_KernelFactors.y, fluvio_KernelSize.x);
				force *= scalar;

				fluvio_Particle[particleIndex].force -= force;
				//fluvio_Particle[neighborIndex].force += force;

				// Viscosity Term
				scalar = neighborMass * ViscosityCalculateLaplacian(dist, fluvio_KernelFactors.z, fluvio_KernelSize.z, fluvio_KernelSize.x) * (1.0f / fluvio_BoundaryParticle[neighborIndex].densityPressure.x);

				force = (fluvio_BoundaryParticle[neighborIndex].velocity - fluvio_Particle[particleIndex].velocity) / fluvio_KernelSize.w;
				force *= scalar * fluvio_Fluid[fluvio_Particle[particleIndex].id.x].viscosity;
				force.w = 0;

				fluvio_Particle[particleIndex].force += force;
				//fluvio_Particle[neighborIndex].force -= force;
			}
		}
	}
}

// ------------------------------
// Synchronize
// ------------------------------

FLUVIO_KERNEL(Solver_Turbulence)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.z);

	int neighborCount = fluvio_Particle[particleIndex].id.z;
    int neighborIndex;
    float neighborMass, neighborTurbulence, scalar;
    float4 dist, force;
            
    // ------------------------------
    // Turbulence
    // ------------------------------
    if (fluvio_Particle[particleIndex].lifetime.x > 0.0f)
    {
        for (int j = 0; j < neighborCount; ++j)
        {
            neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);

			if (neighborIndex < fluvio_Count.y)
			{
				neighborMass = fluvio_Fluid[fluvio_Particle[neighborIndex].id.x].particleMass;
				neighborTurbulence = fluvio_Fluid[fluvio_Particle[neighborIndex].id.x].turbulence;

				dist = fluvio_Particle[particleIndex].position - fluvio_Particle[neighborIndex].position;
				dist.w = 0;

				// Turbulence
				if (neighborIndex < fluvio_Count.z && fluvio_Particle[particleIndex].vorticityTurbulence.w >= fluvio_Fluid[fluvio_Particle[particleIndex].id.x].turbulence && fluvio_Particle[neighborIndex].vorticityTurbulence.w < neighborTurbulence)
				{
					scalar = neighborMass * ViscosityCalculateLaplacian(dist, fluvio_KernelFactors.z, fluvio_KernelSize.z, fluvio_KernelSize.x) * (1.0f / fluvio_Particle[neighborIndex].densityPressure.x);

					fluvio_Particle[particleIndex].vorticityTurbulence = scalar * (fluvio_Particle[neighborIndex].vorticityTurbulence - fluvio_Particle[particleIndex].vorticityTurbulence);

					force.xyz = clamp_len(FLUVIO_TURBULENCE_CONSTANT * cross(dist.xyz, fluvio_Particle[particleIndex].vorticityTurbulence.xyz), FLUVIO_MAX_SQR_VELOCITY_CHANGE * fluvio_Fluid[fluvio_Particle[particleIndex].id.x].particleMass);
					force.w = 0;

					fluvio_Particle[particleIndex].force += force;
				}
			}
        }
    }
}

// ------------------------------
// Synchronize
// ------------------------------

FLUVIO_KERNEL(Solver_ExternalForces)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.z);

	int neighborCount = fluvio_Particle[particleIndex].id.z;
	int neighborIndex;
	float neighborMass, scalar;
	float4 dist, force;
            
    // ------------------------------
    // External forces
    // ------------------------------
    if (fluvio_Particle[particleIndex].lifetime.x > 0.0f)
    {
        for (int j = 0; j < neighborCount; ++j)
        {
            neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);

            if (/*neighborIndex > particleIndex && */neighborIndex < fluvio_Count.y)
            {
				neighborMass = fluvio_Fluid[fluvio_Particle[neighborIndex].id.x].particleMass;
                
				dist = fluvio_Particle[particleIndex].position - fluvio_Particle[neighborIndex].position;
                dist.w = 0;

                // Surface Tension (external)
				if (fluvio_Particle[particleIndex].normal.w > FLUVIO_PI && fluvio_Particle[particleIndex].normal.w < FLUVIO_PI * 2.0f)
                {
                    scalar = neighborMass * Poly6CalculateLaplacian(dist, fluvio_KernelFactors.x, fluvio_KernelSize.y) * fluvio_Fluid[fluvio_Particle[particleIndex].id.x].surfaceTension * (1.0f / fluvio_Particle[neighborIndex].densityPressure.x);

					force = fluvio_Particle[particleIndex].normal;
					force.w = 0;
                    force *= scalar;

					fluvio_Particle[particleIndex].force -= force;
                    //fluvio_Particle[neighborIndex].force += force;
                }
            }
        }

        // Buoyancy Term (external)
		fluvio_Particle[particleIndex].force += fluvio_Fluid[fluvio_Particle[particleIndex].id.x].gravity * (fluvio_Fluid[fluvio_Particle[particleIndex].id.x].buoyancyCoefficient * (fluvio_Particle[particleIndex].densityPressure.x - fluvio_Fluid[fluvio_Particle[particleIndex].id.x].initialDensity));
    }
}

// ------------------------------
// Synchronize
// ------------------------------

FLUVIO_KERNEL(Solver_Constraints)
{
	int particleIndex = get_global_id(0);
	FLUVIO_BOUNDS_CHECK(particleIndex, fluvio_Count.z);

	float particleInvMass = 1.0f / fluvio_Fluid[fluvio_Particle[particleIndex].id.x].particleMass;
    float dt = fluvio_Time.y;
    int neighborCount = fluvio_Particle[particleIndex].id.z;
    float minDistance = (0.5f * fluvio_KernelSize.x);
    float minDistanceSq = minDistance * minDistance;
    int neighborIndex;
    float sqDistance, d;
    float4 dist;
            
    // ------------------------------
    // Distance constraint
    // ------------------------------
    if (fluvio_Particle[particleIndex].lifetime.x > 0.0f)
    {
        for (int j = 0; j < neighborCount; ++j)
        {
            neighborIndex = FluvioGetNeighborIndex(fluvio_Neighbors, particleIndex, fluvio_Stride, j);

			if (/*neighborIndex > particleIndex && */neighborIndex < fluvio_Count.y)
            {
				dist = fluvio_Particle[particleIndex].position - fluvio_Particle[neighborIndex].position;
                dist.w = 0;
                sqDistance = dot(dist, dist);

                if (sqDistance < minDistanceSq)
                {
                    if (sqDistance > FLUVIO_EPSILON)
                    {
                        d = sqrt(sqDistance);
                        dist *= (0.5f*(d - minDistance)/d);                                
                    }
                    else
                    {
                        dist.y = 0.5f * minDistance;
                    }
                    fluvio_Particle[particleIndex].force += (dist*particleInvMass)/dt;
                    //fluvio_Particle[neighborIndex].force -= (dist*(1.0f/fluvio_Fluid[fluvio_Particle[neighborIndex].id.x].particleMass))/dt;
                }
            }
        }
    }
}
