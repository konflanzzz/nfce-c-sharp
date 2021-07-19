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
using EmissaoNFCeXML;

namespace Program 
{
    class Program
    {
        static void Main(string[] args)
        {
            string retorno = EmissaoNFCeXML.EmissaoNFCeXML.emitirNFCe();
        }
    }
}
