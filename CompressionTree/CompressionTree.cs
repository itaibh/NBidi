/*
 * ${res:XML.StandardHeader.CreatedBySharpDevelop}
 * ${res:XML.StandardHeader.User} ${USER}
 * ${res:XML.StandardHeader.Date} ${DATE}
 * ${res:XML.StandardHeader.Time} ${TIME}
 * 
 * ${res:XML.StandardHeader.HowToChangeTemplateInformation}
 */
using System;
using System.IO;
using System.Globalization;
using NBidi;

namespace CompressionTree
{
	public interface ICompTree<T> : IEquatable<ICompTree<T>>
	{
		T Data { get; set; }
		ICompTree<T> Left { set; get; }
		ICompTree<T> Right { set; get; }
		int NodeCount { get; }
		int UniqueNodeCount { get; }
		void ResetCount();
		int LeafCount { get; }
		int UniqueLeafCount { get; }
		void Compress();
	}
	
	public class CompTree<T> : ICompTree<T>
	{
		private T data;
		private ICompTree<T> left, right;
		private bool counted;
		
		public T Data {
			get { return data; }
			set { data = value; }
		}
		
		public ICompTree<T> Left {
			get { return left; }
			set { left = value; }
		}
		
		public ICompTree<T> Right {
			get { return right; }
			set { right = value; }
		}
		
		public int NodeCount
		{
			get
			{
				return 1 + 
					((left == null) ? 0 : left.NodeCount) +
					((right == null) ? 0 : right.NodeCount);
			}
		}

		public int UniqueNodeCount
		{
			get
			{
				if (counted) return 0;
				counted = true;
				return 1 +
					((left == null) ? 0 : left.UniqueNodeCount) +
					((right == null) ? 0 : right.UniqueNodeCount);
			}
		}
		
		public int LeafCount
		{
			get
			{
				return ((left == null && right == null) ? 1 : 0) +
					((left == null) ? 0 : left.LeafCount) +
					((right == null) ? 0 : right.LeafCount);
			}
		}
		
		public int UniqueLeafCount
		{
			get
			{
				if (counted) return 0;
				counted = true;
				return ((left == null && right == null) ? 1 : 0) +
					((left == null) ? 0 : left.UniqueLeafCount) +
					((right == null) ? 0 : right.UniqueLeafCount);
			}
		}
		
		
		public void ResetCount()
		{
			counted = false;
			if (left != null) left.ResetCount();
			if (right != null) right.ResetCount();
		}
		
		public void Compress()
		{
			if (left != null)
				left.Compress();
			
			if (right != null)
				right.Compress();
			
			if (left != null && right != null && left.Equals(right))
			{
				left = right;
			}
		}
		
		public bool Equals(ICompTree<T> other)
		{
			if (object.ReferenceEquals(other, this)) return true;
			if (other  == null) return false;
			bool res = true;
			if (left != null)
			{
				res = res && left.Equals(other.Left);
			}

			if (right != null)
			{
				res = res && right.Equals(other.Right);
			}

			if (data != null)
			{
				res = res && data.Equals(other.Data);
			}
			return res;
		}
	}
	
	public class Program
	{
		public static void Main(string[] args)
		{
			ICompTree<BidiCharacterType> root = new CompTree<BidiCharacterType>();
			ICompTree<BidiCharacterType> node;

			using (StreamReader sr = File.OpenText("UnicodeData.txt"))
			{
				while (sr.Peek() >= 0)
				{
					string line = sr.ReadLine();
					int comment = line.IndexOf('#');
					if (comment >= 0)
						line = line.Substring(0, comment - 1);
					if (line == null || line == string.Empty) continue;
					string[] fields = line.Split(';');
					int charNum = int.Parse(fields[0], NumberStyles.HexNumber);
					if (charNum > 0xffff) continue;
					BidiCharacterType bct = (BidiCharacterType)(Enum.Parse(typeof(BidiCharacterType), fields[4]));

					node = root;
					for (int bit = 0; bit < 16; ++ bit)
					{
						if ((charNum & (1 << bit)) == 0)
						{
							if (node.Left == null)
								node.Left = new CompTree<BidiCharacterType>();
							
							node = node.Left;
						}
						else
						{
							if (node.Right == null)
								node.Right = new CompTree<BidiCharacterType>();
							
							node = node.Right;
						}
					}
					node.Data = bct;
				}
			}
			
			Console.WriteLine("Nodes in tree before compression: {0}", root.NodeCount);
			root.ResetCount();
			Console.WriteLine("Unique nodes in tree before compression: {0}", root.UniqueNodeCount);
			Console.WriteLine("Leafs in tree before compression: {0}", root.LeafCount);
			root.ResetCount();
			Console.WriteLine("Unique leafs in tree before compression: {0}", root.UniqueLeafCount);
			Console.WriteLine("Compressing...");
			root.Compress();
			Console.WriteLine("Nodes in tree after compression: {0}", root.NodeCount);
			root.ResetCount();
			Console.WriteLine("Unique nodes in tree after compression: {0}", root.UniqueNodeCount);
			Console.WriteLine("Leafs in tree after compression: {0}", root.LeafCount);
			root.ResetCount();
			Console.WriteLine("Unique leafs in tree after compression: {0}", root.UniqueLeafCount);
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}