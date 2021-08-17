using UnityEngine;
using UnityEngine.Rendering;

namespace CartoonRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Cartoon Render Pipeline")]
    public class CartoonRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new CartoonRenderPipeline();
        }
    }
}