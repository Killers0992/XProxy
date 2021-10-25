using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace XProxy.ServerList.Cryptography
{
	public class ECDSA
	{
		public static AsymmetricCipherKeyPair GenerateKeys(int size = 384)
		{
			ECKeyPairGenerator eckeyPairGenerator = new ECKeyPairGenerator("ECDSA");
			SecureRandom random = new SecureRandom();
			KeyGenerationParameters parameters = new KeyGenerationParameters(random, size);
			eckeyPairGenerator.Init(parameters);
			return eckeyPairGenerator.GenerateKeyPair();
		}

		public static string Sign(string data, AsymmetricKeyParameter privKey)
		{
			return Convert.ToBase64String(ECDSA.SignBytes(data, privKey));
		}

		public static byte[] SignBytes(string data, AsymmetricKeyParameter privKey)
		{
			byte[] result;
			try
			{
				result = ECDSA.SignBytes(Encoding.UTF8.GetBytes(data), privKey);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		public static byte[] SignBytes(byte[] data, AsymmetricKeyParameter privKey)
		{
			byte[] result;
			try
			{
				ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
				signer.Init(true, privKey);
				signer.BlockUpdate(data, 0, data.Length);
				result = signer.GenerateSignature();
			}
			catch
			{
				result = null;
			}
			return result;
		}

		public static bool Verify(string data, string signature, AsymmetricKeyParameter pubKey)
		{
			return ECDSA.VerifyBytes(data, Convert.FromBase64String(signature), pubKey);
		}

		public static bool VerifyBytes(string data, byte[] signature, AsymmetricKeyParameter pubKey)
		{
			bool result;
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
				signer.Init(false, pubKey);
				signer.BlockUpdate(bytes, 0, data.Length);
				result = signer.VerifySignature(signature);
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		public static AsymmetricKeyParameter PublicKeyFromString(string key)
		{
			AsymmetricKeyParameter result;
			using (TextReader textReader = new StringReader(key))
			{
				result = (AsymmetricKeyParameter)new PemReader(textReader).ReadObject();
			}
			return result;
		}

		public static string KeyToString(AsymmetricKeyParameter key)
		{
			string result;
			using (TextWriter textWriter = new StringWriter())
			{
				PemWriter pemWriter = new PemWriter(textWriter);
				pemWriter.WriteObject(key);
				pemWriter.Writer.Flush();
				result = textWriter.ToString();
			}
			return result;
		}
	}
}
