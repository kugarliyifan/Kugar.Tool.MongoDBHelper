using System;
using System.Runtime.Serialization;
using System.Security;
using Kugar.Core.Exceptions;
using Kugar.Core.ExtMethod;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Kugar.Tool.MongoDBHelper
{
    /// <summary>
    /// 数字类型,用于在MongoDB中处理Decimal类型的字段,可隐式转换为Decimal
    /// </summary>
    [Serializable]
    public class Digit:IComparable,IComparable<decimal>,IEquatable<decimal>,IEquatable<Digit>,IComparable<Digit>,ISerializable
    {
        private decimal _value=0m;

        public Digit() { }

        public Digit(decimal d):this()
        {
            Value = d;
        }

        
        public Digit(SerializationInfo info, StreamingContext context)
        {
            this.Value = info.GetDecimal("Value");
        }


        /// <summary>
        /// 存储的真实定点数,代码中,请尽量使用该值
        /// </summary>
        public virtual decimal Value { set; get; }

        /// <summary>
        /// 用于MongoDB查询过程中的计算等操作
        /// </summary>
        [JsonIgnore]
        
        public double CalcValue {get { return (double) Value; } }



        public static Digit Default
        {
            get
            {
                return new Digit(0);
            }
        }


        public static implicit operator decimal (Digit d)
        {
            if (d==null)
            {
                return 0m;
            }
            return d.Value;
        }

 
        public static implicit operator Digit(decimal d)
        {
            return new Digit(d);
        }

        public static implicit operator BsonValue (Digit d)
        {
            return new BsonDocument()
            {
                {"CalcValue",d.CalcValue },
                {"Value",d.Value.ToString() }
            };
        }

        public static implicit operator Digit(BsonValue d)
        {
            if (!d.IsBsonDocument)
            {
                throw new ArgumentTypeNotMatchException("d","BsonDocument");
            }

            var doc = d.AsBsonDocument;

            if (!doc.Contains("Value")) 
            {
                throw new ArgumentException("未包含Value字段");
            }

            return new Digit(doc.GetValue("Value").AsString.ToDecimal());
        }

        public static implicit operator Digit(string str)
        {
            decimal d;

            if (decimal.TryParse(str,out d))
            {
                return new Digit(d);
            }
            else
            {
                throw new Exception("无法将指定字符串转换为数字");
            }
        }

        public static implicit operator string(Digit d)
        {
            return d.Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value",Value);
        }

        public int CompareTo(object obj)
        {
            return Value.CompareTo(obj);
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public int CompareTo(decimal other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(decimal other)
        {
            return Value.Equals(other);
        }

        public bool Equals(Digit other)
        {
            return Value.Equals(other.Value);
        }

        public int CompareTo(Digit other)
        {
            return Value.CompareTo(other.Value);
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Value.ToString();
        }
        
        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        
        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Value
                .ToString(format);
        }

        //
        // 摘要:
        //     Converts the numeric value of this instance to its equivalent string representation
        //     using the specified format and culture-specific format information.
        //
        // 参数:
        //   format:
        //     A numeric format string (see Remarks).
        //
        //   provider:
        //     An object that supplies culture-specific formatting information.
        //
        // 返回结果:
        //     The string representation of the value of this instance as specified by format
        //     and provider.
        //
        // 异常:
        //   T:System.FormatException:
        //     format is invalid.
        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Value.ToString(format, provider);
        }

        [SecuritySafeCritical]
        public string ToStringEx()
        {
            return Value.ToStringEx();
        }
    }

    public static class DigitExt
    {
        public static Digit GetDigit(this BsonDocument bsonDoc, string key)
        {
            return bsonDoc.GetValue(key);
        }
        
        public static string ToStringEx(this Digit src,string format="",string defaultValue="0.0")
        {
            if (src == null)
            {
                return defaultValue;
            }
            else
            {
                return src.Value.ToString(format);
            }
        }
    }
}
