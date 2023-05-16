namespace Automatix.YTUploader.Constants;

public static class YouTube
{
    public const string BtnCreateVideoXpath = "//yt-icon[@class=\"style-scope ytd-topbar-menu-button-renderer\"]";

    public const string BtnAddVideoXpath = "//a[@href=\"/upload\"]";

    public const string InputUploadVideoXpath = "//input[@type=\"file\"]";

    public const string InputVideoNameXpath = "//div[@id=\"textbox\"][1]";

    public const string BtnNextXpath = "//ytcp-button[@id=\"next-button\"]";

    public const string BtnDoneXpath = "//ytcp-button[@id=\"done-button\"]";

    public const string LinkToVideoXpath = "//ytcp-video-thumbnail-with-info[@class=\"style-scope ytcp-video-share-dialog\"]";
}