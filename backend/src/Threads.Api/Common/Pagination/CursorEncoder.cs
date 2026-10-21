using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Threads.Api.Common.Pagination;

public static class CursorEncoder
{
    public static string Encode(DateTime lastDate, Guid lastId)
    {
        var cursor = new Cursor(lastDate, lastId);
        var jsonString = JsonSerializer.Serialize(cursor);
        var bytes = Encoding.UTF8.GetBytes(jsonString);
        var encodedCursor = Base64UrlTextEncoder.Encode(bytes);
        return encodedCursor;
    }

    public static Cursor? Decode(string encodedCursor)
    {
        if (string.IsNullOrEmpty(encodedCursor))
            return null;

        try
        {
            var bytes = Base64UrlTextEncoder.Decode(encodedCursor);
            var jsonString = Encoding.UTF8.GetString(bytes);
            var decodedCursor = JsonSerializer.Deserialize<Cursor>(jsonString);
            return decodedCursor;
        }
        catch
        {
            return null;
        }
    }
}
