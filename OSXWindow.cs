﻿using Foundation;
using CoreAnimation;
using UIKit;
using OpenGLES;
using GLKit;

using NoZ.Platform.OpenGL;
using System.Diagnostics;
using System;

namespace NoZ.Platform.OSX
{
    internal class NozView : GLKView
    {
        private IOSGameWindow _window;

        internal NozView(IOSGameWindow window, CoreGraphics.CGRect rect) : base(rect)
        {
            _window = window;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            _window.OnTouchesBegan(touches);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            _window.OnTouchesMoved(touches);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            _window.OnTouchesEnded(touches);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            _window.OnTouchesCancelled(touches);
        }

        public override bool CanBecomeFirstResponder => true;
    }

    public class NozNavigationController : UINavigationController
    {
        public NozNavigationController()
        {
            NavigationBarHidden = true;
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }
    }

    internal class IOSGameWindow : GameWindow
    {
        public EAGLContext GLContext {
            get; set;
        }

        public UIWindow Window {
            get; set;
        }

        public override Vector2Int Size => _displaySize;

        private uint _renderBufferId;
        private uint _frameBufferId;
        private NozView _view;
        private Vector2Int _displaySize;

        public IOSGameWindow(AppDelegate appDelegate)
        {
            _displaySize = new Vector2Int(
                (int)UIScreen.MainScreen.NativeBounds.Size.Width,
                (int)UIScreen.MainScreen.NativeBounds.Size.Height
                );

            _stopwatch = new Stopwatch();
            _stopwatch2 = new Stopwatch();


            // Create the window
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            Window.RootViewController = new NozNavigationController();
            Window.BackgroundColor = UIColor.White;
            Window.MakeKeyAndVisible();

            appDelegate.Window = Window;

            // Create GL context
            GLContext = new EAGLContext(EAGLRenderingAPI.OpenGLES3);
            EAGLContext.SetCurrentContext(GLContext);

            // Create teh view
            _view = new NozView(this, Window.Bounds);
            _view.MultipleTouchEnabled = true;
            _view.UserInteractionEnabled = true;
            Window.AddSubview(_view);

            // Create render buffer..
            _renderBufferId = GL.GenRenderBuffer();
            GL.BindRenderBuffer(_renderBufferId);
            GLContext.RenderBufferStorage((uint)GL.Imports.GL_RENDERBUFFER, (CAEAGLLayer)_view.Layer);

            // Create frame buffer
            _frameBufferId = GL.GenFrameBuffer();
            GL.BindFrameBuffer(_frameBufferId);
            GL.FrameBufferRenderBuffer(_renderBufferId);

            var link = CADisplayLink.Create(() =>
            {
                Game.Instance.Frame();
            });
            link.PreferredFramesPerSecond = 60;

            link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
        }

        protected override void OnBeginFrame()
        {
            _stopwatch.Start();
            _stopwatch2.Restart();

            EAGLContext.SetCurrentContext(GLContext);
            GL.BindFrameBuffer(_frameBufferId);
        }

        protected override void OnEndFrame()
        {
            GL.BindRenderBuffer(_renderBufferId);
            GLContext.PresentRenderBuffer((uint)GL.Imports.GL_RENDERBUFFER);


            _stopwatch2.Stop();

            if (_stopwatch.ElapsedMilliseconds > 1000)
            {
                _stopwatch.Stop();

                var lastFPS = (((double)frameCount) / (avgElapsed / 1000.0));

                avgElapsed = avgElapsed / frameCount;

                if (lastFPS >= 1000)
                    Console.WriteLine($"(1000+ FPS  {minElapsed:0.00}ms < {avgElapsed:0.00}ms < {maxElapsed:0.00}ms  {Node.TotalNodes} Nodes) ");
                else
                    Console.WriteLine($"({(int)lastFPS} FPS  {minElapsed:0.00}ms < {avgElapsed:0.00}ms < {maxElapsed:0.00}ms  {Node.TotalNodes} Nodes)");

                _stopwatch.Restart();
                frameCount = 0;
                minElapsed = 100000;
                maxElapsed = 0;
                avgElapsed = 0;
            }
            else
            {

                frameCount++;
                minElapsed = Math.Min(minElapsed, _stopwatch2.Elapsed.TotalMilliseconds);
                maxElapsed = Math.Max(maxElapsed, _stopwatch2.Elapsed.TotalMilliseconds);
                avgElapsed += _stopwatch2.Elapsed.TotalMilliseconds;
            }
        }

        private Stopwatch _stopwatch;
        private Stopwatch _stopwatch2;
        private int frameCount = 0;
        private double minElapsed = 0;
        private double maxElapsed = 0;
        private double avgElapsed = 0;


        protected override void Show()
        {

        }

        private Vector2 ViewToWindow(UITouch touch)
        {
            var point = touch.LocationInView(_view);
            var scale = UIScreen.MainScreen.Scale;
            return new Vector2((float)(point.X * scale), (float)(point.Y * scale));
        }

        internal void OnTouchesBegan(NSSet touches)
        {
            foreach (var touch in touches)
            {
                var uitouch = touch as UITouch;
                if (null == uitouch) continue;
                Raise(TouchBegan, (ulong)uitouch.Handle.ToInt64(), ViewToWindow(uitouch), (int)uitouch.TapCount);
            }
        }

        internal void OnTouchesEnded(NSSet touches)
        {
            foreach (var touch in touches)
            {
                var uitouch = touch as UITouch;
                if (null == uitouch) continue;
                Raise(TouchEnded, (ulong)uitouch.Handle.ToInt64(), ViewToWindow(uitouch), (int)uitouch.TapCount);
            }
        }

        internal void OnTouchesCancelled(NSSet touches)
        {
            foreach (var touch in touches)
            {
                var uitouch = touch as UITouch;
                if (null == uitouch) continue;
                Raise(TouchCancelled, (ulong)uitouch.Handle.ToInt64(), ViewToWindow(uitouch), (int)uitouch.TapCount);
            }
        }

        internal void OnTouchesMoved(NSSet touches)
        {
            foreach (var touch in touches)
            {
                var uitouch = touch as UITouch;
                if (null == uitouch) continue;
                Raise(TouchMoved, (ulong)uitouch.Handle.ToInt64(), ViewToWindow(uitouch), (int)uitouch.TapCount);
            }
        }

        protected override void SetCursor(Cursor cursor)
        {
            // TODO: IOS doesnt have cursors so just ignore the call.
        }
    }
}