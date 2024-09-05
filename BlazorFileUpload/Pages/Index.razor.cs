using Microsoft.AspNetCore.Components.Forms;

namespace BlazorFileUpload.Pages;

public partial class Index
{
    private string fileName = "";
    private string linkUrl = "";
    private long fileSize;
    private string errorMessage = "";

    // The 'HandleFileSelection' method is called when a file is selected within the inputfield.
    // It checks the file ending and file size and saves the file to the wwwroot folder.
    private async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        fileName = e.File.Name;
        fileSize = e.File.Size;
        linkUrl = "";
        errorMessage = "";

        if (!IsTextFile()) return; //guard clauses
        if (IsTooLargeFile()) return;

        using var readStream = e.File.OpenReadStream(GetMaxFileSize());
        using var writeStream = new FileStream($"wwwroot/{fileName}", FileMode.Create);

        try
        {
            await readStream.CopyToAsync(writeStream);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            Logger.LogWarning("File '{fileName}' not saved ({errorMessage}).", fileName, errorMessage);
            return;
        }

        linkUrl = fileName;
        Logger.LogInformation("File '{fileName}' saved.", fileName);
    }

    private bool IsTextFile()
    {
        if (fileName.EndsWith(".txt"))
        {
            return true;
        }

        errorMessage = "File-ending must be '.txt'.";
        Logger.LogWarning("File '{fileName}' is no txt-file.", fileName);
        return false;
    }

    private bool IsTooLargeFile()
    {
        var maxFileSize = GetMaxFileSize();

        if (fileSize <= maxFileSize)
        {
            return false;
        };

        errorMessage = "File is too large.";
        Logger.LogWarning("File '{fileName}' larger than {maxFileSize} ({fileSize} bytes).", fileName, maxFileSize, fileSize);
        return true;
    }

    private long GetMaxFileSize()
    {
        var defaultMaxFileSize = 1024 * 1024 * 2; // 2 MB

        var maxFileSizeString = Configuration["MaxFileSize"];

        if (string.IsNullOrEmpty(maxFileSizeString))
        {
            return defaultMaxFileSize;
        }

        if (!long.TryParse(maxFileSizeString, out var maxFileSize))
        {
            return defaultMaxFileSize;
        }

        return maxFileSize;
    }
}
