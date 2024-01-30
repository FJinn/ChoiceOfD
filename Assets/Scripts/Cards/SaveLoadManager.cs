using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Collections;

public static class SaveLoadManager
{
    public enum SerializationType
    {
        JSON,
        Binary
    }

    public static void SaveData<T>(string fileName, T data, SerializationType serializationType = SerializationType.JSON, bool encrypt = false)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        switch (serializationType)
        {
            case SerializationType.JSON:
                SaveAsJSON(path, data, encrypt);
                break;
            case SerializationType.Binary:
                SaveAsBinary(path, data, encrypt);
                break;
            default:
                Debug.LogError("Unsupported serialization type.");
                break;
        }
    }

    public static T LoadData<T>(string fileName, SerializationType serializationType = SerializationType.JSON, bool decrypt = false)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        switch (serializationType)
        {
            case SerializationType.JSON:
                return LoadFromJSON<T>(path, decrypt);
            case SerializationType.Binary:
                return LoadFromBinary<T>(path, decrypt);
            default:
                Debug.LogError("Unsupported serialization type.");
                return default(T);
        }
    }

    private static void SaveAsJSON<T>(string path, T data, bool encrypt)
    {
        string json;

        if (data is IList)
        {
            // If data is a list, serialize the list
            json = JsonConvert.SerializeObject(data);
        }
        else
        {
            // If data is a single instance, create a list with one element and serialize it
            List<T> dataList = new List<T> { data };
            json = JsonConvert.SerializeObject(dataList);
        }

        if (encrypt)
        {
            json = Encrypt(json);
        }

        File.WriteAllText(path, json);
    }

    private static T LoadFromJSON<T>(string path, bool decrypt)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            if (decrypt)
            {
                json = Decrypt(json);
            }

            // Deserialize the JSON data into a list
            List<T> dataList = JsonConvert.DeserializeObject<List<T>>(json);

            // Return the first element if the list is not null and not empty, otherwise return the default value
            return (dataList != null && dataList.Count > 0) ? dataList[0] : default(T);
        }
        else
        {
            Debug.LogWarning("Save file not found at path: " + path);
            return default(T);
        }
    }

    private static void SaveAsBinary<T>(string path, T data, bool encrypt)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            if (data is IList)
            {
                // If data is a list, serialize the list
                formatter.Serialize(stream, data);
            }
            else
            {
                // If data is a single instance, create a list with one element and serialize it
                List<T> dataList = new List<T> { data };
                formatter.Serialize(stream, dataList);
            }
        }

        if (encrypt)
        {
            EncryptFile(path);
        }
    }

    private static T LoadFromBinary<T>(string path, bool decrypt)
    {
        if (File.Exists(path))
        {
            if (decrypt)
            {
                DecryptFile(path);
            }

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                // Deserialize the binary data into a list
                List<T> dataList = (List<T>)formatter.Deserialize(stream);

                // Return the first element if the list is not null and not empty, otherwise return the default value
                return (dataList != null && dataList.Count > 0) ? dataList[0] : default(T);
            }
        }
        else
        {
            Debug.LogWarning("Save file not found at path: " + path);
            return default(T);
        }
    }

    private static void EncryptFile(string path)
    {
        byte[] dataToEncrypt = File.ReadAllBytes(path);
        byte[] encryptedData = EncryptBytes(dataToEncrypt);

        File.WriteAllBytes(path, encryptedData);
    }

    private static void DecryptFile(string path)
    {
        byte[] encryptedData = File.ReadAllBytes(path);
        byte[] decryptedData = DecryptBytes(encryptedData);

        File.WriteAllBytes(path, decryptedData);
    }

    private static string Encrypt(string data)
    {
        byte[] dataToEncrypt = Encoding.UTF8.GetBytes(data);
        byte[] encryptedData = EncryptBytes(dataToEncrypt);

        return Convert.ToBase64String(encryptedData);
    }

    private static string Decrypt(string data)
    {
        byte[] encryptedData = Convert.FromBase64String(data);
        byte[] decryptedData = DecryptBytes(encryptedData);

        return Encoding.UTF8.GetString(decryptedData);
    }

    private static byte[] EncryptBytes(byte[] data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes("YourEncryptionKey"); // Replace with your encryption key
            aesAlg.IV = new byte[16]; // You may want to generate a random IV for each encryption

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(data, 0, data.Length);
                }
                return msEncrypt.ToArray();
            }
        }
    }

    private static byte[] DecryptBytes(byte[] data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes("YourEncryptionKey"); // Replace with your encryption key
            aesAlg.IV = new byte[16]; // Use the same IV that was used for encryption

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream())
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                {
                    csDecrypt.Write(data, 0, data.Length);
                }
                return msDecrypt.ToArray();
            }
        }
    }
}