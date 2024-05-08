using System.IO;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace XProxy.Cryptography
{
	public static class AES
	{
		public const int NonceSizeBytes = 32;
		public const int MacSizeBits = 128;

		public static byte[] AesGcmEncrypt(byte[] data, byte[] secret, SecureRandom secureRandom)
		{
			var nonce = new byte[NonceSizeBytes];
			secureRandom.NextBytes(nonce, 0, nonce.Length);

			var cipher = new GcmBlockCipher(new AesEngine());
			cipher.Init(true, new AeadParameters(new KeyParameter(secret), MacSizeBits, nonce));

			var cipherText = new byte[cipher.GetOutputSize(data.Length)];
			var len = cipher.ProcessBytes(data, 0, data.Length, cipherText, 0);
			cipher.DoFinal(cipherText, len);

			using (var combinedStream = new MemoryStream())
			{
				using (var binaryWriter = new BinaryWriter(combinedStream))
				{
					binaryWriter.Write(nonce);
					binaryWriter.Write(cipherText);
				}

				return combinedStream.ToArray();
			}
		}

		public static byte[] AesGcmDecrypt(byte[] data, byte[] secret)
		{
			using (var cipherStream = new MemoryStream(data))
				using (var cipherReader = new BinaryReader(cipherStream))
				{
					var nonce = cipherReader.ReadBytes(NonceSizeBytes);

					var cipher = new GcmBlockCipher(new AesEngine());
					cipher.Init(false, new AeadParameters(new KeyParameter(secret), MacSizeBits, nonce));

					var cipherText = cipherReader.ReadBytes(data.Length - nonce.Length);
					var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

					var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
					cipher.DoFinal(plainText, len);

					return plainText;
				}
		}
	}
}
