﻿/*
	This file is part of Kerbal Object Inspector /L Unleashed
		© 2022 LisiasT
		© 2016-2022 linuxgurugamer
		© 2016 IRnifty

	Kerbal Object Inspector /L is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Kerbal Object Inspector /L is distributed in the hope that it will
	be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 2.0
	along with Kerbal Object Inspector /L.
	If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalObjectInspector
{
	public static class Serialization
	{
		private static Type enumType = typeof(Enum);

		public static bool CanParse(Type type)
		{
			if (enumType.IsAssignableFrom(type))
				return true;

			return parsers.ContainsKey(type);
		}

		public static bool TryDeserialize(string strValue, Type type, out object result)
		{
			if (enumType.IsAssignableFrom(type))
			{
				try
				{
					result = Enum.Parse(type, strValue);
					return true;
				}
				catch
				{
					result = default;
					return false;
				}
			}
			else if (parsers.TryGetValue(type, out ValueParser parser) && parser.DeserializeToObject(strValue, out result))
			{
				return true;
			}

			result = default;
			return false;
		}

		public static string Serialize(object value)
		{
			Type typeOfValue = value.GetType();
			if (enumType.IsAssignableFrom(typeOfValue))
			{
				return value.ToString();
			}

			if (parsers.TryGetValue(value.GetType(), out ValueParser parser))
			{
				return parser.SerializeFromObject(value);
			}

			return value.ToString();
		}

		private static Dictionary<Type, ValueParser> parsers = new Dictionary<Type, ValueParser>()
		{
			{ typeof(Enum), new EnumParser() },
			{ typeof(string), new StringParser() },
			{ typeof(bool), new BoolParser() },
			{ typeof(byte), new ByteParser() },
			{ typeof(char), new CharParser() },
			{ typeof(decimal), new DecimalParser() },
			{ typeof(double), new DoubleParser() },
			{ typeof(short), new ShortParser() },
			{ typeof(int), new IntParser() },
			{ typeof(long), new LongParser() },
			{ typeof(sbyte), new SbyteParser() },
			{ typeof(float), new FloatParser() },
			{ typeof(ushort), new UshortParser() },
			{ typeof(uint), new UintParser() },
			{ typeof(ulong), new UlongParser() },
			{ typeof(Guid), new GuidParser() },

			{ typeof(Vector2), new Vector2Parser() },
			{ typeof(Vector3), new Vector3Parser() },
			{ typeof(Vector3d), new Vector3dParser() },
			{ typeof(Vector4), new Vector4Parser() },
			{ typeof(Quaternion), new QuaternionParser() },
			{ typeof(QuaternionD), new QuaternionDParser() },
			{ typeof(Matrix4x4), new Matrix4x4Parser() },
			{ typeof(Color), new ColorParser() },
			{ typeof(Color32), new Color32Parser() },
		};

		public abstract class ValueParser
		{
			public abstract bool DeserializeToObject(string strValue, out object value);
			public abstract string SerializeFromObject(object value);
		}

		public class ValueParser<T> : ValueParser
		{
			protected Type typeOfValue;

			public ValueParser()
			{
				typeOfValue = typeof(T);
			}

			public override bool DeserializeToObject(string strValue, out object value)
			{
				if (Deserialize(strValue, out T typedValue))
				{
					value = typedValue;
					return true;
				}
				value = default(T);
				return false;
			}

			public override string SerializeFromObject(object value)
			{
				return Serialize((T)value);
			}

			public virtual string Serialize(T value) => value.ToString();

			public virtual bool Deserialize(string strValue, out T value)
			{
				try
				{
					value = (T)Convert.ChangeType(strValue, typeOfValue);
					return true;
				}
				catch
				{
					value = default;
					return false;
				}
			}


		}

		#region system types parsers

		public class EnumParser : ValueParser<object>
		{
			public override string Serialize(object value) => value.ToString();
			public override bool Deserialize(string strValue, out object value)
			{
				value = default;
				try
				{
					value = Enum.Parse(value.GetType(), strValue);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private class StringParser : ValueParser<string>
		{
			public override string Serialize(string value) => value;
			public override bool Deserialize(string strValue, out string value)
			{
				value = strValue;
				return value != null;
			}
		}

		public class BoolParser : ValueParser<bool>
		{
			public override string Serialize(bool value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class ByteParser : ValueParser<byte>
		{
			public override string Serialize(byte value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class CharParser : ValueParser<char>
		{
			public override string Serialize(char value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class DecimalParser : ValueParser<decimal>
		{
			public override string Serialize(decimal value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class DoubleParser : ValueParser<double>
		{
			public override string Serialize(double value) => value.ToString("G17", CultureInfo.InvariantCulture);
		}

		public class ShortParser : ValueParser<short>
		{
			public override string Serialize(short value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class IntParser : ValueParser<int>
		{
			public override string Serialize(int value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class LongParser : ValueParser<long>
		{
			public override string Serialize(long value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class SbyteParser : ValueParser<sbyte>
		{
			public override string Serialize(sbyte value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class FloatParser : ValueParser<float>
		{
			public override string Serialize(float value) => value.ToString("G9", CultureInfo.InvariantCulture);
		}

		public class UshortParser : ValueParser<ushort>
		{
			public override string Serialize(ushort value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class UintParser : ValueParser<uint>
		{
			public override string Serialize(uint value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class UlongParser : ValueParser<ulong>
		{
			public override string Serialize(ulong value) => value.ToString(CultureInfo.InvariantCulture);
		}

		public class GuidParser : ValueParser<Guid>
		{
			public override string Serialize(Guid value) => value.ToString("N", CultureInfo.InvariantCulture);
			public override bool Deserialize(string strValue, out Guid value)
			{
				try { value = new Guid(strValue); } // Note : .NET 4.x has a tryParse method
				catch (Exception) { value = Guid.Empty; return false; }
				return true;
			}
		}

		#endregion

		#region Unity/KSP types parsers

		public class Vector2Parser : ValueParser<Vector2>
		{
			public override bool Deserialize(string strValue, out Vector2 value) => ParseExtensions.TryParseVector2(strValue, out value);
			public override string Serialize(Vector2 value) => ConfigNode.WriteVector(value);
		}

		public class Vector3Parser : ValueParser<Vector3>
		{
			public override bool Deserialize(string strValue, out Vector3 value) => ParseExtensions.TryParseVector3(strValue, out value);
			public override string Serialize(Vector3 value) => ConfigNode.WriteVector(value);
		}

		public class Vector3dParser : ValueParser<Vector3d>
		{
			public override bool Deserialize(string strValue, out Vector3d value) => ParseExtensions.TryParseVector3d(strValue, out value);
			public override string Serialize(Vector3d value) => ConfigNode.WriteVector(value);
		}

		public class Vector4Parser : ValueParser<Vector4>
		{
			public override bool Deserialize(string strValue, out Vector4 value) => ParseExtensions.TryParseVector4(strValue, out value);
			public override string Serialize(Vector4 value) => ConfigNode.WriteVector(value);
		}

		public class QuaternionParser : ValueParser<Quaternion>
		{
			public override bool Deserialize(string strValue, out Quaternion value) => ParseExtensions.TryParseQuaternion(strValue, out value);
			public override string Serialize(Quaternion value) => ConfigNode.WriteQuaternion(value);
		}

		public class QuaternionDParser : ValueParser<QuaternionD>
		{
			public override bool Deserialize(string strValue, out QuaternionD value) => ParseExtensions.TryParseQuaternionD(strValue, out value);
			public override string Serialize(QuaternionD value) => ConfigNode.WriteQuaternion(value);
		}

		public class Matrix4x4Parser : ValueParser<Matrix4x4>
		{
			public override bool Deserialize(string strValue, out Matrix4x4 value)
			{
				value = ConfigNode.ParseMatrix4x4(strValue);
				return true;
			}
			public override string Serialize(Matrix4x4 value) => ConfigNode.WriteMatrix4x4(value);
		}

		public class ColorParser : ValueParser<Color>
		{
			public override bool Deserialize(string strValue, out Color value) => ParseExtensions.TryParseColor(strValue, out value);
			public override string Serialize(Color value) => ConfigNode.WriteColor(value);
		}

		public class Color32Parser : ValueParser<Color32>
		{
			public override bool Deserialize(string strValue, out Color32 value) => ParseExtensions.TryParseColor32(strValue, out value);
			public override string Serialize(Color32 value) => ConfigNode.WriteColor(value);
		}

		#endregion

	}
}

