﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.Crypto;

namespace Bitcoin_Tool.Scripts
{
	public static class SigningExtensions
	{
		public static Transaction CopyForSigning(this Transaction tx, UInt32 txInIndex, Script subScript, HashType hashType)
		{
			Transaction txCopy = new Transaction(tx.ToBytes());

			for (int i = 0; i < txCopy.inputs.Length; i++)
			{
				if (i == txInIndex)
					txCopy.inputs[txInIndex].scriptSig = subScript.ToBytes();
				else
					txCopy.inputs[i].scriptSig = new Byte[0];
			}

			if (((Byte)hashType & 0x1F) == (Byte)HashType.SIGHASH_NONE)
			{
				txCopy.outputs = new TxOut[0];

				for (int i = 0; i < txCopy.inputs.Length; i++)
					if (i != txInIndex)
						txCopy.inputs[i].sequenceNo = 0;
			}
			else if (((Byte)hashType & 0x1F) == (Byte)HashType.SIGHASH_SINGLE)
			{
				TxOut txOut = txCopy.outputs[txInIndex];
				txCopy.outputs = new TxOut[txInIndex + 1];

				for (int i = 0; i < txCopy.outputs.Length; i++)
				{
					if (i == txInIndex)
						txCopy.outputs[i] = txOut;
					else
					{
						txCopy.outputs[i].value = UInt64.MaxValue;
						txCopy.outputs[i].scriptPubKey = new Byte[0];
					}
				}

				for (int i = 0; i < txCopy.inputs.Length; i++)
					if (i != txInIndex)
						txCopy.inputs[i].sequenceNo = 0;
			}

			if (((Byte)hashType & (Byte)HashType.SIGHASH_ANYONECANPAY) == (Byte)HashType.SIGHASH_ANYONECANPAY)
			{
				txCopy.inputs = new TxIn[] { txCopy.inputs[txInIndex] };
			}

			return txCopy;
		}

		public static void Sign(this TxIn txin, Transaction tx, TxOut prevOut, PrivateKey key, HashType hashType = HashType.SIGHASH_ALL, Script redeemScript = null)
		{
			SHA256 sha256 = new SHA256Managed();
			UInt32 txInIndex;
			for (txInIndex = 0; txInIndex < tx.inputs.Length; txInIndex++)
			{
				if (TxIn.ReferenceEquals(txin, tx.inputs[txInIndex]))
					break;
			}
			if (txInIndex == tx.inputs.Length)
				throw new ArgumentException("Input not part of transaction.");

			// Only know how to sign if output does not contain OP_CODESEPERATOR for now
			Script subScript = new Script(prevOut.scriptPubKey);

			//// Still don't fully handle codeseperator, but we remove them all as in the spec.
			//// Might be all we need to do, scriptSig will never contain OP_CODESEPERATOR since we are creating it.
			////subScript.elements.RemoveAll(x => x.opCode == OpCode.OP_CODESEPARATOR);
			// Actually, those would be nonstandard anyway

			Transaction txCopy = tx.CopyForSigning(txInIndex, subScript, hashType);

			Byte[] txHash = txCopy.ToBytes().Concat(new Byte[] { (Byte)hashType, 0x00, 0x00, 0x00 }).ToArray();
			txHash = sha256.ComputeHash(sha256.ComputeHash(txHash));

			Script s = new Script();

			if (subScript.IsPayToScriptHash())
			{
				if (redeemScript == null)
					throw new ArgumentNullException("P2SH transaction requires serialied script");

				s.elements.Add(new ScriptElement(redeemScript.ToBytes()));
				subScript = redeemScript;
			}

			if (subScript.IsPayToPubKeyHash())
			{
				Byte[] sig = key.Sign(txHash).Concat(new Byte[] { (Byte)hashType }).ToArray();
				s.elements.Insert(0, new ScriptElement(sig));
				s.elements.Insert(1, new ScriptElement(key.pubKey.ToBytes()));
			}
			else if (subScript.IsPayToPublicKey())
			{
				Byte[] sig = key.Sign(txHash).Concat(new Byte[] { (Byte)hashType }).ToArray();
				s.elements.Insert(0, new ScriptElement(sig));
			}
			else if (subScript.IsPayToMultiSig())
			{
				Script scriptSig = new Script(txin.scriptSig);

				if (scriptSig.elements.Count == 0)
					scriptSig.elements.Add(new ScriptElement(OpCode.OP_0));

				Byte[] sig = key.Sign(txHash).Concat(new Byte[] { (Byte)hashType }).ToArray();
				s.elements.Insert(0, new ScriptElement(sig));
			}
			else
			{
				throw new ArgumentException("Unrecognized TxOut Script");
			}

			txin.scriptSig = s.ToBytes();
		}
	}
}
