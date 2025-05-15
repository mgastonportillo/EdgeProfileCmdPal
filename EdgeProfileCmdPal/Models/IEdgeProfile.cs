using Microsoft.CommandPalette.Extensions.Toolkit;

namespace EdgeProfileCmdPal.Models
{
    // Interface for Edge profile model
    public interface IEdgeProfile
    {
        string Name { get; }
        string Basename { get; }
        string Path { get; }
        IconInfo Icon { get; }
        string CommandArgs { get; }
    }

    // Implementation of the Edge profile model
    public class EdgeProfile : IEdgeProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Basename { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public IconInfo Icon { get; set; } = new IconInfo(string.Empty);
        public string CommandArgs { get; set; } = string.Empty;
    }
}