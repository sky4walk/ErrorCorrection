// André Betz
// http://www.andrebetz.de
using System;
using System.Collections;
using System.IO;

namespace ErrorCorrection
{
	/// <summary>
	/// generate Error correction codes
	/// and test a file and correct it
	/// </summary>
	public class ECC
	{
		/// <summary>
		/// CodewortSize
		/// </summary>
		private byte m_CodeSize = 8;
		private int m_RedundanceSize = 0;
		private int m_grtsHammingDistance = 0;
		private ArrayList m_CodeWords = null;
		private char[] m_ByteBuf = null;
		private int m_BitPos = 0;
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="CodeSize"></param>
		/// <param name="RedundanceSize"></param>
		public ECC(int RedundanceSize)
		{
			m_ByteBuf = new char[m_CodeSize];
			m_RedundanceSize = RedundanceSize;
			m_CodeWords = GenerateHammingMatrix(m_CodeSize,m_RedundanceSize);
			m_grtsHammingDistance = FindGreatesHammingCWDistance();
		}
		/// <summary>
		/// Bitstream reader
		/// </summary>
		/// <param name="rdStrm">Stream</param>
		/// <returns>-1: not success else succes bit value</returns>
		private int ReadBits(FileStream rdStrm)
		{
			if(m_BitPos==0)
			{
				try
				{
					int rdByte = rdStrm.ReadByte();
					if(rdByte<0)
					{
						return -1;
					}
					else
					{
						m_ByteBuf = Byte2Bits(rdByte,m_CodeSize);
					}
				}
				catch
				{
					return -1;
				}
			}

			int Bit = m_ByteBuf[m_BitPos];
			m_BitPos++;
			if(m_BitPos==m_CodeSize)
				m_BitPos = 0;
			return Bit;
		}
		/// <summary>
		/// return sthe lenght of HammingCodewords added size
		/// </summary>
		/// <returns>lenght of HammingCodewords</returns>
		private int GetHammingAddSize()
		{
			int len = -1;
			if(m_CodeWords.Count>0)
			{
				HammingCodeWort hcw = (HammingCodeWort)m_CodeWords[0];
				len = hcw.Redundanz.Length;
			}
			return len;
		}
		/// <summary>
		/// Reads a HammingCodeWort from file
		/// </summary>
		/// <param name="rdStrm">Filestream read</param>
		/// <param name="hcw">reference to Codeword</param>
		/// <returns>success</returns>
		private int ReadHamminCodeWord(FileStream rdStrm,ref HammingCodeWort hcw)
		{
			int LenRead = 0;
			bool EndRd = false;
			hcw = new HammingCodeWort();
			hcw.CodeWort = Byte2Bits(0,m_CodeSize);
			for(int i=0;i<hcw.CodeWort.Length && !EndRd;i++)
			{
				int rdSign = ReadBits(rdStrm);
				if(rdSign<0)
				{
					EndRd = true;
					break;
				}
				else
				{
					hcw.CodeWort[i] = (char)rdSign;
					LenRead++;
				}
			}
			hcw.Redundanz = Byte2Bits(0,GetHammingAddSize());
			for(int i=0;i<hcw.Redundanz.Length && !EndRd;i++)
			{
				int rdSign = ReadBits(rdStrm);
				if(rdSign<0)
				{
					EndRd = true;
					break;
				}
				else
				{
					hcw.Redundanz[i] = (char)rdSign;
					LenRead++;
				}
			}
			return LenRead;
		}
		/// <summary>
		/// Decodes an ECC File
		/// </summary>
		/// <param name="EccFileName">InputFile</param>
		/// <returns>success</returns>
		public bool Decode(string EccFileName)
		{
			string DatafileName = Path.GetFileNameWithoutExtension(EccFileName);
			if(File.Exists(DatafileName))
			{
				File.Delete(DatafileName);
			}
			FileStream ReadDatei = null;
			FileStream WriteDatei = null;
			try
			{
				ReadDatei = new FileStream(EccFileName,FileMode.Open,FileAccess.Read);
			}
			catch
			{
				return false;
			}
			try
			{
				WriteDatei = new FileStream(DatafileName,FileMode.Create,FileAccess.Write);
			}
			catch
			{
				return false;
			}

			HammingCodeWort hcw = new HammingCodeWort();

			while(ReadHamminCodeWord(ReadDatei,ref hcw)>0)
			{
				int pos = FindNearestCodeWordPosition(hcw);
				if(pos>=0 && pos<m_CodeWords.Count)
				{
					HammingCodeWort hcw2 = (HammingCodeWort)m_CodeWords[pos];
					byte wrSgn = (byte)Bits2Byte(hcw2.CodeWort);
					WriteDatei.WriteByte(wrSgn);
				}
			}

			try
			{
				ReadDatei.Close();
				WriteDatei.Flush();
				WriteDatei.Close();
			}
			catch(Exception)
			{
			}
			return true;
		}
		/// <summary>
		/// generates ECC File
		/// </summary>
		/// <param name="FileName">InputFile</param>
		/// <returns>success</returns>
		public bool Encode(string FileName)
		{
			string EccDatafileName = FileName + ".ecc";
			if(File.Exists(EccDatafileName))
			{
				File.Delete(EccDatafileName);
			}

			FileStream ReadDatei = null;
			FileStream WriteDatei = null;


			try
			{
				ReadDatei = new FileStream(FileName,FileMode.Open,FileAccess.Read);
			}
			catch
			{
				return false;
			}
			try
			{
				WriteDatei = new FileStream(EccDatafileName,FileMode.Create,FileAccess.Write);
			}
			catch
			{
				return false;
			}

			long len = ReadDatei.Length;
			long Pos = 0;
			int rdbyte = 0;

			while(rdbyte>-1&&Pos<len)
			{
				try
				{
					rdbyte = ReadDatei.ReadByte();
					HammingCodeWort hcw = (HammingCodeWort)m_CodeWords[rdbyte];
					WriteHammingCode(WriteDatei,hcw);
					Pos++;
				}
				catch
				{
					return false;
				}
			}

			try
			{
				FlushBitStream(WriteDatei);
				ReadDatei.Close();
				WriteDatei.Flush();
				WriteDatei.Close();
			}
			catch(Exception)
			{
			}
			return true;
		}
		
		/// <summary>
		/// write Hemmingcoded word to file
		/// </summary>
		/// <param name="wrStream">FileStream for writing</param>
		/// <param name="hcw">HammingCodeWort</param>
		/// <returns>success</returns>
		private bool WriteHammingCode(FileStream wrStream,HammingCodeWort hcw)
		{
			for(int i=0;i<hcw.CodeWort.Length;i++)
			{
				WriteBits(wrStream,hcw.CodeWort[i]);
			}
			for(int i=0;i<hcw.Redundanz.Length;i++)
			{
				WriteBits(wrStream,hcw.Redundanz[i]);
			}
			return true;
		}
		/// <summary>
		/// writes rest of bitstreambuffer out
		/// </summary>
		private void FlushBitStream(FileStream wrStream)
		{
			if(m_BitPos!=0)
			{
				try
				{

					byte onebyte = (byte)Bits2Byte(m_ByteBuf);
					wrStream.WriteByte(onebyte);
					m_BitPos = 0;
				}
				catch
				{
				}
			}
		}
		/// <summary>
		/// Bitstreamer
		/// </summary>
		/// <param name="wrStream">Filestream writing</param>
		/// <param name="Bit">Bit</param>
		/// <returns>success</returns>
		private bool WriteBits(FileStream wrStream,char Bit)
		{
			m_ByteBuf[m_BitPos] = Bit;
			m_BitPos++;

			if(m_BitPos==m_ByteBuf.Length)
			{
				m_BitPos = 0;
				byte onebyte = (byte)Bits2Byte(m_ByteBuf);
				try
				{
					wrStream.WriteByte(onebyte);
				}
				catch
				{
					return false;
				}
			}
			return true;
		}
		/// <summary>
		/// Reading Hamming Codeword
		/// </summary>
		/// <param name="rdStrm"></param>
		/// <param name="Bits"></param>
		/// <returns></returns>
		private bool ReadHammingCode(FileStream rdStrm,ref HammingCodeWort hcw)
		{
//			rdbyte = ReadDatei.ReadByte();
			return true;
		}
		/// <summary>
		/// converts a byte into a bitArray
		/// </summary>
		/// <param name="A">byte</param>
		/// <param name="len">fill to len</param>
		/// <returns>byteArray</returns>
		private static char[] Byte2Bits(int A,int len)
		{
			string res = "";
			if(A<0)
				res = "0";
			else
			{
				while(A>0)
				{
					int r = A % 2;
					A >>= 1;
					res = r.ToString()+ res;
				}
			}
			for(int i=res.Length;i<len;i++)
			{
				res = "0" + res;
			}

			return res.ToCharArray();
		}
		/// <summary>
		/// calculate an integer from a BitArray
		/// </summary>
		/// <param name="Byte">BitArray</param>
		/// <returns>integer</returns>
		private static int Bits2Byte(char[] Byte)
		{
			int Multipl = 1;
			int Sum = 0;
			for(int i=Byte.Length-1;i>=0;i--)
			{
				Sum += (Byte[i]=='0' ? 0 : Multipl);
				Multipl *= 2;
			}
			return Sum;
		}
		/// <summary>
		/// generate a BitArray with HammingDistance
		/// </summary>
		/// <param name="Basis">source Word</param>
		/// <param name="Dist">Hamming Distance</param>
		/// <returns>Word with HammingDistance from source</returns>
		private static char[] GenerateWordWithDistance(char[] Basis,int Dist)
		{
			char[] resWord = Byte2Bits(0,Basis.Length);
			for(int i=0;i<Basis.Length;i++)
			{
				if(i<Dist)
				{
					if(Basis[i]=='0')
						resWord[i]='1';
					else
						resWord[i]='0';
				}
				else
					resWord[i] = Basis[i];
			}
			return resWord;
		}
		/// <summary>
		/// generates a Matrix with defined hammingdistance
		/// </summary>
		/// <param name="BitLen">Code bitlen</param>
		/// <param name="HammingAbstand">minimal HammingDistance</param>
		/// <returns>HammingMatrix</returns>
		private static ArrayList GenerateHammingMatrix(int BitLen,int HammingAbstand)
		{
			ArrayList HammingMatrix = new ArrayList();
			HammingCodeWort hcw_old = new HammingCodeWort();
			int SumCodeWords = CalcPowerofTwo(BitLen);
			for(int i=0;i<SumCodeWords;i++)
			{
				HammingCodeWort hcw = new HammingCodeWort();
				hcw.CodeWort = Byte2Bits(i,BitLen);
				if(i>0)
				{
					int distCw = HammingDistance(hcw.CodeWort,hcw_old.CodeWort);
					if(distCw<HammingAbstand)
						hcw.Redundanz = GenerateWordWithDistance(hcw_old.Redundanz,HammingAbstand-distCw);
					else
						hcw.Redundanz = GenerateWordWithDistance(hcw_old.Redundanz,0);
				}
				else
				{
					// Eins abziehen, weil zwischen zwei Codewörtern immer
					// ein Hammingabstand von 1 ist
					hcw.Redundanz = Byte2Bits(0,HammingAbstand-1);
				}
				hcw_old = hcw;
				HammingMatrix.Add(hcw);
			}
			return HammingMatrix;
		}
		/// <summary>
		/// Find greatest Distance
		/// </summary>
		/// <returns>greatest Distance</returns>
		private int FindGreatesHammingCWDistance()
		{	
			int Distance = 0;
			for(int i=1;i<m_CodeWords.Count;i++)
			{
				HammingCodeWort hcw1 = (HammingCodeWort)m_CodeWords[i-1];
				HammingCodeWort hcw2 = (HammingCodeWort)m_CodeWords[i];
				int distCw = HammingDistance(hcw1.CodeWort,hcw2.CodeWort);
				int distRz = HammingDistance(hcw1.Redundanz,hcw2.Redundanz);
				if((distCw+distRz) > Distance)
					Distance = distCw+distRz;
			}
			return Distance;
		}
		/// <summary>
		/// look for the nearest codeword
		/// </summary>
		/// <param name="hcw">actual Codeword</param>
		/// <returns></returns>
		private int FindNearestCodeWordPosition(HammingCodeWort hcw)
		{
			int hcwPos = -1;
			int smallestDist = m_grtsHammingDistance;
			for(int i=0;i<m_CodeWords.Count;i++)
			{
				HammingCodeWort hcwArr = (HammingCodeWort)m_CodeWords[i];
				int distCw = HammingDistance(hcw.CodeWort,hcwArr.CodeWort);
				int distRz = HammingDistance(hcw.Redundanz,hcwArr.Redundanz);
				if((distCw+distRz)<smallestDist)
				{
					smallestDist = distCw+distRz;
					hcwPos = i;
				}
			}
			return hcwPos;
		}
		/// <summary>
		/// calculate power of 2
		/// </summary>
		/// <param name="BitLen">length of Bits</param>
		/// <returns>power of 2</returns>
		private static int CalcPowerofTwo(int BitLen)
		{
			if(BitLen<=0)
				return 0;
			int res = 1;
			for(int i=0;i<BitLen;i++)
				res = res*2;
			return res;
		}
		/// <summary>
		/// Hamming Distance between two bytes
		/// </summary>
		/// <param name="A">byteArray A</param>
		/// <param name="B">byteArray B</param>
		/// <returns>Distance</returns>
		private static int HammingDistance(char[] A,char[] B)
		{
			int res = 0;
			if(A.Length!=B.Length)
			{
				return -1;
			}
			for(int i=0;i<A.Length;i++) 
			{
				if(A[i]!=B[i])
				{
					res++;
				}
			}
			return res;
		}
		/// <summary>
		/// calculate Hamming Wight
		/// </summary>
		/// <param name="A">Codeword</param>
		/// <returns>Hammingwight</returns>
		private static int HammingWight(char[] A)
		{
			int res = 0;
			for(int i=0;i<A.Length;i++) 
			{
				if(A[i]!='0')
				{
					res++;
				}
			}
			return res;
		}
		/// <summary>
		/// HammingCodeword
		/// </summary>
		private struct HammingCodeWort
		{
			public char[] CodeWort;
			public char[] Redundanz;
		}
	}
}
