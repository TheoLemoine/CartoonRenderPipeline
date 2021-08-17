using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CartoonRP.Runtime
{
#if UNITY_EDITOR
    /**
     * Part of the CRP only used in the editor
     */
    public partial class CartoonRenderPipeline
    {
        
        static readonly ShaderTagId[] LegacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        
        private Material _errorMaterial;
        
        private void DrawUnsupported()
        {
            var sortingSettings = new SortingSettings(_currentCamera);
            var drawingSettings = new DrawingSettings(ShaderTagId.none, sortingSettings)
            {
                overrideMaterial = _errorMaterial
            };
            var filteringSettings = FilteringSettings.defaultValue;

            for (int i = 0; i < LegacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);
            }
            
            _context.DrawRenderers(
                _currentCullingResults, ref drawingSettings, ref filteringSettings
            );
        }

        private void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos()) {
                _context.DrawGizmos(_currentCamera, GizmoSubset.PreImageEffects);
                _context.DrawGizmos(_currentCamera, GizmoSubset.PostImageEffects);
            }
        }
    }
#endif
}