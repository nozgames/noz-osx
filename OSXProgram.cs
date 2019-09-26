using System;
using System.IO;
using UIKit;

namespace NoZ.Platform.OSX {
    public static class Program {
        internal static ProgramContext Context { get; set; }

        public static void Main(ProgramContext context) {
            Context = context;
            UIApplication.Main(context.Args, null, "AppDelegate");
        }
    }
}