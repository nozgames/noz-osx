/*
  NoZ Game Engine

  Copyright(c) 2019 NoZ Games, LLC

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files(the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions :

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/

using Foundation;
using AppKit;

namespace NoZ.Platform.OSX
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private IOSGameWindow _window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            _window = new IOSGameWindow(this);

            var context = new GameContext
            {
                GraphicsDriver = new IOSGraphicsDriver(),
                AudioDriver = new IOSAudioDriver(),
                Name = Program.Context.Name,
                GameResourceName = Program.Context.GameResourceName,
                Window = _window,
                Args = Program.Context.Args,
                Resources = new ResourceDatabase(Program.Context.Archives)
            };

            Game.Start(context);
        }

        public override void WillTerminate(NSNotification notification)
        {
        }
    }
}


