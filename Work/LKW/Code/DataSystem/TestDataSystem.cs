using System;
using UnityEngine;
using System.IO;
using ExcelDataReader;

namespace Code.DataSystem
{
    public class TestDataSystem : MonoBehaviour
    {
        private readonly string filePath = "Assets/Work/LKW/DataSystem/Material_Table.xlsx";

        private void Start()
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    Debug.Log(reader.Name);

                    while (reader.Read())
                    {
                        Debug.Log(reader.GetString(0));
                        Debug.Log(reader.GetString(1));
                        Debug.Log(reader.GetString(2));
                    }
                }
            }
        }
    }
}