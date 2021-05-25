using GrupoCoen.Corporativo.Libraries.ConexionBD;
using System;
using System.Collections.Generic;
using System.Text;

namespace WSREGPROXY.Encriptor
{
    internal class BFEngine
    {
        public enum AccionCifrado
        {
            Encrypt,
            Decrypt
        }
        private string _key;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oPlainText">Plaintext data to encrypt</param>
        /// <returns>Ciphertext with IV appended to front</returns>
        /// 
        public BFEngine()
        {

            _key = Getkey();
        }

        public BFEngine(string key)
        {

            _key = key;
        }

        /// <summary>
        /// Encrypts / Decrypts
        /// </summary>
        ~BFEngine()
        { }

        public string Process(string oPlainText, AccionCifrado oAccion)
        {
            string oResultado = "";
            try
            {
                NewBlowFish oBF = new NewBlowFish(_key, true);

                if (oAccion == AccionCifrado.Encrypt)
                {
                    oResultado = oBF.Encrypt_CBC(oPlainText);
                }

                if (oAccion == AccionCifrado.Decrypt)
                {
                    oResultado = oBF.Decrypt_CBC(oPlainText);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return oResultado;
        }


        protected internal string Getkey()
        {
            CipherEngine oCE = new CipherEngine(CipherEngine.AlgorithmType.TripleDES);
            string k1 = oCE.Encrypt("Microsoft");
            string k2 = oCE.Encrypt("Visual");
            string k3 = oCE.Encrypt("Studio");
            return (k3 + k1 + k2).Substring(3, 12);
        }
    }
}
