using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using WebGpuKedilkaman.Context.Interfaces;

namespace WebGpuKedilkaman.Context;

public unsafe class WebGPUContext : IWebGPUContext
{
    public IWindow WindowContext { get; }

    public unsafe Instance* Instance { get; }

    public unsafe Surface* Surface { get; }

    public unsafe Adapter* Adapter { get; }

    public unsafe Queue* Queue { get; }

    public unsafe CommandEncoder* CurrentCommandEncoder { get; }

    public SurfaceTexture SurfaceTexture { get; }

    public unsafe TextureView* SurfaceTextureView { get; }

    public WebGPU WGPU { get; }

    public unsafe Device* Device { get; }

    public unsafe RenderPassEncoder* CurrentRenderPassEncoder { get; }

    public WebGPUContext()
    {
        var windowOptions = WindowOptions.Default with
        {
            API = GraphicsAPI.None,
            Size = new Vector2D<int>(800, 600),
            Title = "LoKaki GPU"
        };

        WindowContext = Window.Create(windowOptions);
        WGPU = WebGPU.GetApi();
        var descriptor = new InstanceDescriptor();
        Instance = WGPU.CreateInstance(ref descriptor);
        Surface = WindowContext.CreateWebGPUSurface(WGPU, Instance);

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }


}
