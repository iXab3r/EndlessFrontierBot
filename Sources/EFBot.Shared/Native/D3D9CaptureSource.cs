using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Reactive.Subjects;
using EFBot.Shared.Scaffolding;
using ReactiveUI;
using SlimDX.Direct3D9;

namespace EFBot.Shared.Native
{
    public sealed class D3D9CaptureSource : DisposableReactiveObject, IWindowCaptureSource
    {
        private static readonly Direct3D Direct3D9 = new Direct3D();
        private static readonly ConcurrentDictionary<IntPtr, Device> Direct3DDeviceCache = new ConcurrentDictionary<IntPtr, Device>();

        public AdapterInformation DefaultAdapterInfo => Direct3D9.Adapters.DefaultAdapter;

        public Bitmap Capture(IntPtr handle)
        {
            var region = NativeMethods.GetAbsoluteClientRect(handle);
            return Capture(handle, region);
        }
        
        public Bitmap Capture(IntPtr handle, Rectangle region)
        {
            var hWnd = handle;
            Bitmap bitmap = null;

            var device = Direct3DDeviceCache.GetOrAdd(hWnd, x => CreateDevice(hWnd));
            if (region.IsEmpty)
            {
                region = NativeMethods.GetAbsoluteClientRect(hWnd);
            }

            using (var surface = Surface.CreateOffscreenPlain(device, DefaultAdapterInfo.CurrentDisplayMode.Width, DefaultAdapterInfo.CurrentDisplayMode.Height, Format.A8R8G8B8, Pool.SystemMemory))
            {
                device.GetFrontBufferData(0, surface);
                bitmap = new Bitmap(Surface.ToStream(surface, ImageFileFormat.Bmp, new Rectangle(0,0, 100, 100)));
            }

            return bitmap;
        }

        private Device CreateDevice(IntPtr hWnd)
        {
            var parameters = new PresentParameters {BackBufferFormat = DefaultAdapterInfo.CurrentDisplayMode.Format};
            var clientRect = NativeMethods.GetAbsoluteClientRect(hWnd);
            parameters.BackBufferHeight = clientRect.Height;
            parameters.BackBufferWidth = clientRect.Width;
            parameters.Multisample = MultisampleType.None;
            parameters.SwapEffect = SwapEffect.Discard;
            parameters.DeviceWindowHandle = hWnd;
            parameters.PresentationInterval = PresentInterval.Default;
            parameters.FullScreenRefreshRateInHertz = 0;

            // Create the Direct3D device
            var result = new Device(
                Direct3D9, 
                DefaultAdapterInfo.Adapter, 
                DeviceType.Hardware, 
                hWnd, 
                CreateFlags.SoftwareVertexProcessing, 
                parameters);

            return result;
        }
    }
}