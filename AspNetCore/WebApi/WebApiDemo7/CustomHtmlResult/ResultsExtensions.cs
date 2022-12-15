using Microsoft.AspNetCore.Http.Metadata;

using System.Net.Mime;
using System.Reflection;
using System.Text;

namespace WebApiDemo7.CustomHtmlResult
{
    public static class ResultsExtensions
    {
        public static IResult Html(this IResultExtensions resultExtensions, string html)
        {
            ArgumentNullException.ThrowIfNull(resultExtensions);

            return new HtmlResult(html);
        }
    }


    class HtmlResult : IResult, IEndpointMetadataProvider
    {
        private readonly string _html;

        public HtmlResult(string html)
        {
            _html = html;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
            return httpContext.Response.WriteAsync(_html);
        }

        public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        {
            builder.Metadata.Add(new ProducesHtmlMetadata());
        }
    }

    internal sealed class ProducesHtmlMetadata : IProducesResponseTypeMetadata
    {
        public Type? Type => null;

        public int StatusCode => 200;

        public IEnumerable<string> ContentTypes { get; } = new[] { MediaTypeNames.Text.Html };
    }
}
