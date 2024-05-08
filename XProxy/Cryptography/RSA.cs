using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Unicode;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace XProxy.Cryptography
{
	public static class RSA
	{
		//Source: https://stackoverflow.com/questions/8830510/c-sharp-sign-data-with-rsa-using-bouncycastle

		public static bool Verify(string data, string signature, string key)
		{
			// Disposed un-disposed StringReader @ Dankrushen
			using (TextReader reader = new StringReader(key))
			{
				var pemReader = new PemReader(reader);
				var encKey = (AsymmetricKeyParameter)pemReader.ReadObject();
				var signer = SignerUtilities.GetSigner("SHA256withRSA");
				signer.Init(false, encKey);
				var expectedSig = Convert.FromBase64String(signature);
				byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(data.Length));
				int length = Utf8.GetBytes(data, buffer);
				signer.BlockUpdate(buffer, 0, length);
				ArrayPool<byte>.Shared.Return(buffer);
				return signer.VerifySignature(expectedSig);
			}
		}
	}
}
