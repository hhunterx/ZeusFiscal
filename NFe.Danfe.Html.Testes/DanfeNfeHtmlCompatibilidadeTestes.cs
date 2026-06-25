using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Observacoes;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Danfe.Html.CrossCutting;
using NFe.Danfe.Html.Dominio;
using NFe.Danfe.Html.Interfaces;
using Xunit;
using NFeModel = NFe.Classes.NFe;

namespace NFe.Danfe.Html.Testes;

public class DanfeNfeHtmlCompatibilidadeTestes
{
    [Fact]
    public async Task ObterDocHtmlAsync_DeveGerarHtmlComCodigoDeBarrasSemPlaceholders()
    {
        var html = await GerarHtmlAsync(quantidadeProdutos: 2);

        Assert.False(string.IsNullOrWhiteSpace(html));
        Assert.Contains("DANFE", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EMPRESA TESTE LTDA", html);
        Assert.Contains("CLIENTE TESTE LTDA", html);
        Assert.Contains("Produto teste 001", html);
        Assert.Contains("data:image/png;base64", html);
        Assert.DoesNotContain("[next-page]", html);
        Assert.DoesNotContain("[BarCode]", html);
    }

    [Fact]
    public async Task ObterDocHtmlAsync_DeveGerarPaginasSubsequentesSemTagsInternas()
    {
        var html = await GerarHtmlAsync(quantidadeProdutos: 70);

        Assert.Contains("Produto teste 001", html);
        Assert.Contains("Produto teste 070", html);
        Assert.True(ContarOcorrencias(html, "class=\"page nfeArea\"") >= 2);
        Assert.DoesNotContain("[actual_page]", html);
        Assert.DoesNotContain("[total_pages]", html);
        Assert.DoesNotContain("[next-page]", html);
        Assert.DoesNotContain("[itens_products]", html);
    }

    private static int ContarOcorrencias(string texto, string valor)
    {
        var total = 0;
        var indice = 0;

        while ((indice = texto.IndexOf(valor, indice, StringComparison.Ordinal)) >= 0)
        {
            total++;
            indice += valor.Length;
        }

        return total;
    }

    private static async Task<string> GerarHtmlAsync(int quantidadeProdutos)
    {
        var nfe = CriarNfe(quantidadeProdutos);
        var danfe = new DanfeNFe(nfe, Status.Autorizada, "135240000000001", "HHunterx");
        IDanfeHtml2 htmlDanfe = new DanfeNfeHtml2(danfe);

        var documento = await htmlDanfe.ObterDocHtmlAsync();

        return documento.Html;
    }

    private static NFeModel CriarNfe(int quantidadeProdutos)
    {
        var produtos = CriarProdutos(quantidadeProdutos);
        var valorProdutos = quantidadeProdutos * 10m;
        var valorIcms = quantidadeProdutos * 1.8m;

        return new NFeModel
        {
            infNFe = new infNFe
            {
                versao = "4.00",
                Id = "NFe35240612345678000199550010000000011000000010",
                ide = CriarIdentificacao(),
                emit = CriarEmitente(),
                dest = CriarDestinatario(),
                det = produtos,
                total = new total
                {
                    ICMSTot = new ICMSTot
                    {
                        vBC = valorProdutos,
                        vICMS = valorIcms,
                        vBCST = 0m,
                        vST = 0m,
                        vProd = valorProdutos,
                        vFrete = 0m,
                        vSeg = 0m,
                        vDesc = 0m,
                        vOutro = 0m,
                        vII = 0m,
                        vIPI = 0m,
                        vPIS = 0m,
                        vCOFINS = 0m,
                        vNF = valorProdutos,
                        vTotTrib = 0m
                    }
                },
                transp = new transp
                {
                    modFrete = ModalidadeFrete.mfSemFrete
                },
                infAdic = new infAdic
                {
                    infCpl = "Informacoes adicionais para teste Linux."
                }
            }
        };
    }

    private static ide CriarIdentificacao()
    {
        return new ide
        {
            cUF = Estado.SP,
            cNF = "00000010",
            natOp = "VENDA DE MERCADORIA",
            mod = ModeloDocumento.NFe,
            serie = 1,
            nNF = 1,
            dhEmi = new DateTimeOffset(2024, 6, 24, 10, 30, 0, TimeSpan.FromHours(-3)),
            dhSaiEnt = new DateTimeOffset(2024, 6, 24, 10, 45, 0, TimeSpan.FromHours(-3)),
            tpNF = TipoNFe.tnSaida,
            idDest = DestinoOperacao.doInterna,
            cMunFG = 3550308,
            tpImp = TipoImpressao.tiRetrato,
            tpEmis = TipoEmissao.teNormal,
            cDV = 0,
            tpAmb = TipoAmbiente.Producao,
            finNFe = FinalidadeNFe.fnNormal,
            indFinal = ConsumidorFinal.cfNao,
            indPres = PresencaComprador.pcPresencial,
            procEmi = ProcessoEmissao.peAplicativoContribuinte,
            verProc = "TesteLinux"
        };
    }

    private static emit CriarEmitente()
    {
        return new emit
        {
            CNPJ = "12345678000199",
            xNome = "EMPRESA TESTE LTDA",
            xFant = "EMPRESA TESTE",
            IE = "110042490114",
            CRT = CRT.RegimeNormal,
            enderEmit = new enderEmit
            {
                xLgr = "Rua de Teste",
                nro = "100",
                xBairro = "Centro",
                cMun = 3550308,
                xMun = "Sao Paulo",
                UF = Estado.SP,
                CEP = "01001000",
                fone = 1133334444
            }
        };
    }

    private static dest CriarDestinatario()
    {
        return new dest(VersaoServico.Versao400)
        {
            CNPJ = "98765432000188",
            xNome = "CLIENTE TESTE LTDA",
            IE = "110042490115",
            enderDest = new enderDest
            {
                xLgr = "Avenida Cliente",
                nro = "200",
                xBairro = "Bairro Teste",
                cMun = 3550308,
                xMun = "Sao Paulo",
                UF = "SP",
                CEP = "01002000",
                fone = 1144445555
            }
        };
    }

    private static List<det> CriarProdutos(int quantidadeProdutos)
    {
        var produtos = new List<det>();

        for (var i = 1; i <= quantidadeProdutos; i++)
        {
            produtos.Add(new det
            {
                nItem = i,
                prod = new prod
                {
                    cProd = i.ToString("000"),
                    cEAN = "SEM GTIN",
                    xProd = $"Produto teste {i:000}",
                    NCM = "61091000",
                    CFOP = 5102,
                    uCom = "UN",
                    qCom = 1m,
                    vUnCom = 10m,
                    vProd = 10m,
                    cEANTrib = "SEM GTIN",
                    uTrib = "UN",
                    qTrib = 1m,
                    vUnTrib = 10m,
                    indTot = IndicadorTotal.ValorDoItemCompoeTotalNF
                },
                imposto = new imposto
                {
                    ICMS = new ICMS
                    {
                        TipoICMS = new ICMS00
                        {
                            orig = OrigemMercadoria.OmNacional,
                            CST = Csticms.Cst00,
                            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            vBC = 10m,
                            pICMS = 18m,
                            vICMS = 1.8m
                        }
                    }
                }
            });
        }

        return produtos;
    }
}
