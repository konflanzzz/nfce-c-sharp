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

namespace EmissaoNFCeXML 
{
    class Program
    {
        static void Main(string[] args)
        {
            TNFCe NFCeXML = Layout.GerarLayoutNFCeXML();
            NFCeXML.infNFe.ide.nNF = "21101";

            bool cont = false;

            if (cont == true)
            {
                NFCeXML = Contingencia.aplicarContingencia(NFCeXML);
            }

            string conteudoXML = Genericos.NFCeToXML(NFCeXML);
            string retorno = NSSuite.NSSuite.emitirNFCeSincrono(conteudoXML, "xml", "07364617000135", "2", @".\NFCe\",false,false);

            Console.WriteLine(retorno);

        }
    }
}
