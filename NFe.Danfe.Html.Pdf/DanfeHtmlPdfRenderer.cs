using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NFe.Danfe.Html.Interfaces;

namespace NFe.Danfe.Html.Pdf
{
    public sealed class DanfeHtmlPdfRenderer : IDanfeHtmlPdfRenderer
    {
        public async Task<byte[]> RenderizarPdfAsync(
            IDanfeHtml2 danfeHtml,
            DanfeHtmlPdfOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (danfeHtml == null)
                throw new ArgumentNullException(nameof(danfeHtml));

            cancellationToken.ThrowIfCancellationRequested();

            var documento = await danfeHtml.ObterDocHtmlAsync().ConfigureAwait(false);
            if (documento == null)
                throw new InvalidOperationException("O gerador de DANFE HTML retornou um documento nulo.");

            return await RenderizarPdfDeHtmlAsync(documento.Html, options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> RenderizarPdfDeHtmlAsync(
            string html,
            DanfeHtmlPdfOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("O HTML do DANFE nao pode ser vazio.", nameof(html));

            options = options ?? new DanfeHtmlPdfOptions();
            cancellationToken.ThrowIfCancellationRequested();

            using (var playwright = await Playwright.CreateAsync().ConfigureAwait(false))
            {
                IBrowser browser = null;
                IBrowserContext context = null;

                try
                {
                    browser = await playwright.Chromium.LaunchAsync(CriarLaunchOptions(options)).ConfigureAwait(false);
                    context = await browser.NewContextAsync().ConfigureAwait(false);

                    var page = await context.NewPageAsync().ConfigureAwait(false);
                    page.SetDefaultTimeout(options.TimeoutMilissegundos);

                    if (options.EmularMidiaImpressao)
                        await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Print }).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    await page.SetContentAsync(
                        html,
                        new PageSetContentOptions
                        {
                            WaitUntil = WaitUntilState.Load,
                            Timeout = options.TimeoutMilissegundos
                        }).ConfigureAwait(false);

                    if (options.AguardarNetworkIdle)
                    {
                        await page.WaitForLoadStateAsync(
                            LoadState.NetworkIdle,
                            new PageWaitForLoadStateOptions { Timeout = options.TimeoutMilissegundos }).ConfigureAwait(false);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    return await page.PdfAsync(CriarPdfOptions(options)).ConfigureAwait(false);
                }
                catch (PlaywrightException ex) when (BrowserNaoInstalado(ex))
                {
                    throw new InvalidOperationException(
                        "Chromium do Playwright nao foi encontrado. Execute o script 'playwright install chromium' gerado no output do app/teste, ou informe DanfeHtmlPdfOptions.CaminhoExecutavelChromium.",
                        ex);
                }
                finally
                {
                    if (context != null)
                        await context.CloseAsync().ConfigureAwait(false);

                    if (browser != null)
                        await browser.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        private static BrowserTypeLaunchOptions CriarLaunchOptions(DanfeHtmlPdfOptions options)
        {
            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = true,
                Timeout = options.TimeoutMilissegundos
            };

            if (!string.IsNullOrWhiteSpace(options.CaminhoExecutavelChromium))
                launchOptions.ExecutablePath = options.CaminhoExecutavelChromium;

            if (!string.IsNullOrWhiteSpace(options.CanalBrowser))
                launchOptions.Channel = options.CanalBrowser;

            var argumentos = options.CriarArgumentosChromium();
            if (argumentos.Length > 0)
                launchOptions.Args = argumentos;

            return launchOptions;
        }

        private static PagePdfOptions CriarPdfOptions(DanfeHtmlPdfOptions options)
        {
            var pdfOptions = new PagePdfOptions
            {
                Format = options.FormatoPapel,
                PrintBackground = options.ImprimirPlanoDeFundo,
                PreferCSSPageSize = options.PreferirTamanhoCss
            };

            if (options.Escala.HasValue)
                pdfOptions.Scale = options.Escala.Value;

            return pdfOptions;
        }

        private static bool BrowserNaoInstalado(PlaywrightException ex)
        {
            return ex.Message.IndexOf("Executable doesn't exist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   ex.Message.IndexOf("Looks like Playwright was just installed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   ex.Message.IndexOf("Please run the following command", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
