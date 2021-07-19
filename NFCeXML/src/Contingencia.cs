using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NSNFCeXML.src.Classes.NFCe;
using NSNFCeXML.src.Genericos;
using LayoutXML;
using System.Timers;


namespace ContingenciaNFCe
{
    class Contingencia
    {

        // classe para controlar a contingencia
        public static void controleContingencia(TNFCe NFCeXML, string statusEnvio) //paralelo com emissaoSincrona
        {

            // verifica se em 3000ms a NFCe foi autorizada ou não
            bool autorizadoNormal = timerEmissaoNormal(statusEnvio);

            if (autorizadoNormal == false)
            {
                //caso nao tenha sido autorizada em 3000ms sera tratada como possivel contingencia, mas chance de autorizar normal
                autorizadoNormal = timerContingencia(statusEnvio); 
            }

            if (autorizadoNormal == false)
            {
                //caso nao tenha sido autorizada em 12000ms é aplicada a contingencia
                aplicarContingencia(NFCeXML);
            }
        }

        //alterar o xml inserindo tags de contingencia
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

            Genericos.gravarLinhaLog("65", "[GERANDO_chNFe]");
            NFCeXMLContingencia.infNFe.Id = Genericos.GerarChaveNFCe(NFCeXMLContingencia);
            Genericos.gravarLinhaLog("65", "[CHAVE_DE_ACESSO_GERADA: " + NFCeXMLContingencia.infNFe.Id.ToString() + "]");

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

        public static bool timerContingencia(string statusEnvio)
        {
            int i = 1;
            bool autorizado = false;

            while ( i < 9 ) {
                if (statusEnvio == "100")
                {
                    autorizado = true;
                    break;
                    
                }
                else 
                {
                    Thread.Sleep(1000);
                };
                ++i;
            }

            if (i==9 || autorizado == false)
            {
                Genericos.gravarLinhaLog("65", "[CONTINGENCIA ATIVADA as " + DateTime.Now.ToString("s") + "-03:00" + "]");
            }

            return autorizado;
        }

        public static bool timerEmissaoNormal(string statusEnvio)
        {
            int i = 1;
            bool autorizado = false;

            while (i < 3)
            {
                if (statusEnvio == "100")
                {
                    autorizado = true;
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                };
                ++i;
            }
            Genericos.gravarLinhaLog("65", "[TIMEOUT Emissao Normal: " + DateTime.Now.ToString("s") + "-03:00" + "]");
            return autorizado;
        }


        //public static void timerEmissaoNormal(EmitirSincronoRetNFCe retorno) 
        //{
        //    // defime timeout da emissão normal - timer para ativacao da contingencia
        //    System.Timers.Timer bTimer = new System.Timers.Timer(3000); //1500ms x2

        //    bTimer.Elapsed += OnTimedEventEmissaoNormal;
        //    bTimer.AutoReset = true;
        //    bTimer.Enabled = true;

        //    bTimer.Stop();
        //    bTimer.Dispose();
        //}

        //public static void timerContingencia(EmitirSincronoRetNFCe retorno) 
        //{
        //    //timer para executar a contingencia
        //    System.Timers.Timer aTimer = new System.Timers.Timer(9000); // 1500ms x6

        //    aTimer.Elapsed += OnTimedEventContingencia;
        //    aTimer.AutoReset = true;
        //    aTimer.Enabled = true;

        //    aTimer.Stop();
        //    aTimer.Dispose();

        //}

        //private static void OnTimedEventContingencia(Object source, ElapsedEventArgs e)
        //{
        //    Console.WriteLine("Contingencia Aplicada as " + DateTime.Now.ToString("s") + "-03:00");
        //}

        //private static void OnTimedEventEmissaoNormal(Object source, ElapsedEventArgs e)
        //{
        //    Genericos.gravarLinhaLog("65","[TIMEOUT Emissao Normal - Contingencia Ativada as " + DateTime.Now.ToString("s") + "-03:00");
        //}

    }
}
