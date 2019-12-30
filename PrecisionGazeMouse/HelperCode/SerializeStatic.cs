/*
 * Copyright 2019 Rishi Kapadia
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * http://www.apache.org/licenses/LICENSE-2.0|http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Soap;
using System.Reflection;
using System.IO;
using System.Diagnostics;


namespace PrecisionGazeMouse
{
    public class SerializeStatic
    {

        public static bool Save(Type static_class, string filename)
        {
            Logger.WriteFunc(3);
            try
            {
                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);
                object[,] a = new object[fields.Length, 2];
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    a[i, 0] = field.Name;
                    a[i, 1] = field.GetValue(null);
                    i++;
                };
                Stream f = File.Open(filename, FileMode.Create);
                SoapFormatter formatter = new SoapFormatter();
                formatter.Serialize(f, a);
                f.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Load(Type static_class, string filename)
        {
            Logger.WriteFunc(3);
            FieldInfo[] classFields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);
            object[,] a;
            if (File.Exists(filename))
            {
                Stream f = File.Open(filename, FileMode.Open);
                SoapFormatter formatter = new SoapFormatter();
                a = formatter.Deserialize(f) as object[,];
                f.Close();
                if (a.GetLength(0) < 1) return false; //no valid data

                //Try to restore those fields which are found even if the "Settings form" version has changed
                int i = 0;
                foreach (FieldInfo field in classFields)
                {
                    try
                    {
                        if(i < a.GetLength(0) && 1 < a.GetLength(1))
                        {
                            if (field.Name == (a[i, 0] as string))
                            {
                                field.SetValue(null, a[i, 1]);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Logger.WriteError("Cannot restore field: " + field.ToString() + "\n" + e);
                    }
                    
                    i++;
                };

                return true;
            }

            return false;

        }
    }
}
