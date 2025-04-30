using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace BoosterBot
{
    internal static class SettingsHelper
    {
        public static void Save(string key, object value)
        {
            const string configFileName = "appsettings.json";
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);

            try
            {
                // 处理文件不存在的情况
                if (!File.Exists(configPath))
                {
                    File.WriteAllText(configPath, "{}");
                }

                // 安全读取文件内容
                var jsonContent = File.ReadAllText(configPath);

                // 反序列化并处理空值
                var jsonObj = JsonConvert.DeserializeObject<JObject>(jsonContent) ?? new JObject();

                // 安全更新值
                jsonObj[key] = value != null
                    ? JToken.FromObject(value)
                    : JValue.CreateNull();

                // 格式化和写入文件
                var formattedJson = jsonObj.ToString(Formatting.Indented);
                File.WriteAllText(configPath, formattedJson);

                // 强制重新加载配置
                new ConfigurationBuilder()
                    .AddJsonFile(configPath)
                    .Build()
                    .Reload();
            }
            catch (JsonException jsonEx)
            {
                HandleError("配置文件格式错误", jsonEx);
            }
            catch (IOException ioEx)
            {
                HandleError("文件访问失败", ioEx);
            }
            catch (Exception ex)
            {
                HandleError("未知配置错误", ex);
            }
        }

        private static void HandleError(string message, Exception ex)
        {
            // 这里可以添加日志记录或用户通知
            throw new ApplicationException($"{message}: {ex.Message}", ex);
        }
    }
}