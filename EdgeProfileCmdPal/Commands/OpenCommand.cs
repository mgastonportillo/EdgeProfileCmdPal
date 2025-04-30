using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Diagnostics;
using System.IO;

namespace EdgeProfileCmdPal.Commands
{
    internal sealed partial class OpenCommand : InvokableCommand
    {
        public override string Name => $"Open {_profileName}";
        private readonly string _profileName;

        public OpenCommand(string profileName)
        {
            _profileName = profileName;
        }

        public override CommandResult Invoke()
        {
            // Maybe add support for Canary or Dev, or even custom install dirs
            string edgeExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

            if (!File.Exists(edgeExecutablePath))
            {
                Debug.WriteLine($"[OpenCommand] Edge executable not found at: {edgeExecutablePath}");
                return CommandResult.Hide();
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = edgeExecutablePath,
                    Arguments = $"--profile-directory=\"{_profileName}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OpenCommand] Failed to launch Edge: {ex.Message}");
            }

            // Hide CmdPal after execution
            return CommandResult.Hide();
        }
    }
}
