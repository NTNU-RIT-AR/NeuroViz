using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeuroViz
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UnionTagAttribute : Attribute
    {
        public String TagPropertyName { get; }

        public UnionTagAttribute(String tagPropertyName) => this.TagPropertyName = tagPropertyName;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class UnionCaseAttribute : Attribute
    {
        public Type CaseType { get; }

        public String TagPropertyValue { get; }

        public UnionCaseAttribute(Type caseType, String tagPropertyValue) =>
            (this.CaseType, this.TagPropertyValue) = (caseType, tagPropertyValue);
    }
    
    public sealed class UnionConverter<T> : JsonConverter<T> where T : class
    {
        private String TagPropertyName { get; }

        private Dictionary<String, Type> UnionTypes { get; }

        public UnionConverter()
        {
            var type = typeof(T);
            var unionTag = type.GetCustomAttribute<UnionTagAttribute>();
            if (unionTag is null) throw new InvalidOperationException();

            var concreteTypeFactory = type.CreateConcreteTypeFactory();
            this.TagPropertyName = unionTag.TagPropertyName;
            this.UnionTypes = type.GetCustomAttributes<UnionCaseAttribute>()
                .ToDictionary(k => k.TagPropertyValue, e => concreteTypeFactory(e.CaseType));
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var propertyName = options.PropertyNamingPolicy?.ConvertName(this.TagPropertyName) ?? this.TagPropertyName;
            var property = document.RootElement.GetProperty(propertyName);
            var type = this.UnionTypes[property.GetString() ?? throw new InvalidOperationException()];
            return (T?)document.ToObject(type, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    public static class TypeExtensions
    {
        public static Func<Type, Type> CreateConcreteTypeFactory(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                return givenType => givenType.MakeGenericType(genericArgs);
            }
            else
            {
                return givenType => givenType;
            }
        }
    }

    public static class JsonExtensions
    {
        public static object? ToObject(this JsonElement element, Type type, JsonSerializerOptions options)
        {
            IBufferWriter<byte> bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }
            return JsonSerializer.Deserialize(bufferWriter.GetSpan(), type, options);
        }

        public static Object? ToObject(this JsonDocument document, Type type, JsonSerializerOptions options)
        {
            if (document is null) throw new ArgumentNullException(nameof(document));
            return document.RootElement.ToObject(type, options);
        }
    }
    
    public sealed class UnionConverterFactory : JsonConverterFactory
    {
        public override Boolean CanConvert(Type typeToConvert) =>
            typeToConvert.GetCustomAttribute<UnionTagAttribute>(false) is not null;

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(UnionConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter?)Activator.CreateInstance(converterType);
        }
    }
}