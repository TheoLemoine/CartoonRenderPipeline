using UnityEngine;
using UnityEngine.Rendering;

namespace CartoonRP.Runtime
{
    public partial class CartoonRenderPipeline : RenderPipeline
    {
        static readonly ShaderTagId UnlitPassId = new ShaderTagId("CRPUnlit");

        private static readonly int ColorRT = Shader.PropertyToID("_CRPColor");
        private static readonly int DepthRT = Shader.PropertyToID("_CRPDepth");
        private static readonly int NormalsRT = Shader.PropertyToID("_CRPNormals");

        private readonly Material ScreenMaterial = new Material(Shader.Find("Hidden/Screen"));

        private CommandBuffer _buffer;
        private ScriptableRenderContext _context;
        private Camera _currentCamera;
        private CullingResults _currentCullingResults;
        
        public CartoonRenderPipeline()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = false;
            _buffer = new CommandBuffer {name = "Draw Geometry"};

#if UNITY_EDITOR
            _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
#endif
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            _context = context;
            
            foreach (var camera in cameras)
            {
                _currentCamera = camera;
                RenderCamera();
            }
        }

        private void RenderCamera()
        {
            Setup();
            
            // clear and draw background
            DrawBackground();
            
            if (!ComputeCullingResults(out _currentCullingResults)) return;
            
            DrawOpaque();
            DrawScreen();
#if UNITY_EDITOR
            DrawUnsupported();
            DrawGizmos();
#endif
            
            Cleanup();
            
            _context.Submit();
        }

        private void Setup()
        {
            // setup VP matrix
            _context.SetupCameraProperties(_currentCamera);
            
            _buffer.BeginSample(_currentCamera.name);

            // create render textures
            var width = _currentCamera.pixelWidth;
            var height = _currentCamera.pixelHeight;
            _buffer.GetTemporaryRT(ColorRT, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.Default);
            _buffer.GetTemporaryRT(DepthRT, width, height, 16, FilterMode.Point, RenderTextureFormat.Depth);
            _buffer.GetTemporaryRT(NormalsRT, width, height, 0, FilterMode.Point, RenderTextureFormat.Default);
            RunBuffer();
        }

        private void Cleanup()
        {
            // dispose of render textures
            _buffer.ReleaseTemporaryRT(ColorRT);
            _buffer.ReleaseTemporaryRT(DepthRT);
            _buffer.ReleaseTemporaryRT(NormalsRT);
            
            _buffer.EndSample(_currentCamera.name);
            RunBuffer();
        }

        private void DrawBackground()
        {
            // clear camera
            _buffer.ClearRenderTarget(true, true, Color.clear);
            
            // clear color buffer
            _buffer.SetRenderTarget(ColorRT);
            switch (_currentCamera.clearFlags)
            {
                case CameraClearFlags.SolidColor:
                    _buffer.ClearRenderTarget(true, true, _currentCamera.backgroundColor);
                    
                    break;
                case CameraClearFlags.Skybox:
                    _context.DrawSkybox(_currentCamera);
                    break;
            }
            
            // clear normals to black
            _buffer.SetRenderTarget(NormalsRT);
            _buffer.ClearRenderTarget(true, true, Color.black);
                
            RunBuffer();
        }

        private void DrawOpaque()
        {
            // setup RT to be rendered to
            RenderTargetIdentifier[] targets = { ColorRT, NormalsRT };
            RenderBufferLoadAction[] loadActions = { RenderBufferLoadAction.DontCare, RenderBufferLoadAction.DontCare };
            RenderBufferStoreAction[] storeActions = { RenderBufferStoreAction.Store, RenderBufferStoreAction.Store };
            var rtBinding = new RenderTargetBinding(targets, loadActions, storeActions, DepthRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            
            _buffer.SetRenderTarget(rtBinding);
            RunBuffer();

            // other settings
            var sortingSettings = new SortingSettings(_currentCamera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(UnlitPassId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            // draw
            _context.DrawRenderers(_currentCullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void DrawScreen()
        {
            // use all RTs to render to the screen
            // here add screen space effects such as edge detection
            _buffer.SetGlobalTexture("_Color", ColorRT);
            _buffer.SetGlobalTexture("_Normals", NormalsRT);
            _buffer.SetGlobalTexture("_Depth", DepthRT);
            _buffer.SetGlobalInt("_ScreenWidth", _currentCamera.pixelWidth);
            _buffer.SetGlobalInt("_ScreenHeight", _currentCamera.pixelHeight);
            _buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            _buffer.Blit(null, BuiltinRenderTextureType.CurrentActive, ScreenMaterial);
            RunBuffer();
        }
        
        
        // --- utils
        private bool ComputeCullingResults(out CullingResults cullingResults)
        {
            if (_currentCamera.TryGetCullingParameters(out ScriptableCullingParameters cullParams))
            {
                cullingResults = _context.Cull(ref cullParams);
                return true;
            }

            cullingResults = new CullingResults(); // whatever, we wont use it
            return false;
        }

        private void RunBuffer()
        {
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        } 
    }
}