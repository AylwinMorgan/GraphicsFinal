/*.cs
 * using System;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class DepthOfFieldEffect : MonoBehaviour
{
    [HideInInspector]
    public Shader dofShader;

    //Numbering passes
    const int circleOfConfusionPass = 0;
    //const int preFilterPass = 1;
    const int bokehPass = 1;
    const int postFilterPass = 2;

    [Range(0.1f, 100f)]
    public float focusDistance = 10f;

    [Range(0.1f, 10f)]
    public float focusRange = 3f;

    [Range(1f, 10f)]
    public float bokehRadius = 4f;

    [NonSerialized] Material dofMaterial;

    //Full screen pass
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // If the material doesn't exist, create it
        if (dofMaterial == null)
        {
            dofMaterial = new Material(dofShader);
            // Don't need to see object in hierarchy or save it so hide flags accordingly
            dofMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        // Storing the coc in a temporary buffer because we will need it in another pass
        // Also, the texture only has one channel (RHalf) because we are only storing 1 value
        RenderTexture coc = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);

        // Setting uniforms to use in the shader
        dofMaterial.SetFloat("_BokehRadius", bokehRadius);
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange", focusRange);
        //dofMaterial.SetTexture("_CoCTex", coc);

        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;
        RenderTexture dof0 = RenderTexture.GetTemporary(width, height, 0, format);
        RenderTexture dof1 = RenderTexture.GetTemporary(width, height, 0, format);

        Graphics.Blit(source, coc, dofMaterial, circleOfConfusionPass);
        Graphics.Blit(source, dof0); //, dofMaterial, preFilterPass);
        Graphics.Blit(dof0, dof1, dofMaterial, bokehPass);
        Graphics.Blit(dof1, dof0, dofMaterial, postFilterPass);
        Graphics.Blit(dof0, destination);
        
        //Graphics.Blit(coc, destination);
        // Blit to the bokeh buffer
        //Graphics.Blit(source, destination, dofMaterial, bokehPass);

        RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(dof0);
        RenderTexture.ReleaseTemporary(dof1);
    }
}



*/

/*.shader
 * Shader "Hidden/DepthOfField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex, _CameraDepthTexture; //, _CoCTex;
        float4 _MainTex_TexelSize;

        float _BokehRadius, _FocusDistance, _FocusRange;

        struct VertexData {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Interpolators {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Interpolators VertexProgram (VertexData v) {
            Interpolators i;
            i.pos = UnityObjectToClipPos(v.vertex);
            i.uv = v.uv;
            return i;
        }
    ENDCG

    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off

        Pass{ // 0 circleOfConfusion
            CGPROGRAM
                #pragma vertex VertexProgram
                #pragma fragment FragmentProgram

                half4 FragmentProgram (Interpolators i) : SV_Target {
                    half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                    depth = LinearEyeDepth(depth);
                    //return depth;
                    float coc = (depth - _FocusDistance) / _FocusRange;
                    coc = clamp(coc, -1, 1); // * _BokehRadius;
                    if (coc < 0) {
                        return coc * -half4(1,0,0,1);
                    }
                    return coc;
                }
            ENDCG
        }

        /*Pass{ // 1 preFilterPass
            CGPROGRAM
                #pragma vertex VertexProgram
                #pragma fragment FragmentProgram

                half4 FragmentProgram (Interpolators i) : SV_Target {
                    float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
                    half coc0 = tex2D(_CoCTex, i.uv + o.xy).r;
                    half coc1 = tex2D(_CoCTex, i.uv + o.zy).r;
                    half coc2 = tex2D(_CoCTex, i.uv + o.xw).r;
                    half coc3 = tex2D(_CoCTex, i.uv + o.zw).r;

                    //half coc = (coc0 + coc1 + coc2 + coc3) * 0.25;
                    half cocMin = min(min(min(coc0, coc1), coc2), coc3);
                    half cocMax = max(max(max(coc0, coc1), coc2), coc3);
                    half coc = cocMax >= -cocMin ? cocMax : cocMin;

                    return half4(tex2D(_MainTex, i.uv).rgb, coc);
                }
            ENDCG
        }/

using static UnityEditor.ShaderData;

Pass { // 1 bokehPass
            CGPROGRAM
                #pragma vertex VertexProgram
                #pragma fragment FragmentProgram

                #define BOKEH_KERNEL_MEDIUM

                // From https://github.com/Unity-Technologies/PostProcessing/
				// blob/v2/PostProcessing/Shaders/Builtins/DiskKernels.hlsl
				#if defined (BOKEH_KERNEL_SMALL)
                    /*static const int kernelSampleCount = 16;
				    static const float2 kernel[kernelSampleCount] = {
    					float2(0, 0),
	    				float2(0.54545456, 0),
		    			float2(0.16855472, 0.5187581),
			    		float2(-0.44128203, 0.3206101),
				    	float2(-0.44128197, -0.3206102),
					    float2(0.1685548, -0.5187581),
    					float2(1, 0),
	    				float2(0.809017, 0.58778524),
		    			float2(0.30901697, 0.95105654),
			    		float2(-0.30901703, 0.9510565),
				    	float2(-0.80901706, 0.5877852),
					    float2(-1, 0),
    					float2(-0.80901694, -0.58778536),
	    				float2(-0.30901664, -0.9510566),
		    			float2(0.30901712, -0.9510565),
			    		float2(0.80901694, -0.5877853),
				    };/
                #elif defined (BOKEH_KERNEL_MEDIUM)
                    static const int kernelSampleCount = 22;
                    static const float2 kernel[kernelSampleCount] = {
                        float2(0, 0),
						float2(0.53333336, 0),
						float2(0.3325279, 0.4169768),
						float2(-0.11867785, 0.5199616),
						float2(-0.48051673, 0.2314047),
						float2(-0.48051673, -0.23140468),
						float2(-0.11867763, -0.51996166),
						float2(0.33252785, -0.4169769),
						float2(1, 0),
						float2(0.90096885, 0.43388376),
						float2(0.6234898, 0.7818315),
						float2(0.22252098, 0.9749279),
						float2(-0.22252095, 0.9749279),
						float2(-0.62349, 0.7818314),
						float2(-0.90096885, 0.43388382),
						float2(-1, 0),
						float2(-0.90096885, -0.43388376),
						float2(-0.6234896, -0.7818316),
						float2(-0.22252055, -0.974928),
						float2(0.2225215, -0.9749278),
						float2(0.6234897, -0.7818316),
						float2(0.90096885, -0.43388376),
                    };
                    #endif

                half4 FragmentProgram (Interpolators i) : SV_Target {
                    half3 color = 0;
half weight = 0;
for (int k = 0; k < kernelSampleCount; k++)
{
    float o = kernel[k]; //* _BokehRadius;
    o *= _MainTex_TexelSize.xy * _BokehRadius;
    //half radius = length(o);
    //o *= _MainTex_TexelSize.xy;
    color += tex2D(_MainTex, i.uv + o).rgb;
    /*half4 s = tex2D(_MainTex, i.uv + o);

    if (abs(s.a) >= radius) {
        color += s.rgb;
        weight += 1;
    }/
}

color *= 1.0 / kernelSampleCount; //weight;
return half4(color, 1);
                }
            ENDCG
        }

        Pass{ // 2 postFilterPass
            CGPROGRAM
                #pragma vertex VertexProgram
                #pragma fragment FragmentProgram

                half4 FragmentProgram (Interpolators i) : SV_Target {
                    float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
half4 s = tex2D(_MainTex, i.uv + o.xy) +
          tex2D(_MainTex, i.uv + o.zy) +
          tex2D(_MainTex, i.uv + o.xw) +
          tex2D(_MainTex, i.uv + o.zw);
return s * 0.25;
                }
            ENDCG
        }
    }
}
*/