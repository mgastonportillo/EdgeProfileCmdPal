using EdgeProfileCmdPal.Commands;
using EdgeProfileCmdPal.Helpers;
using EdgeProfileCmdPal.Models;
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
                    string profileBasename = profile.Name;
                    string profilePath = Path.Combine(edgeDataFolder, profileBasename);

                    if (!Directory.Exists(profilePath))
                    {
                        return null;
                    }

                    var profileData = profile.Value;

                    // Handle icon
                    string iconFileName = profileData.TryGetProperty("gaia_picture_file_name", out var iconProp) && iconProp.ValueKind == JsonValueKind.String
                        ? iconProp.GetString() ?? "Edge Profile Picture.png"
                        : "Edge Profile Picture.png";
                    string iconPath = Path.Combine(profilePath, iconFileName);
                    string roundIconPath = ImageHelpers.ClipToCircle(iconPath);
                    var profileIcon = new IconInfo(roundIconPath);

                    var edgeProfile = new EdgeProfile
                    {
                        Name = profileData.GetProperty("name").GetString() ?? profileBasename,
                        Basename = profileBasename,
                        Path = profilePath,
                        Icon = profileIcon,
                        CommandArgs = $"--profile-directory=\"{profileBasename}\"",
                    };

                    return new ListItem(new OpenCommand(edgeProfile))
                    {
                        Title = edgeProfile.Name,
                        Subtitle = edgeProfile.Basename,
                        Icon = edgeProfile.Icon,
                    };
                })
                .Where(item => item != null)
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