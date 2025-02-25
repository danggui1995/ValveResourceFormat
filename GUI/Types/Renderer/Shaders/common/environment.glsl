// https://seblagarde.wordpress.com/2012/09/29/image-based-lighting-approaches-and-parallax-corrected-cubemap/
vec3 CubeMapBoxProjection(vec3 pos, vec3 R, vec3 mins, vec3 maxs, vec3 center)
{
    // Following is the parallax-correction code
    // Find the ray intersection with box plane
    vec3 FirstPlaneIntersect = (maxs - pos) / R;
    vec3 SecondPlaneIntersect = (mins - pos) / R;
    // Get the furthest of these intersections along the ray
    // (Ok because x/0 give +inf and -x/0 give -inf )
    vec3 FurthestPlane = max(FirstPlaneIntersect, SecondPlaneIntersect);
    // Find the closest far intersection
    float Distance = min(min(FurthestPlane.x, FurthestPlane.y), FurthestPlane.z);

    // Get the intersection position
    vec3 IntersectPositionWS = pos + R * Distance;
    // Get corrected reflection
    return IntersectPositionWS - center;
    // End parallax-correction code
}

#define MAX_ENVMAP_LOD 7
#define SCENE_ENVIRONMENT_TYPE 0

#if (SCENE_ENVIRONMENT_TYPE == 0) // None or missing environment map
    // ...
#elif (SCENE_ENVIRONMENT_TYPE == 1) // Per-object cube map
    uniform samplerCube g_tEnvironmentMap;
    uniform vec4 g_vEnvMapBoxMins;
    uniform vec4 g_vEnvMapBoxMaxs;
    uniform vec4 g_vEnvMapPositionWs;
#elif (SCENE_ENVIRONMENT_TYPE == 2) // Per scene cube map array
    #define MAX_ENVMAPS 144
    uniform samplerCubeArray g_tEnvironmentMap;
    uniform mat4 g_matEnvMapWorldToLocal[MAX_ENVMAPS];
    uniform vec4 g_vEnvMapPositionWs[MAX_ENVMAPS];
    uniform vec4 g_vEnvMapBoxMins[MAX_ENVMAPS];
    uniform vec4 g_vEnvMapBoxMaxs[MAX_ENVMAPS];
    uniform int g_iEnvironmentMapArrayIndex;
#endif

vec3 GetEnvironment(vec3 R, float lod)
{
    #if (SCENE_ENVIRONMENT_TYPE == 0)
        return vec3(0.0, 0.0, 0.0);
    #else

    #if (SCENE_ENVIRONMENT_TYPE == 1)
        vec3 coords = R;
        vec3 mins = g_vEnvMapBoxMins.xyz;
        vec3 maxs = g_vEnvMapBoxMaxs.xyz;
        vec3 center = g_vEnvMapPositionWs.xyz;
    #elif (SCENE_ENVIRONMENT_TYPE == 2)
        vec4 coords = vec4(R, g_iEnvironmentMapArrayIndex);
        vec3 mins = g_vEnvMapBoxMins[g_iEnvironmentMapArrayIndex].xyz;
        vec3 maxs = g_vEnvMapBoxMaxs[g_iEnvironmentMapArrayIndex].xyz;
        vec3 center = g_vEnvMapPositionWs[g_iEnvironmentMapArrayIndex].xyz;
        if (g_vEnvMapPositionWs[g_iEnvironmentMapArrayIndex].w > 0.0)
        {
            coords.xyz = CubeMapBoxProjection(vFragPosition, R, mins, maxs, center);
        }
    #endif

    return textureLod(g_tEnvironmentMap, coords, lod).rgb;
    #endif
}
