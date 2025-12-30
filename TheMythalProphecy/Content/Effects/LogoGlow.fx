//-----------------------------------------------------------------------------
// LogoGlow.fx - Godray effect streaming from logo letter edges
//-----------------------------------------------------------------------------

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Parameters
float GlowIntensity;  // Ray brightness (0.3-0.6 recommended)
float GlowRadius;     // Max ray length in pixels
float2 TextureSize;   // Texture dimensions
float Time;           // Animation time (optional, for subtle shimmer)

sampler2D TextureSampler : register(s0);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TexCoord;
    float4 texColor = tex2D(TextureSampler, uv);
    float2 pixelSize = 1.0 / TextureSize;

    // Center of texture for radial direction calculation
    float2 center = float2(0.5, 0.5);

    // Direction from center to this pixel (for radial rays)
    float2 toPixel = uv - center;
    float distFromCenter = length(toPixel);
    float2 radialDir = distFromCenter > 0.001 ? normalize(toPixel) : float2(0, 1);

    // Ray accumulation
    float rayAccum = 0.0;
    float3 rayColorAccum = float3(0, 0, 0);
    float totalWeight = 0.0;

    // 12 ray directions - radial outward plus some angular spread
    int numRays = 12;
    float rayLength = GlowRadius;
    int samplesPerRay = 8;

    for (int ray = 0; ray < numRays; ray++)
    {
        // Create ray directions with angular spread around the radial direction
        float angle = (float(ray) / float(numRays)) * 6.28318; // Full circle
        float2 rayDir = float2(cos(angle), sin(angle));

        // Sample along this ray direction (inward toward text)
        for (int s = 1; s <= samplesPerRay; s++)
        {
            float t = float(s) / float(samplesPerRay);
            float sampleDist = t * rayLength;

            // Sample inward along ray direction to find text edges
            float2 sampleOffset = -rayDir * pixelSize * sampleDist;
            float2 sampleUV = clamp(uv + sampleOffset, 0.0, 1.0);
            float4 sampleColor = tex2D(TextureSampler, sampleUV);

            // Exponential falloff for natural light decay
            float falloff = exp(-t * 2.5);

            // Weight by how aligned this ray is with the radial direction
            // This makes rays appear to stream outward from text
            float alignment = max(0.0, dot(rayDir, radialDir));
            alignment = lerp(0.4, 1.0, alignment); // Keep some omnidirectional glow

            float weight = falloff * alignment;

            rayAccum += sampleColor.a * weight;
            rayColorAccum += sampleColor.rgb * sampleColor.a * weight;
            totalWeight += weight;
        }
    }

    // Normalize
    if (totalWeight > 0.0)
    {
        rayAccum /= totalWeight;
        rayColorAccum /= totalWeight;
    }

    // Only apply rays where current pixel is transparent but text is nearby
    float rayMask = (1.0 - texColor.a) * rayAccum;

    // Soft threshold to create defined ray edges without harsh cutoff
    rayMask = smoothstep(0.05, 0.4, rayMask);

    // Warm golden light color for the rays
    float3 rayColor = float3(1.0, 0.9, 0.7);

    // Subtle brightness variation based on distance from center
    float centerFade = 1.0 - smoothstep(0.0, 0.6, distFromCenter);
    float rayBrightness = lerp(0.6, 1.0, centerFade);

    float3 rays = rayColor * rayMask * GlowIntensity * rayBrightness;

    // Final composite
    float3 finalColor = texColor.rgb + rays;

    // Extend alpha slightly for ray visibility, but keep it subtle
    float finalAlpha = max(texColor.a, rayMask * GlowIntensity * 0.3);

    return float4(finalColor, finalAlpha) * input.Color;
}

technique BasicEffect
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
