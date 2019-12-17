using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Tool.MongoDBHelper.Serializers
{
    /// <summary>
    /// Represents a base class for implementations of <see cref="Newtonsoft.Json.JsonConverter"/> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public abstract class JsonConverterBase<T> : Newtonsoft.Json.JsonConverter
    {
        // public methods
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T) || objectType.IsSubclassOf(typeof(T)) ;
        }

        // protected methods
        /// <summary>
        /// Reads the end object.
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected void ReadEndObject(Newtonsoft.Json.JsonReader reader)
        {
            ReadExpectedToken(reader, Newtonsoft.Json.JsonToken.EndObject);
        }

        /// <summary>
        /// Reads a property name and verifies that it matches the expected property name.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="expectedPropertyName">The expected property name.</param>
        protected void ReadExpectedPropertyName(Newtonsoft.Json.JsonReader reader, string expectedPropertyName)
        {
            var propertyName = ReadPropertyName(reader);
            if (propertyName != expectedPropertyName)
            {
                var message = string.Format("Expected property named '{0}', but got '{1}.", expectedPropertyName, propertyName);
                throw new Newtonsoft.Json.JsonReaderException(message);
            }
        }

        /// <summary>
        /// Reads a token and verifies that the token type matches the expected token type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="expectedTokenType">The expected token type.</param>
        protected void ReadExpectedToken(Newtonsoft.Json.JsonReader reader, Newtonsoft.Json.JsonToken expectedTokenType)
        {
            ReadToken(reader);
            if (reader.TokenType != expectedTokenType)
            {
                var message = string.Format("Expected token {0}, but got {1}.", expectedTokenType, reader.TokenType);
                throw new Newtonsoft.Json.JsonReaderException(message);
            }
        }

        /// <summary>
        /// Reads a property name.
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected string ReadPropertyName(Newtonsoft.Json.JsonReader reader)
        {
            ReadExpectedToken(reader, Newtonsoft.Json.JsonToken.PropertyName);
            return (string)reader.Value;
        }

        /// <summary>
        /// Reads a start object.
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected void ReadStartObject(Newtonsoft.Json.JsonReader reader)
        {
            ReadExpectedToken(reader, Newtonsoft.Json.JsonToken.StartObject);
        }

        /// <summary>
        /// Reads a string value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A string value.</returns>
        protected string ReadStringValue(Newtonsoft.Json.JsonReader reader)
        {
            ReadExpectedToken(reader, Newtonsoft.Json.JsonToken.String);
            return (string)reader.Value;
        }

        /// <summary>
        /// Reads a token.
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected void ReadToken(Newtonsoft.Json.JsonReader reader)
        {
            if (!reader.Read())
            {
                var message = "Expected token but none available.";
                throw new Newtonsoft.Json.JsonSerializationException(message);
            }
        }
    }
}
