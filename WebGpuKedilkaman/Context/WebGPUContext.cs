using System;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using WebGpuKedilkaman.Context.Interfaces;

namespace WebGpuKedilkaman.Context;

public unsafe class WebGPUContext : IWebGPUContext
{
    public IWindow Window => throw new NotImplementedException();

    public unsafe Instance* Instance => throw new NotImplementedException();

    public unsafe Surface* Surface => throw new NotImplementedException();

    public unsafe Adapter* Adapter => throw new NotImplementedException();

    public unsafe Queue* Queue => throw new NotImplementedException();

    public unsafe CommandEncoder* CurrentCommandEncoder => throw new NotImplementedException();

    public SurfaceTexture SurfaceTexture => throw new NotImplementedException();

    public unsafe TextureView* SurfaceTextureView => throw new NotImplementedException();

    public WebGPU WGPU => throw new NotImplementedException();

    public unsafe Device* Device => throw new NotImplementedException();

    public unsafe RenderPassEncoder* CurrentRenderPassEncoder => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
