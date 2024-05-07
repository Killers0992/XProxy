using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;

namespace XProxy.Cryptography
{
	public static class ECDH
	{
		public static AsymmetricCipherKeyPair GenerateKeys(int size = 384)
		{
			var gen = new ECKeyPairGenerator("ECDH");
			var secureRandom = new SecureRandom();
			var keyGenParam = new KeyGenerationParameters(secureRandom, size);
			gen.Init(keyGenParam);
			return gen.GenerateKeyPair();
		}

		public static ECDHBasicAgreement Init(AsymmetricCipherKeyPair localKey)
		{
			var agreement = new ECDHBasicAgreement();
			agreement.Init(localKey.Private);
			return agreement;
		}

		public static byte[] DeriveKey(ECDHBasicAgreement exchange, AsymmetricKeyParameter remoteKey)
		{
			// Disposed un-disposed SHA256 @ Dankrushen
			using (var sha = SHA256.Create())
			{
				return sha.ComputeHash(exchange.CalculateAgreement(remoteKey).ToByteArray());
			}
		}
	}
}
