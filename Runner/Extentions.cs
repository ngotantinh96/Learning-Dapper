using YamlDotNet.Serialization;

namespace Runner
{
    public static class Extentions
    {
        public static void Output(this object item)
        {
            var serializer = new SerializerBuilder().Build();
            var yamlResult = serializer.Serialize(item);
            Console.WriteLine(yamlResult);
        }
    }
}
