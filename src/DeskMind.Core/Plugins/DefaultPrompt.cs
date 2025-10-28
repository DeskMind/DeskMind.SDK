namespace DeskMind.Core.Plugins
{
    // Simple model for a default prompt suggestion
    public class DefaultPrompt
    {
        public string Header { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // Optional icon glyph (e.g., Segoe MDL2 Assets code like "\uE707" or emoji)
        public string Icon { get; set; } = "\uE722"; // default to a search-like icon
    }
}

