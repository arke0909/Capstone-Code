using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Scripts.Rendering
{
    public class FovVisibilityMaskFeature : ScriptableRendererFeature
    {
        [Serializable]
        private sealed class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public LayerMask fovLayerMask = 1 << 19;
            public Material visibilityMaskMaterial;
            public bool debugVisibilityMask;
        }

        private const string PassName = "FOV Visibility Mask";
        private static readonly int FovVisibilityTextureId = Shader.PropertyToID("_FovVisibilityTexture");
        private static readonly int FovVisibilityDebugId = Shader.PropertyToID("_FovVisibilityDebug");

        [SerializeField] private Settings settings = new();

        private FovVisibilityMaskPass pass;

        public override void Create()
        {
            pass = new FovVisibilityMaskPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Preview ||
                renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            if (settings.visibilityMaskMaterial == null)
            {
                Debug.LogError($"{nameof(FovVisibilityMaskFeature)} needs a visibility mask material.");
                return;
            }

            pass.renderPassEvent = settings.renderPassEvent;
            pass.Setup(settings.fovLayerMask, settings.visibilityMaskMaterial, settings.debugVisibilityMask);
            renderer.EnqueuePass(pass);
        }

        private sealed class FovVisibilityMaskPass : ScriptableRenderPass
        {
            private static readonly List<ShaderTagId> ShaderTagIds = new()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };

            private LayerMask fovLayerMask;
            private Material visibilityMaskMaterial;
            private bool debugVisibilityMask;

            public FovVisibilityMaskPass()
            {
                profilingSampler = new ProfilingSampler(PassName);
            }

            public void Setup(LayerMask layerMask, Material material, bool debugMask)
            {
                fovLayerMask = layerMask;
                visibilityMaskMaterial = material;
                debugVisibilityMask = debugMask;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();

                TextureDesc textureDesc = new(CreateDescriptor(cameraData.cameraTargetDescriptor))
                {
                    name = "_FovVisibilityTexture",
                    clearBuffer = true,
                    clearColor = Color.black
                };

                TextureHandle maskTexture = renderGraph.CreateTexture(textureDesc);

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(
                           PassName,
                           out PassData passData,
                           profilingSampler))
                {
                    passData.debugVisibilityMask = debugVisibilityMask;
                    passData.rendererList = CreateRendererList(renderGraph, renderingData, cameraData, lightData);

                    builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);
                    builder.UseRendererList(passData.rendererList);
                    builder.SetGlobalTextureAfterPass(maskTexture, FovVisibilityTextureId);
                    builder.AllowPassCulling(false);
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc<PassData>(ExecuteRenderGraphPass);
                }
            }

            private RendererListHandle CreateRendererList(
                RenderGraph renderGraph,
                UniversalRenderingData renderingData,
                UniversalCameraData cameraData,
                UniversalLightData lightData)
            {
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    ShaderTagIds,
                    renderingData,
                    cameraData,
                    lightData,
                    cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = visibilityMaskMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;

                FilteringSettings filteringSettings = new(RenderQueueRange.all, fovLayerMask);
                RendererListParams rendererListParams = new(renderingData.cullResults, drawingSettings, filteringSettings);
                return renderGraph.CreateRendererList(rendererListParams);
            }

            private static void ExecuteRenderGraphPass(PassData data, RasterGraphContext context)
            {
                context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black, 1f, 0);
                context.cmd.SetGlobalFloat(FovVisibilityDebugId, data.debugVisibilityMask ? 1f : 0f);
                context.cmd.DrawRendererList(data.rendererList);
            }

            private static RenderTextureDescriptor CreateDescriptor(RenderTextureDescriptor cameraDescriptor)
            {
                cameraDescriptor.graphicsFormat = GraphicsFormat.R8_UNorm;
                cameraDescriptor.depthStencilFormat = GraphicsFormat.None;
                cameraDescriptor.msaaSamples = 1;
                cameraDescriptor.useMipMap = false;
                cameraDescriptor.autoGenerateMips = false;
                return cameraDescriptor;
            }

            private sealed class PassData
            {
                public RendererListHandle rendererList;
                public bool debugVisibilityMask;
            }
        }
    }
}
