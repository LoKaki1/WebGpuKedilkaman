using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using WebGpuKedilkaman.Context.Interfaces;
using WebGpuKedilkaman.Extensions;

namespace WebGpuKedilkaman.Context;

public unsafe class WebGPUContext : IWebGPUContext
{
    public IWindow WindowContext { get; }

    public unsafe Instance* Instance { get; }

    public unsafe Surface* Surface { get; }

    public unsafe Adapter* Adapter { get; private set; }

    public unsafe Queue* Queue { get; }

    public unsafe CommandEncoder* CurrentCommandEncoder { get; }

    public SurfaceTexture SurfaceTexture { get; }

    public unsafe TextureView* SurfaceTextureView { get; }

    public WebGPU WGPU { get; }

    public unsafe Device* Device { get; }

    public unsafe RenderPassEncoder* CurrentRenderPassEncoder { get; }

    public TextureFormat PreferredTextureFormat { get; }

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
        var options = new RequestAdapterOptions
        {
            CompatibleSurface = Surface,
            BackendType = BackendType.Metal,
            PowerPreference = PowerPreference.HighPerformance
        };

        PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From(
            (status, wgpuAdapter, msgPtr, userDataPtr) =>
            {
                if (status == RequestAdapterStatus.Success)
                {
                    Adapter = wgpuAdapter;
                }
                else
                {
                    string msg = BytesExtensions.ToString(msgPtr);

                    throw new ArgumentException(msg);
                }
            });

        WGPU.InstanceRequestAdapter(Instance, ref options, callback, null);

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }


}
