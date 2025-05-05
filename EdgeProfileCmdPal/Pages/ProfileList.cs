using EdgeProfileCmdPal.Commands;
using EdgeProfileCmdPal.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EdgeProfileCmdPal;

internal sealed partial class ProfileList : ListPage
{
    public ProfileList()
    {
        Icon = IconHelpers.FromRelativePath(@"Assets\EdgeProfile.png");
        Title = "Open Edge by Profile";
        Name = "Select profile";
    }

    public override IListItem[] GetItems()
    {
        string edgeDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Edge\User Data";
        string localStateFile = Path.Combine(edgeDataFolder, "Local State");

        if (!Directory.Exists(edgeDataFolder) || !File.Exists(localStateFile))
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = $"Directory or file not found: '{edgeDataFolder}'",
                    Subtitle = "UserData folder or Local State file not found. Is MS Edge installed?"
                }
            ];
        }

        try
        {
            // Parse the "Local State" JSON file
            string localStateContent = File.ReadAllText(localStateFile);
            var jsonDoc = JsonDocument.Parse(localStateContent);
            var root = jsonDoc.RootElement;

            // Extract profiles from "info_cache"
            var profiles = root.GetProperty("profile")
                .GetProperty("info_cache")
                .EnumerateObject()
                .Select(profile =>
                {
                    string profilePath = profile.Name;
                    string profileDir = Path.Combine(edgeDataFolder, profilePath);

                    if (!Directory.Exists(profileDir))
                    {
                        return null; // Skip profiles without a valid directory
                    }

                    var profileData = profile.Value;
                    string profileName = profileData.GetProperty("name").GetString() ?? profilePath;

                    // Handle icon
                    string iconFileName = profileData.TryGetProperty("gaia_picture_file_name", out var iconProp) && iconProp.ValueKind == JsonValueKind.String
                        ? iconProp.GetString() ?? "Edge Profile Picture.png"
                        : "Edge Profile Picture.png";
                    string iconPath = Path.Combine(profileDir, iconFileName);
                    string roundIconPath = ImageHelpers.ClipToCircle(iconPath);
                    var profileIcon = new IconInfo(roundIconPath);

                    return new ListItem(new OpenCommand(profilePath))
                    {
                        Title = profileName,
                        Subtitle = profilePath,
                        Icon = profileIcon
                    };
                })
                .Where(item => item != null) // Remove null entries
                .Cast<ListItem>() // Cast as List<ListItem> (probably unsafe)
                .ToList();

            var filteredItems = ListHelpers.FilterList(profiles, SearchText);

            // Score and sort the filtered items
            var scoredItems = filteredItems
                .Select(item => new { Item = item, Score = ListHelpers.ScoreListItem(SearchText, item) })
                .OrderByDescending(scored => scored.Score)
                .Select(scored => scored.Item)
                .ToArray();

            return scoredItems;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProfileList] Error: {ex.Message}");
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Error",
                    Subtitle = "An unexpected error occurred."
                }
            ];
        }
    }
}