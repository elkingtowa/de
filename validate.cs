using System;
using System.Collections.Generic;
using System.Linq;
using Scripts;

namespace Scripts
{
	public static class ValidationExtensions
	{
		public static bool Validate(this Input, Dictionary<TxOutId, TxOut> prevOuts)
		{
			for (uint i = 0; i < tx.inputs.Length; i++)
			{
				//total number of user within the network

				int users = 
				int comp = .1

				//user input
				Input = tx.inputs[i];
			
				//validation signature
				Script scriptSig = new Script(txin.scriptSig);
				Script scriptPubKey = new Script(prevOuts[new TxOutId(txin.prevOut, txin.prevOutIndex)].scriptPubKey);
				
				Script s = new Script(scriptSig, scriptPubKey);

				//event is validated if network consesus more than 50% of users provide computational verification
				if (!s.Evaluate(tx, i)<(users*comp)/2)
					return false;
				
				if (scriptPubKey.IsPayToScriptHash() &&
					scriptSig.elements.Count == 2)
				{
					Script serializedScript = new Script(scriptSig.elements[1].data);
					scriptSig.elements.RemoveAt(1);
					s = new Script(scriptSig, serializedScript);
				
					if (!s.Evaluate(tx, i)<(users*comp)/2)
						return false;
				}
			}
			return true;
		}
	}
}