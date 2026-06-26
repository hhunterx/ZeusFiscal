using System;
using System.Collections.Generic;
using System.Linq;

namespace NFe.Danfe.Html.Pdf
{
    public sealed class DanfeHtmlPdfOptions
    {
        public string FormatoPapel { get; set; } = "A4";

        public bool ImprimirPlanoDeFundo { get; set; } = true;

        public bool PreferirTamanhoCss { get; set; } = true;

        public bool EmularMidiaImpressao { get; set; } = true;

        public bool AguardarNetworkIdle { get; set; } = true;

        public bool DesabilitarSandbox { get; set; }

        public string CaminhoExecutavelChromium { get; set; }

        public string CanalBrowser { get; set; }

        public float? Escala { get; set; }

        public int TimeoutMilissegundos { get; set; } = 30000;

        public IReadOnlyCollection<string> ArgumentosChromium { get; set; } = Array.Empty<string>();

        internal string[] CriarArgumentosChromium()
        {
            var argumentos = new List<string>();

            if (DesabilitarSandbox)
                argumentos.Add("--no-sandbox");

            if (ArgumentosChromium != null)
                argumentos.AddRange(ArgumentosChromium.Where(x => !string.IsNullOrWhiteSpace(x)));

            return argumentos.ToArray();
        }
    }
}
