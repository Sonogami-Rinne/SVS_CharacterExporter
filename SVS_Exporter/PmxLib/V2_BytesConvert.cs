using System;
using System.Collections.Generic;
using System.IO;

namespace PmxLib;

internal static class V2_BytesConvert
{
	public static readonly int UnitBytes = 8;

	public static int ByteCount => UnitBytes;

	public static byte[] ToBytes(Vector2 v2)
	{
		List<byte> list = new List<byte>();
		list.AddRange(BitConverter.GetBytes(v2.x));
		list.AddRange(BitConverter.GetBytes(v2.y));
		return list.ToArray();
	}

	public static Vector2 FromBytes(byte[] bytes, int startIndex)
	{
		int num = 4;
		return new Vector2(BitConverter.ToSingle(bytes, startIndex), BitConverter.ToSingle(bytes, startIndex + num));
	}

	public static Vector2 FromStream(Stream s)
	{
		Vector2 zero = Vector2.zero;
		byte[] array = new byte[8];
		s.Read(array, 0, 8);
		int num = 0;
		zero.x = BitConverter.ToSingle(array, num);
		int startIndex = num + 4;
		zero.y = BitConverter.ToSingle(array, startIndex);
		return zero;
	}

	public static void ToStream(Stream s, Vector2 v)
	{
		s.Write(BitConverter.GetBytes(v.x), 0, 4);
		s.Write(BitConverter.GetBytes(v.y), 0, 4);
	}
}
