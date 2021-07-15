using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using NSNFCeXML.src.Classes.NFCe;

namespace NSNFCeXML.src.Genericos
{
    public class Genericos
    {
        public static void salvarXML(string xml, string caminho, string nome, string tpEvento = "", string nSeqEvento = "")
        {
            string localParaSalvar = caminho + tpEvento + nome + nSeqEvento + ".xml";
            string ConteudoSalvar = "";
            ConteudoSalvar = xml.Replace(@"\""", "");
            File.WriteAllText(localParaSalvar, ConteudoSalvar);
        }

        public static void salvarJSON(string json, string caminho, string nome, string tpEvento = "", string nSeqEvento = "")
        {
            string localParaSalvar = caminho + tpEvento + nome + nSeqEvento + ".json";
            File.WriteAllText(localParaSalvar, json);
        }

        public static void salvarPDF(string pdf, string caminho, string nome, string tpEvento = "", string nSeqEvento = "")
        {
            string localParaSalvar = caminho + tpEvento + nome + nSeqEvento + ".pdf";
            byte[] bytes = Convert.FromBase64String(pdf);
            if (File.Exists(localParaSalvar))
                File.Delete(localParaSalvar);
            FileStream stream = new FileStream(localParaSalvar, FileMode.CreateNew);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        public static void gravarLinhaLog(string modelo, string conteudo)
        {
            string caminho = @".\log\";
            Console.Write(caminho);

            if (!Directory.Exists(caminho))
                Directory.CreateDirectory(caminho);

            string data = DateTime.Now.ToShortDateString();
            string hora = DateTime.Now.ToShortTimeString();
            string nomeArq = data.Replace("/", "") + "_" + modelo;

            using (StreamWriter outputFile = new StreamWriter(caminho + nomeArq + ".txt", true))
            {
                outputFile.WriteLine(data + " " + hora + " - " + conteudo);
            }
        }
        
        public static X509Certificate2 buscaCertificado(String cnpj)
        {
            X509Certificate2Collection lcerts;
            X509Store lStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            lStore.Open(OpenFlags.ReadOnly);

            lcerts = lStore.Certificates;
            X509Certificate2 cert = null;
            foreach (X509Certificate2 elemento in lcerts)
            {
                if (elemento.Subject.Contains(cnpj))
                {
                    cert = elemento;
                    lStore.Close();
                    return cert;
                }
            }
            lStore.Close();
            return cert;
        }

        public static string assinaXML(string XMLString, string RefUri, X509Certificate2 X509Cert)
        {
            XmlDocument XMLDoc;
            try
            {

                string _xnome = "";

                if (X509Cert != null)
                {
                    _xnome = X509Cert.Subject.ToString();
                }

                X509Certificate2 _X509Cert = new X509Certificate2();
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection collection1 = (X509Certificate2Collection)collection.Find(X509FindType.FindBySubjectDistinguishedName, _xnome, false);
                
                if (collection1.Count == 0)
                {
                    throw new Exception("Problemas no certificado digital");
                }
                else
                {
                    _X509Cert = collection1[0];
                    string x;
                    x = _X509Cert.GetKeyAlgorithm().ToString();
                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = false;

                    try
                    {
                        doc.LoadXml(XMLString);
                        int qtdeRefUri = doc.GetElementsByTagName(RefUri).Count;

                        if (qtdeRefUri == 0)
                        {
                            throw new Exception("A tag de assinatura " + RefUri.Trim() + " inexiste");
                        }
                        else
                        {
                            if (qtdeRefUri > 1)
                            {
                                throw new Exception("A tag de assinatura " + RefUri.Trim() + " não é unica");
                            }
                            else
                            {
                                try
                                {

                                    SignedXml signedXml = new SignedXml(doc);

                                    signedXml.SigningKey = _X509Cert.PrivateKey;

                                    Reference reference = new Reference();

                                    XmlAttributeCollection _Uri = doc.GetElementsByTagName(RefUri).Item(0).Attributes;
                                    foreach (XmlAttribute _atributo in _Uri)
                                    {
                                        if (_atributo.Name == "Id")
                                        {
                                            reference.Uri = "#" + _atributo.InnerText;
                                        }
                                    }

                                    XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                                    reference.AddTransform(env);

                                    XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
                                    reference.AddTransform(c14);

                                    signedXml.AddReference(reference);

                                    KeyInfo keyInfo = new KeyInfo();

                                    keyInfo.AddClause(new KeyInfoX509Data(_X509Cert));

                                    signedXml.KeyInfo = keyInfo;

                                    signedXml.ComputeSignature();

                                    XmlElement xmlDigitalSignature = signedXml.GetXml();

                                    doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
                                    XMLDoc = new XmlDocument();
                                    XMLDoc.PreserveWhitespace = false;
                                    XMLDoc = doc;
                                    return XMLDoc.OuterXml;
                                }
                                catch (Exception caught)
                                {
                                    throw new Exception("Erro: Ao assinar o documento - " + caught.Message);
                                }
                            }
                        }
                    }
                    catch (Exception caught)
                    {
                        throw new Exception("Erro: XML mal formado - " + caught.Message);
                    }
                }
            }
            catch (Exception caught)
            {
                throw new Exception("Erro: Problema ao acessar o certificado digital" + caught.Message);
            }
        }

        //transforma objeto xml em string xml
        public static string NFCeToXML(object NFCe)
        {
            using (var stringwriter = new StringWriter())
            {
                var serializer = new XmlSerializer(NFCe.GetType());
                serializer.Serialize(stringwriter, NFCe);
                return stringwriter.ToString();
            }
        }

        private static int GerarDigitoVerificador(string chave)
        {
            int soma = 0;
            int restoDivisao = -1;
            int digitoVerificador = -1;
            int pesoMultiplicacao = 2;

            for (int i = chave.Length - 1; i != -1; i--)
            {
                int ch = Convert.ToInt32(chave[i].ToString());
                soma += ch * pesoMultiplicacao;

                if (pesoMultiplicacao < 9)
                    pesoMultiplicacao += 1;
                else
                    pesoMultiplicacao = 2;
            }
            restoDivisao = soma % 11;
            if (restoDivisao == 0 || restoDivisao == 1)
                digitoVerificador = 0;
            else
                digitoVerificador = 11 - restoDivisao;

            return digitoVerificador;
        }

        public static string removeINT(string str)
        {
            var apenasDigitos = new Regex(@"[^\d]");
            return apenasDigitos.Replace(str, "");
        }


        public static int gerarCodigoCDF()
        {
            int min = 10000000;
            int max = 99999999;
            Random random = new Random();
            return random.Next(min, max);
        }

        public static string GerarChaveNFCe(TNFCe NFCe, string tpEvento = "", string nSeqEvento = "")
        {
            string projeto = removeINT(NFCe.infNFe.ide.mod.ToString());
            string cUF = removeINT(NFCe.infNFe.ide.cUF.ToString());
            string dhEmi = NFCe.infNFe.ide.dhEmi.ToString();
            string serie = NFCe.infNFe.ide.serie.ToString();
            string nDF = NFCe.infNFe.ide.nNF.ToString();
            string tpEmis = removeINT(NFCe.infNFe.ide.tpEmis.ToString());
            string cnpjEmitente = NFCe.infNFe.emit.Item.ToString();


            for (int i = serie.Length; i < 3; i++)
                serie = "0" + serie;

            for (int i = nDF.Length; i < 9; i++)
                serie = "0" + serie;

            string[] auxAAMM = dhEmi.Split('T');
            DateTime dateTime = DateTime.Parse(auxAAMM[0]);
            string aamm = dateTime.ToString("yyMM");
            string chave43 = cUF + aamm + cnpjEmitente + projeto + serie + nDF + tpEmis + gerarCodigoCDF();
            string chave = tpEvento + chave43 + GerarDigitoVerificador(chave43) + nSeqEvento;
            return chave;
        }

    }
}