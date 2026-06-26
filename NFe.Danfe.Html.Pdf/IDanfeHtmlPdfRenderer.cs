using System.Threading;
using System.Threading.Tasks;
using NFe.Danfe.Html.Interfaces;

namespace NFe.Danfe.Html.Pdf
{
    public interface IDanfeHtmlPdfRenderer
    {
        Task<byte[]> RenderizarPdfAsync(
            IDanfeHtml2 danfeHtml,
            DanfeHtmlPdfOptions options = null,
            CancellationToken cancellationToken = default);

        Task<byte[]> RenderizarPdfDeHtmlAsync(
            string html,
            DanfeHtmlPdfOptions options = null,
            CancellationToken cancellationToken = default);
    }
}
