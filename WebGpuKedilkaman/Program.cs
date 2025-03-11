
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using WebGpuKedilkaman;
using WebGpuKedilkaman.Pipelines;

var serviceCollection = new ServiceCollection();
// Initialize the window without any api
var windowOptions = WindowOptions.Default with
{
    API = GraphicsAPI.None,
    Size = new Vector2D<int>(800, 600),
    Title = "LoKaki GPU"
};

var window = Window.Create(windowOptions);
window.Initialize();
serviceCollection.AddSingleton(window);
// Initialize the WebGPU API
// var webGPUAPI = WebGPU.GetApi();
// serviceCollection.AddSingleton(webGPUAPI);
// var descriptor = new InstanceDescriptor();

// unsafe
// {
//     var instance = webGPUAPI.CreateInstance(ref descriptor);
//     // We can't 
//     serviceCollection.AddSingleton();
// }


// Engine engine = new Engine();

// UnlitRenderPipeline unlitRenderPipeline = new UnlitRenderPipeline(engine);

// engine.OnInitialize += () =>
// {
//     unlitRenderPipeline.Initialize();
// };
// engine.OnRender += () =>
// {
//     unlitRenderPipeline.Render();
// };

// engine.Initialize();
// engine.Dispose();