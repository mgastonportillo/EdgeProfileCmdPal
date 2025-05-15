// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using System;
using System.Threading;

namespace EdgeProfileCmdPal;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            using ExtensionServer server = new();
            ManualResetEvent extensionDisposedEvent = new(false);
            EdgeProfileCmdPal extensionInstance = new(extensionDisposedEvent);

            server.RegisterExtension(() => extensionInstance);

            // We are instantiating an extension instance once above, and returning it every time the callback in RegisterExtension below is called.
            // This will make the main thread wait until the event is signalled by the extension class.
            // Since we have single instance of the extension object, we exit as soon as it is disposed.
            extensionDisposedEvent.WaitOne();
            // server.Stop();
            // server.UnsafeDispose();
        }
        else
        {
            Console.WriteLine("Not being launched as a Extension... exiting.");
        }
    }
}
