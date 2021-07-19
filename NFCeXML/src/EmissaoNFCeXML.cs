using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NSNFCeXML.src.Classes.NFCe;
using NSNFCeXML.src.Genericos;
using LayoutXML;
using ContingenciaNFCe;
using NSSuite;
using System.Timers;
using Newtonsoft.Json;

namespace EmissaoNFCeXML
{
    class EmissaoNFCeXML
    {
        public static string emitirNFCe()
        {
            TNFCe NFCeXML = Layout.GerarLayoutNFCeXML();
            NFCeXML.infNFe.ide.nNF = "21101";
            string conteudoXML = Genericos.NFCeToXML(NFCeXML);

            string retorno = NSSuite.NSSuite.emitirNFCeSincrono(conteudoXML, "xml", "07364617000135", "2", @".\NFCe\", false, false);
            dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
            string statusEnvio = respostaJson.statusEnvio;

            Thread controleContingencia = new Thread(Contingencia.controleContingencia(NFCeXML, statusEnvio));
            controleContingencia.Start();

            return retorno;
        }

    }
}
