using System;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace WebGpuKedilkaman.Context.Interfaces;

public unsafe interface IWebGPUContext : IDisposable
{
    /// <summary>
    /// Our regular window
    /// </summary>
    IWindow WindowContext { get; }

    /// <summary>
    /// In WebGPU, an instance is the main entry point for interacting with the WebGPU API.
    /// It is similar to a VkInstance in Vulkan or an EGLDisplay in OpenGL.
    /// Similar to the GL Context in openGL
    /// </summary>
    Instance* Instance { get; }

    /// <summary>
    /// In WebGPU, a surface is an abstraction that represents a platform-specific window or canvas where the GPU can render.
    /// It is similar to an OpenGL framebuffer or swap chain, but it is managed explicitly.
    /// </summary>
    Surface* Surface { get; }

    /// <summary>
    /// In WebGPU, an adapter represents a physical or virtual GPU that your application can use.
    /// It is similar to an OpenGL graphics driver selection or a Vulkan physical device.
    /// 
    /// How It Works:
    /// Query Available GPUs: The browser or system provides a list of available GPUs.
    /// Select an Adapter: WebGPU picks a suitable one (integrated or discrete GPU).
    /// Create a Device: Once an adapter is selected, a device is created to interact with the GPU.
    /// </summary>
    Adapter* Adapter { get; }

    /// <summary>
    /// In WebGPU, a queue is responsible for submitting commands to the GPU.
    /// It acts like a command buffer manager in Vulkan or a draw call manager in OpenGL.
    /// 
    /// How It Works:
    /// Every Device Has One Queue → When you create a GPUDevice, it comes with a default queue.
    /// Commands Are Encoded → You create a GPUCommandEncoder to record GPU operations.
    /// Commands Are Submitted → The recorded commands are sent to the queue for execution.
    /// </summary>
    Queue* Queue { get; }

    /// <summary>
    /// In WebGPU, a command encoder is responsible for recording commands that will be submitted to the GPU for execution.
    /// It acts like a command buffer in Vulkan or a display list in OpenGL.
    /// 
    /// How It Works:
    /// 1. Create a Command Encoder: You create a GPUCommandEncoder to start recording commands.
    /// 2. Record Commands: You record various GPU operations like drawing, copying, etc.
    /// 3. Finish Encoding: Once all commands are recorded, you finish the encoding process to get a command buffer.
    /// 4. Submit Commands: The command buffer is then submitted to the GPU queue for execution.
    /// 
    /// Example Usage:
    /// var commandEncoder = device.CreateCommandEncoder();
    /// var renderPass = commandEncoder.BeginRenderPass(renderPassDescriptor);
    /// renderPass.SetPipeline(renderPipeline);
    /// renderPass.Draw(vertexCount, instanceCount, firstVertex, firstInstance);
    /// renderPass.EndPass();
    /// var commandBuffer = commandEncoder.Finish();
    /// queue.Submit(new[] { commandBuffer });
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you create a VkCommandBuffer and record commands into it. The process is similar but more verbose.
    /// Example:
    /// VkCommandBufferAllocateInfo allocInfo = {};
    /// vkAllocateCommandBuffers(device, &allocInfo, &commandBuffer);
    /// vkBeginCommandBuffer(commandBuffer, &beginInfo);
    /// vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    /// vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);
    /// vkCmdDraw(commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
    /// vkCmdEndRenderPass(commandBuffer);
    /// vkEndCommandBuffer(commandBuffer);
    /// vkQueueSubmit(queue, 1, &submitInfo, VK_NULL_HANDLE);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you use display lists or direct state access to record and execute commands.
    /// Example:
    /// GLuint list = glGenLists(1);
    /// glNewList(list, GL_COMPILE);
    /// glBindPipeline(GL_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);
    /// glDrawArrays(GL_TRIANGLES, firstVertex, vertexCount);
    /// glEndList();
    /// glCallList(list);
    /// </summary>
    CommandEncoder* CurrentCommandEncoder { get; }

    /// <summary>
    /// In WebGPU, a surface texture represents the actual image that is rendered to the screen.
    /// It is similar to a framebuffer in OpenGL or a swapchain image in Vulkan.
    /// 
    /// How It Works:
    /// 1. Acquire Surface Texture: The GPU acquires a texture from the surface to render into.
    /// 2. Render to Texture: Commands are recorded to render into the texture.
    /// 3. Present Texture: The texture is presented to the screen.
    /// 
    /// Example Usage:
    /// var surfaceTexture = swapChain.GetCurrentTexture();
    /// var textureView = surfaceTexture.CreateView();
    /// var renderPassDescriptor = new RenderPassDescriptor
    /// {
    ///     ColorAttachments = new[]
    ///     {
    ///         new RenderPassColorAttachment
    ///         {
    ///             View = textureView,
    ///             LoadOp = LoadOp.Clear,
    ///             StoreOp = LoadOp.Store,
    ///             ClearColor = new Color(0, 0, 0, 1)
    ///         }
    ///     }
    /// };
    /// var commandEncoder = device.CreateCommandEncoder();
    /// var renderPass = commandEncoder.BeginRenderPass(renderPassDescriptor);
    /// renderPass.SetPipeline(renderPipeline);
    /// renderPass.Draw(vertexCount, instanceCount, firstVertex, firstInstance);
    /// renderPass.EndPass();
    /// var commandBuffer = commandEncoder.Finish();
    /// queue.Submit(new[] { commandBuffer });
    /// surfaceTexture.Present();
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you acquire an image from the swapchain, render to it, and then present it.
    /// Example:
    /// vkAcquireNextImageKHR(device, swapChain, timeout, semaphore, fence, &imageIndex);
    /// VkCommandBuffer commandBuffer = BeginCommandBuffer();
    /// vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    /// vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);
    /// vkCmdDraw(commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
    /// vkCmdEndRenderPass(commandBuffer);
    /// EndCommandBuffer(commandBuffer);
    /// VkSubmitInfo submitInfo = {};
    /// vkQueueSubmit(queue, 1, &submitInfo, VK_NULL_HANDLE);
    /// vkQueuePresentKHR(queue, &presentInfo);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you render directly to the default framebuffer or a framebuffer object (FBO).
    /// Example:
    /// glBindFramebuffer(GL_FRAMEBUFFER, framebuffer);
    /// glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    /// glBindPipeline(GL_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);
    /// glDrawArrays(GL_TRIANGLES, firstVertex, vertexCount);
    /// glBindFramebuffer(GL_FRAMEBUFFER, 0);
    /// glfwSwapBuffers(window);
    /// </summary>
    SurfaceTexture SurfaceTexture { get; }

    /// <summary>
    /// In WebGPU, a texture view is a specific view into a texture, allowing you to interpret the texture data in different ways.
    /// It is similar to a texture view in Vulkan or a texture target in OpenGL.
    /// 
    /// How It Works:
    /// 1. Create Texture: You create a texture with specific properties.
    /// 2. Create Texture View: You create a view into the texture, specifying how the data should be interpreted.
    /// 3. Use Texture View: The texture view is used in various GPU operations like rendering or sampling.
    /// 
    /// Example Usage:
    /// var texture = device.CreateTexture(textureDescriptor);
    /// var textureView = texture.CreateView(textureViewDescriptor);
    /// var renderPassDescriptor = new RenderPassDescriptor
    /// {
    ///     ColorAttachments = new[]
    ///     {
    ///         new RenderPassColorAttachment
    ///         {
    ///             View = textureView,
    ///             LoadOp = LoadOp.Clear,
    ///             StoreOp = LoadOp.Store,
    ///             ClearColor = new Color(0, 0, 0, 1)
    ///         }
    ///     }
    /// };
    /// var commandEncoder = device.CreateCommandEncoder();
    /// var renderPass = commandEncoder.BeginRenderPass(renderPassDescriptor);
    /// renderPass.SetPipeline(renderPipeline);
    /// renderPass.Draw(vertexCount, instanceCount, firstVertex, firstInstance);
    /// renderPass.EndPass();
    /// var commandBuffer = commandEncoder.Finish();
    /// queue.Submit(new[] { commandBuffer });
    /// surfaceTexture.Present();
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you create a VkImageView to interpret the data in a VkImage.
    /// Example:
    /// VkImageViewCreateInfo viewInfo = {};
    /// vkCreateImageView(device, &viewInfo, nullptr, &imageView);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you use texture targets to interpret the data in a texture.
    /// Example:
    /// glBindTexture(GL_TEXTURE_2D, texture);
    /// glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
    /// </summary>
    TextureView* SurfaceTextureView { get; }

    /// <summary>
    /// WebGPU is the main entry point for interacting with the WebGPU API.
    /// It provides methods for creating instances, devices, and other WebGPU objects.
    /// 
    /// How It Works:
    /// 1. Create Instance: You create an instance of WebGPU.
    /// 2. Create Device: You create a device from the instance.
    /// 3. Use Device: The device is used to create and manage GPU resources.
    /// 
    /// Example Usage:
    /// var instance = WebGPU.CreateInstance();
    /// var adapter = instance.RequestAdapter(adapterOptions);
    /// var device = adapter.RequestDevice(deviceDescriptor);
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you create a VkInstance and VkDevice to interact with the GPU.
    /// Example:
    /// VkInstanceCreateInfo createInfo = {};
    /// vkCreateInstance(&createInfo, nullptr, &instance);
    /// VkDeviceCreateInfo deviceInfo = {};
    /// vkCreateDevice(physicalDevice, &deviceInfo, nullptr, &device);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you use the OpenGL context to interact with the GPU.
    /// Example:
    /// glfwMakeContextCurrent(window);
    /// gladLoadGLLoader((GLADloadproc)glfwGetProcAddress);
    /// </summary>
    public WebGPU WGPU { get; }

    /// <summary>
    /// In WebGPU, a device represents a logical connection to a physical or virtual GPU.
    /// It is used to create and manage GPU resources like buffers, textures, and pipelines.
    /// 
    /// How It Works:
    /// 1. Request Device: You request a device from an adapter.
    /// 2. Create Resources: You use the device to create GPU resources.
    /// 3. Submit Commands: You submit commands to the device's queue for execution.
    /// 
    /// Example Usage:
    /// var device = adapter.RequestDevice(deviceDescriptor);
    /// var buffer = device.CreateBuffer(bufferDescriptor);
    /// var texture = device.CreateTexture(textureDescriptor);
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you create a VkDevice to interact with the GPU.
    /// Example:
    /// VkDeviceCreateInfo deviceInfo = {};
    /// vkCreateDevice(physicalDevice, &deviceInfo, nullptr, &device);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you use the OpenGL context to create and manage GPU resources.
    /// Example:
    /// GLuint buffer;
    /// glGenBuffers(1, &buffer);
    /// glBindBuffer(GL_ARRAY_BUFFER, buffer);
    /// </summary>
    public Device* Device { get; }

    /// <summary>
    /// WebGPU, a texture format specifies the format of the texture data, such as the number of color channels and their bit depth.
    /// It is similar to a texture format in Vulkan or OpenGL.
    /// 
    /// How It Works:
    /// 1. Choose Format: You choose a texture format based on your rendering needs.
    /// 2. Create Texture: You create a texture with the chosen format.
    /// 3. Use Texture: The texture is used in various GPU operations like rendering or sampling.
    /// 
    /// Example Usage:
    /// var textureDescriptor = new TextureDescriptor
    /// {
    ///     Format = TextureFormat.Bgra8Unorm,
    ///     Width = 1024,
    ///     Height = 1024,
    ///     Usage = TextureUsage.RenderAttachment | TextureUsage.Sampled
    /// };
    /// var texture = device.CreateTexture(textureDescriptor);
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you specify the format of a VkImage when creating it.
    /// Example:
    /// VkImageCreateInfo imageInfo = {};
    /// imageInfo.format = VK_FORMAT_B8G8R8A8_UNORM;
    /// vkCreateImage(device, &imageInfo, nullptr, &image);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you specify the format of a texture when creating it.
    /// Example:
    /// glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, width, height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data);
    /// </summary>
    public TextureFormat PreferredTextureFormat => TextureFormat.Bgra8Unorm;

    /// <summary>
    /// In WebGPU, a render pass encoder is responsible for recording rendering commands within a render pass.
    /// It is similar to a render pass in Vulkan or a draw call in OpenGL.
    /// 
    /// How It Works:
    /// 1. Begin Render Pass: You begin a render pass with a render pass descriptor.
    /// 2. Record Commands: You record various rendering commands like setting pipelines and drawing.
    /// 3. End Render Pass: You end the render pass to finalize the recorded commands.
    /// 
    /// Example Usage:
    /// var renderPassDescriptor = new RenderPassDescriptor
    /// {
    ///     ColorAttachments = new[] { colorAttachment }
    /// };
    /// var renderPassEncoder = commandEncoder.BeginRenderPass(renderPassDescriptor);
    /// renderPassEncoder.SetPipeline(renderPipeline);
    /// renderPassEncoder.Draw(vertexCount, instanceCount, firstVertex, firstInstance);
    /// renderPassEncoder.EndPass();
    /// 
    /// Vulkan Comparison:
    /// In Vulkan, you use a VkRenderPass and VkCommandBuffer to record rendering commands.
    /// Example:
    /// vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    /// vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);
    /// vkCmdDraw(commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
    /// vkCmdEndRenderPass(commandBuffer);
    /// 
    /// OpenGL Comparison:
    /// In OpenGL, you use draw calls to record rendering commands.
    /// Example:
    /// glBindPipeline(GL_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);
    /// glDrawArrays(GL_TRIANGLES, firstVertex, vertexCount);
    /// </summary>
    RenderPassEncoder* CurrentRenderPassEncoder { get; }
}
