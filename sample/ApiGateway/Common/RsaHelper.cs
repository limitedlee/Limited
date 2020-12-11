using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Limited.Gateway.Security
{
    class RsaHelper
    {
        /*
         *
         *    私钥 openssl genrsa -out rsa_1024_priv.pem 1024
         *    公钥 openssl rsa -pubout -in rsa_1024_priv.pem -out rsa_1024_pub.pem
         *    RSA 算法 操作规则
         *    1.为保证跨语言跨平台通用  RSA密钥格式 必须为 PCKS1 
         *    2.加密或签名之后的base64 中可能包含空格或加号,如果作为http请求参数 请url encode
         *    3.加密解密和签名验签 一定 不要使用同一组密钥对!!!
         *    4.为保证传输数据安全   通讯的双方 请各自生成一组密钥对  私钥各自保留 彼此公布 公钥
         *    5.加密之前 请一定先签名!   
         *      
         *      示例 
         *      A 为请求方   B为接收方 
         *     
         *      以下是 请求方 的操作
         *      比如 要传送的内容为  abc
         *      1.组装报文 如  RP_abc         原因:固定的消息头 方便对方知道解密成功
         *      2.用A的私钥 对 RP_abc 签名  假设签名结果为 C1K9D5   
         *      3.把签名结果组装在原报文末尾 如:RP_abc_C1K9D5
         *      4.用B的公钥 对 RP_abc_C1K9D5 加密    结果假设为:   NG3NGB3T90N=
         *      5.把 NG3NGB3T90N=    发送给B
         *      
         *      以下是 接收方 的操作
         *      1.收到密文 NG3NGB3T90N=
         *      2.用B的私钥解密   
         *      3.判断报文开头是否是 固定消息头 RP   以检验A是否是用B的公钥加密.
         *      4.解密成功后  截取末尾签名  C1K9D5
         *      5.用A的公钥 对 消息体验签   待验签的消息体 RP_abc   签名值  C1K9D5
         *      6.若成功验签,即可表明 该消息一定是A发送的.
         */

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="message">待加密消息体</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string Encrypt(string message, string publicKey)
        {
            RSA rsa = CreateRsaFromPublicKey(publicKey);

            Byte[] PlaintextData = Encoding.UTF8.GetBytes(message);
            int MaxBlockSize = rsa.KeySize / 8 - 11; //加密块最大长度限制

            if (PlaintextData.Length <= MaxBlockSize)
            {
                return Convert.ToBase64String(rsa.Encrypt(PlaintextData, RSAEncryptionPadding.Pkcs1));
            }
            else
            {
                using (MemoryStream PlaiStream = new MemoryStream(PlaintextData))
                using (MemoryStream CrypStream = new MemoryStream())
                {
                    Byte[] Buffer = new Byte[MaxBlockSize];
                    int BlockSize = PlaiStream.Read(Buffer, 0, MaxBlockSize);

                    while (BlockSize > 0)
                    {
                        Byte[] ToEncrypt = new Byte[BlockSize];
                        Array.Copy(Buffer, 0, ToEncrypt, 0, BlockSize);

                        Byte[] Cryptograph = rsa.Encrypt(ToEncrypt, RSAEncryptionPadding.Pkcs1);
                        CrypStream.Write(Cryptograph, 0, Cryptograph.Length);

                        BlockSize = PlaiStream.Read(Buffer, 0, MaxBlockSize);
                    }

                    return Convert.ToBase64String(CrypStream.ToArray(), Base64FormattingOptions.None);
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cipher">密文</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string Decrypt(string cipher, string privateKey)
        {
            var rsa = CreateRsaFromPrivateKey(privateKey);

            Byte[] CiphertextData = Convert.FromBase64String(cipher);
            int MaxBlockSize = rsa.KeySize / 8; //解密块最大长度限制

            if (CiphertextData.Length <= MaxBlockSize)
            {
                var buffer = rsa.Decrypt(CiphertextData, RSAEncryptionPadding.Pkcs1);
                return System.Text.Encoding.UTF8.GetString(buffer);
            }

            using (MemoryStream CrypStream = new MemoryStream(CiphertextData))
            using (MemoryStream PlaiStream = new MemoryStream())
            {
                Byte[] Buffer = new Byte[MaxBlockSize];
                int BlockSize = CrypStream.Read(Buffer, 0, MaxBlockSize);

                while (BlockSize > 0)
                {
                    Byte[] ToDecrypt = new Byte[BlockSize];
                    Array.Copy(Buffer, 0, ToDecrypt, 0, BlockSize);

                    Byte[] Plaintext = rsa.Decrypt(ToDecrypt, RSAEncryptionPadding.Pkcs1);
                    PlaiStream.Write(Plaintext, 0, Plaintext.Length);

                    BlockSize = CrypStream.Read(Buffer, 0, MaxBlockSize);
                }

                return System.Text.Encoding.UTF8.GetString(PlaiStream.ToArray());
            }
        }


        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Sign(string data, string privateKey)
        {
            var rsa = CreateRsaFromPrivateKey(privateKey);
            var signBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = rsa.SignData(signBytes, HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
            var signature = Convert.ToBase64String(signatureBytes);
            return signature;
        }

        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="data">待签名的报文</param>
        /// <param name="signature">签名值</param>
        /// <returns></returns>
        public static bool Verify(string data, string signature, string publicKey)
        {
            var rsa = CreateRsaFromPublicKey(publicKey);
            var result = rsa.VerifyData(
                Encoding.UTF8.GetBytes(data),
                Convert.FromBase64String(signature),
                HashAlgorithmName.MD5,
                RSASignaturePadding.Pkcs1);
            return result;
        }


        private static RSA CreateRsaFromPrivateKey(string privateKey)
        {
            if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));
            privateKey = privateKey.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "").Trim().Replace("\r", "").Replace("\n", "");

            var privateKeyBits = System.Convert.FromBase64String(privateKey);
            var rsa = RSA.Create();
            var RSAparams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(RSAparams);
            return rsa;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }

            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static RSA CreateRsaFromPublicKey(string publicKey)
        {
            if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));
            publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "").Trim().Replace("\r", "").Replace("\n", "");

            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key;
            byte[] seq = new byte[15];
            int x509size;

            x509key = Convert.FromBase64String(publicKey);
            x509size = x509key.Length;

            using (var mem = new MemoryStream(x509key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return null;

                    seq = binr.ReadBytes(15);
                    if (!CompareBytearrays(seq, SeqOID))
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                        binr.ReadByte();
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                        lowbyte = binr.ReadByte();
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;

                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }

                    byte[] modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02)
                        return null;
                    int expbytes = (int)binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);

                    var rsa = RSA.Create();
                    var rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);
                    return rsa;
                }
            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }

            return true;
        }
    }
}
