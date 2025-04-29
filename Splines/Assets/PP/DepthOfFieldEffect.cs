using System;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class DepthOfFieldEffect : MonoBehaviour
{
    [HideInInspector]
    public Shader dofShader;

    //Numbering passes
    const int circleOfConfusionPass = 0;
    const int preFilterPass = 1;
    const int bokehPass = 2;
    const int postFilterPass = 3;
    const int combinePass = 4;

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

        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;
        RenderTexture dof0 = RenderTexture.GetTemporary(width, height, 0, format);
        RenderTexture dof1 = RenderTexture.GetTemporary(width, height, 0, format);

        // Setting uniforms to use in the shader
        dofMaterial.SetFloat("_BokehRadius", bokehRadius);
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange", focusRange);
        dofMaterial.SetTexture("_CoCTex", coc);
        dofMaterial.SetTexture("_DoFTex", dof0);

        Graphics.Blit(source, coc, dofMaterial, circleOfConfusionPass);
        Graphics.Blit(source, dof0, dofMaterial, preFilterPass);
        Graphics.Blit(dof0, dof1, dofMaterial, bokehPass);
        Graphics.Blit(dof1, dof0, dofMaterial, postFilterPass);
        Graphics.Blit(source, destination, dofMaterial, combinePass);
        //Graphics.Blit(dof0, destination);
        
        //Graphics.Blit(coc, destination);
        // Blit to the bokeh buffer
        //Graphics.Blit(source, destination, dofMaterial, bokehPass);

        RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(dof0);
        RenderTexture.ReleaseTemporary(dof1);
    }
}
