using System;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class DepthOfFieldEffect : MonoBehaviour
{
    [HideInInspector]
    public Shader dofShader;

    //Numbering passes
    const int circleOfConfusionPass = 0;

    [Range(0.1f, 100f)]
    public float focusDistance = 10f;

    [Range(0.1f, 10f)]
    public float focusRange = 3f;

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
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange", focusRange);
        
        // Blit to the coc buffer
        Graphics.Blit(source, coc, dofMaterial, circleOfConfusionPass);
        // New blit to copy that buffer to the destination
        Graphics.Blit(coc, destination);

        RenderTexture.ReleaseTemporary(coc);
    }
}
