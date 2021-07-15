using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSNFCeXML.src.Classes.NFCe;
using NSNFCeXML.src.Genericos;
using LayoutXML;
using ContingenciaNFCe;
using NSSuite;
using System.Timers;
using Newtonsoft.Json;

namespace EmissaoNFCeXML 
{
    class Program
    {
        static void Main(string[] args)
        {
            TNFCe NFCeXML = Layout.GerarLayoutNFCeXML();
            NFCeXML.infNFe.ide.nNF = "21102";

            string conteudoXML = Genericos.NFCeToXML(NFCeXML);

            string retorno = NSSuite.NSSuite.emitirNFCeSincrono(conteudoXML, "xml", "07364617000135", "2", @".\NFCe\", false, false);

            //Contingencia.timerContingencia(JsonConvert.DeserializeObject<EmitirSincronoRetNFCe>(retorno));
            Console.WriteLine(retorno);
        }
    }
}
