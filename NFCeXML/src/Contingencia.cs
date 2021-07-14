using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSNFCeXML.src.Classes.NFCe;
using NSNFCeXML.src.Genericos;
using LayoutXML;

namespace ContingenciaNFCe
{
    class Contingencia
    {
        public static TNFCe aplicarContingencia(TNFCe NFCe)
        {
            //cria um objeto temporario para alterar os dados
            TNFCe NFCeXMLContingencia = NFCe;

            //altera os dados da NFCe para contingencia
            Genericos.gravarLinhaLog("65", "[ATIVANDO_CONTINGENCIA_NFCE]");

            Genericos.gravarLinhaLog("65", "[REALIZANDO_AJUSTES_CONTINGENCIA]");

            Genericos.gravarLinhaLog("65", "[ALTERANDO_tpEmis]");
            NFCeXMLContingencia.infNFe.ide.tpEmis = TNFeInfNFeIdeTpEmis.Item9;
            Genericos.gravarLinhaLog("65", "[tpEmis ALTERADO PARA "+ NFCeXMLContingencia.infNFe.ide.tpEmis.ToString() + "]");

            Genericos.gravarLinhaLog("65", "[INSERINDO_dhCont]");
            NFCeXMLContingencia.infNFe.ide.dhCont = DateTime.Now.ToString("s") + "-03:00";
            Genericos.gravarLinhaLog("65", "[dhCont: " + NFCeXMLContingencia.infNFe.ide.dhCont.ToString() + "]");

            Genericos.gravarLinhaLog("65", "[INSERINDO_xJust]");
            NFCeXMLContingencia.infNFe.ide.xJust = "CONTINGENCIA DEVIDO FALHA NA COMUNICACAO COM SEFAZ";
            Genericos.gravarLinhaLog("65", "[xJust: " + NFCeXMLContingencia.infNFe.ide.xJust.ToString() + "]");

            Genericos.gravarLinhaLog("65", "[AJUSTES_CONTINGENCIA_FINALIZADOS]");

            //salva o arquivo XML para envio posterior
            salvarArquivoXML(NFCe);

            //retorna o xml da NFCe alterado com as tags de contingencia
            return NFCeXMLContingencia;
        }

        public static void salvarArquivoXML(TNFCe NFCe)
        {
            //indica o caminho da pasta das NFCe's em contingencia
            string caminho = @".\Contingencia\";

            //verifica se o diretorio existe ou nao
            if (!Directory.Exists(caminho)) Directory.CreateDirectory(caminho);

            Genericos.gravarLinhaLog("65", "[GRAVANDO_ARQUIVO_XML_CONTINGENCIA]");
            //cria um arquivo com o numero da nota + indicador de contigencia
            string nomeArquivo = caminho + NFCe.infNFe.ide.nNF.ToString() + "contingencia.xml";
            FileStream stream = new FileStream(nomeArquivo, FileMode.CreateNew);

            using (StreamWriter writer = new StreamWriter(stream))
            {
                //escreve o xml da NFCe no arquivo criado
                writer.Write(Genericos.NFCeToXML(NFCe));

                Genericos.gravarLinhaLog("65", "[XML CONTINGENCIA SALVO EM: " + caminho + " PARA ENVIO POSTERIOR");
            }
        }
    }
}
