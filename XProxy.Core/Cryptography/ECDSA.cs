using System;
using System.Buffers;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace XProxy.Cryptography
{
	public static class ECDSA
	{
		/// <summary>
		/// Generates a ECDSA keys pair
		/// </summary>
		/// <param name="size">Length of the key</param>
		/// <returns>Keys pair</returns>
		public static AsymmetricCipherKeyPair GenerateKeys(int size = 384)
		{
			var gen = new ECKeyPairGenerator("ECDSA");
			var secureRandom = new SecureRandom();
			var keyGenParam = new KeyGenerationParameters(secureRandom, size);
			gen.Init(keyGenParam);
			return gen.GenerateKeyPair();
		}

		/// <summary>
		/// Signs data using ECDSA algorithm
		/// </summary>
		/// <param name="data">Data to sign</param>
		/// <param name="privKey">Private key</param>
		/// <returns>Signature</returns>
		public static string Sign(string data, AsymmetricKeyParameter privKey)
		{
			return Convert.ToBase64String(SignBytes(data, privKey));
		}

		/// <summary>
		/// Signs data using ECDSA algorithm
		/// </summary>
		/// <param name="data">Data to sign</param>
		/// <param name="privKey">Private key</param>
		/// <returns>Signature</returns>
		public static byte[] SignBytes(string data, AsymmetricKeyParameter privKey)
		{
			try
			{
				byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(data.Length));
				int length = Utf8.GetBytes(data, buffer);
				byte[] result = SignBytes(buffer, 0, length, privKey);
				ArrayPool<byte>.Shared.Return(buffer);
				return result;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Signs data using ECDSA algorithm
		/// </summary>
		/// <param name="data">Data to sign</param>
		/// <param name="privKey">Private key</param>
		/// <returns>Signature</returns>
		public static byte[] SignBytes(byte[] data, AsymmetricKeyParameter privKey)
		{
			return SignBytes(data, 0, data.Length, privKey);
		}

		/// <summary>
		/// Signs data using ECDSA algorithm
		/// </summary>
		/// <param name="data">Data to sign</param>
		/// <param name="offset">Offset of the data</param>
		/// <param name="count">Amount of bytes to sign</param>
		/// <param name="privKey">Private key</param>
		/// <returns>Signature</returns>
		public static byte[] SignBytes(byte[] data, int offset, int count, AsymmetricKeyParameter privKey)
		{
			try
			{
				var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
				signer.Init(true, privKey);
				signer.BlockUpdate(data, offset, count);
				var sigBytes = signer.GenerateSignature();
				return sigBytes;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Verifies an ECDSA digital signature
		/// </summary>
		/// <param name="data">Signed data</param>
		/// <param name="signature">ECDSA signature</param>
		/// <param name="pubKey">Public key used to sign the data</param>
		/// <returns>Whether the signature is valid or not</returns>
		public static bool Verify(string data, string signature, AsymmetricKeyParameter pubKey)
		{
			return VerifyBytes(data, Convert.FromBase64String(signature), pubKey);
		}

		/// <summary>
		/// Verifies an ECDSA digital signature
		/// </summary>
		/// <param name="data">Signed data</param>
		/// <param name="signature">ECDSA signature</param>
		/// <param name="pubKey">Public key used to sign the data</param>
		/// <returns>Whether the signature is valid or not</returns>
		public static bool VerifyBytes(string data, byte[] signature, AsymmetricKeyParameter pubKey)
		{
			try
			{
				var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
				signer.Init(false, pubKey);
				byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(data.Length));
				int length = Utf8.GetBytes(data, buffer);
				signer.BlockUpdate(buffer, 0, length);
				ArrayPool<byte>.Shared.Return(buffer);
				return signer.VerifySignature(signature);
			}
			catch (Exception e)
			{
				Console.WriteLine("ECDSA Verification Error (BouncyCastle): " + e.Message + ", " + e.StackTrace);
				return false;
			}
		}

		/// <summary>
		/// Loads a public key from PEM
		/// </summary>
		/// <param name="key">PEM to process</param>
		/// <returns>Loaded key</returns>
		public static AsymmetricKeyParameter PublicKeyFromString(string key)
		{
			// Disposed un-disposed StringReader @ Dankrushen
			using (TextReader reader = new StringReader(key))
			{
				return (AsymmetricKeyParameter)new PemReader(reader).ReadObject();
			}
		}

		/// <summary>
		/// Loads a private key from PEM
		/// </summary>
		/// <param name="key">PEM to process</param>
		/// <returns>Loaded key</returns>
		public static AsymmetricKeyParameter PrivateKeyFromString(string key)
		{
			// Disposed un-disposed StringReader @ Dankrushen
			using (TextReader reader = new StringReader(key))
			{
				return ((AsymmetricCipherKeyPair)new PemReader(reader).ReadObject()).Private;
			}
		}

		/// <summary>
		/// Saves an asymmetric key in a PEM format.
		/// </summary>
		/// <param name="key">Key to save</param>
		/// <returns>PEM-encoded key</returns>
		public static string KeyToString(AsymmetricKeyParameter key)
		{
			// Disposed un-disposed StringWriter @ Dankrushen
			using (TextWriter textWriter = new StringWriter())
			{
				var pemWriter = new PemWriter(textWriter);
				pemWriter.WriteObject(key);
				pemWriter.Writer.Flush();
				return textWriter.ToString();
			}
		}
	}
}
